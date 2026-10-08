using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Vraies animations du zombie (Mixamo) : marche, attaque, mort.
// Les 3 fichiers "Zombie Walk.fbx", "Zombie Attack.fbx", "Zombie Death.fbx" (rangés dans Assets/Animations/Zombie)
// sont réglés en Humanoid puis mis dans un Animator Controller piloté par ZombieAnimationBridge :
//   Speed (float) = vitesse réelle en m/s, Attack (trigger), Die (trigger).
// Appelé par ZombieBuilder.BuildShirtless : si les 3 fichiers sont absents, on garde l'animation par script (ZombieBoneAnim).
// Types de zombie : chaque type (Variant) a sa marche et son attaque, rangées dans Assets/Animations/<dossier>.
//   La mort est la même pour tous (« Zombie Death »). Un type dont les fichiers manquent est simplement ignoré.
public static class ZombieMixamo
{
    public const string ControllerPath = "Assets/Generated/Zombie_Mixamo.controller";

    // vitesse (m/s) de référence du clip de marche : à cette vitesse du zombie, le clip est joué à vitesse normale.
    // (le clip Mixamo avance en réalité de 0,32 m/s : 1,29 m en 4,03 s ; avec 0,55, à 0,9 m/s les pieds glissent un peu
    //  mais la marche n'est pas accélérée de façon grotesque — à ajuster au casque)
    const float WalkClipSpeed = 0.55f;

    // attaque : dans le clip Mixamo (2,63 s), la main frappe à ~1,05 s (mesuré). Jouée x1,5 pour tenir dans l'intervalle
    // entre deux coups (1,5 s) : le coup tombe alors à 0,7 s, moment où Zombie applique les dégâts (attackHitDelay).
    const float AttackClipSpeed = 1.5f;
    const float AttackHitTimeInClip = 1.05f;
    public const float AttackHitDelay = AttackHitTimeInClip / AttackClipSpeed;

    // ---------------- types de zombie (animations différentes) ----------------
    public class Variant
    {
        public string id;               // nom du type, sert aux noms des fichiers générés
        public string walk, attack;     // chemins des fichiers .fbx
        public bool walkLoops = true;
        public float walkClipSpeed;     // vitesse (m/s) à laquelle le clip de marche est joué à vitesse normale
        public float attackClipSpeed;   // vitesse de lecture du clip d'attaque
        public float attackHitTime;     // moment du coup dans le clip (s, mesuré)
        public string ControllerPath => $"Assets/Generated/Zombie_{id}.controller";
        public float AttackHitDelay => attackHitTime / attackClipSpeed;
    }

    // Zombie « cogneur » : marche lourde, coups de poing.
    //   Walking : 1,53 s, avance de 0,85 m (0,55 m/s). Zombie Punching : 1,03 s, 1er poing en avant à 0,33 s (mesuré).
    public static readonly Variant Cogneur = new Variant
    {
        id = "Cogneur", walk = "Assets/Animations/Zombie2/Walking.fbx", attack = "Assets/Animations/Zombie2/Zombie Punching.fbx",
        walkClipSpeed = 0.7f, attackClipSpeed = 1f, attackHitTime = 0.33f,
    };

    // Zombie « coureur » : il court, et frappe d'un coup de pied.
    //   Zombie Running : 0,8 s, avance de 2,5 m (3,1 m/s : un vrai sprint, joué plus lentement car le zombie va moins vite).
    //   Zombie Kicking : 3,5 s, pied le plus haut et le plus en avant à 1,23 s (mesuré). Joué x2,3 : le coup tombe à 0,53 s
    //   et le clip (1,5 s) tient dans l'intervalle entre deux coups.
    public static readonly Variant Coureur = new Variant
    {
        id = "Coureur", walk = "Assets/Animations/Zombie3/Zombie Running.fbx", attack = "Assets/Animations/Zombie3/Zombie Kicking.fbx",
        walkClipSpeed = 2f, attackClipSpeed = 2.3f, attackHitTime = 1.23f,
    };

    /// <summary>Contrôleur d'un type de zombie, ou null si ses fichiers ne sont pas dans le projet.</summary>
    public static RuntimeAnimatorController EnsureController(Variant v)
    {
        string death = FindModel("Zombie Death");
        if (death == null || AssetImporter.GetAtPath(v.walk) == null || AssetImporter.GetAtPath(v.attack) == null) return null;

        SetupClip(v.walk, $"Zombie_{v.id}_Walk", loop: v.walkLoops);
        SetupClip(v.attack, $"Zombie_{v.id}_Attack", loop: false);
        SetupClip(death, "Zombie_Death", loop: false);

        return BuildController(v.ControllerPath, LoadClip(v.walk), LoadClip(v.attack), LoadClip(death),
                               v.walkClipSpeed, v.attackClipSpeed);
    }

    [MenuItem("SAE501/Zombie/Régler les animations Mixamo")]
    static void Menu()
    {
        var c = EnsureController();
        if (c == null)
            EditorUtility.DisplayDialog("Zombie", "Animations Mixamo introuvables.\nIl faut « Zombie Walk.fbx », « Zombie Attack.fbx » et « Zombie Death.fbx » dans Assets/Animations/Zombie.", "OK");
        else
        {
            Selection.activeObject = c;
            Debug.Log($"Zombie : animations Mixamo prêtes ({ControllerPath}). Lance « Reconstruire le prefab (modèle Shirtless Zombie) » ou « Build VR Scene ».");
        }
    }

    /// <summary>Renvoie le contrôleur des animations Mixamo, ou null si les 3 fichiers ne sont pas dans le projet.</summary>
    public static RuntimeAnimatorController EnsureController()
    {
        string walk = FindModel("Zombie Walk"), attack = FindModel("Zombie Attack"), death = FindModel("Zombie Death");
        if (walk == null || attack == null || death == null) return null;

        SetupClip(walk, "Zombie_Walk", loop: true);
        SetupClip(attack, "Zombie_Attack", loop: false);
        SetupClip(death, "Zombie_Death", loop: false);

        return BuildController(ControllerPath, LoadClip(walk), LoadClip(attack), LoadClip(death), WalkClipSpeed, AttackClipSpeed);
    }

    static string FindModel(string fileName) =>
        AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).Equals(fileName, StringComparison.OrdinalIgnoreCase));

    static void SetupClip(string path, string clipName, bool loop)
    {
        var imp = (ModelImporter)AssetImporter.GetAtPath(path);
        if (imp.animationType != ModelImporterAnimationType.Human)
        {
            imp.animationType = ModelImporterAnimationType.Human;         // squelette Humanoid : le clip passe sur le Shirtless Zombie
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            imp.materialImportMode = ModelImporterMaterialImportMode.None; // on ne garde que le mouvement
            imp.importAnimation = true;
            imp.SaveAndReimport();
        }

        var clips = imp.clipAnimations.Length > 0 ? imp.clipAnimations : imp.defaultClipAnimations;
        bool dirty = imp.clipAnimations.Length == 0;
        foreach (var c in clips)
        {
            // Les clips Mixamo ne sont pas « sur place » : la marche fait avancer le bassin de 1,29 m en 4 s.
            // Si ce déplacement est intégré à la pose (lockRootPositionXZ = vrai), le corps s'éloigne pendant le cycle
            // puis revient d'un coup au début de la boucle (la marche « se recharge »). On l'extrait donc en root motion,
            // que l'Animator ignore (applyRootMotion = faux) : le zombie marche sur place, c'est le NavMeshAgent qui avance.
            dirty |= c.name != clipName || c.loopTime != loop || !c.lockRootRotation || c.lockRootPositionXZ || !c.lockRootHeightY
                     || !c.keepOriginalOrientation || !c.keepOriginalPositionY || c.keepOriginalPositionXZ;
            c.name = clipName;
            c.loopTime = loop;
            c.lockRootRotation = true;      // l'orientation reste dans la pose : le zombie ne tourne pas tout seul
            c.lockRootPositionXZ = false;   // déplacement horizontal extrait (puis ignoré) : pas de saut en fin de boucle
            c.lockRootHeightY = true;       // hauteur gardée dans la pose : à la mort le corps descend vraiment jusqu'au sol
                                            // (extraite en root motion, elle serait ignorée et le corps resterait à hauteur de hanche)
            c.keepOriginalOrientation = true;
            c.keepOriginalPositionY = true;
            c.keepOriginalPositionXZ = false;
        }
        if (!dirty) return;                 // déjà réglé
        imp.clipAnimations = clips;
        imp.SaveAndReimport();
    }

    static AnimationClip LoadClip(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));

    static AnimatorController BuildController(string controllerPath, AnimationClip walk, AnimationClip attack, AnimationClip death,
                                              float walkClipSpeed, float attackClipSpeed)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");

        // on réutilise le fichier s'il existe (même identifiant : pas de changements inutiles dans git)
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)
                   ?? AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var sm = ctrl.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions) sm.RemoveAnyStateTransition(t);
        foreach (var s in sm.states) sm.RemoveState(s.state);
        foreach (var p in ctrl.parameters) ctrl.RemoveParameter(p);

        // mêmes noms que ZombieAnimationBridge
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        // Marche : la vitesse du clip suit la vitesse réelle (Speed en m/s). À l'arrêt (Speed = 0) le zombie se fige.
        var sWalk = sm.AddState("Walk", new Vector3(300, 0, 0));
        sWalk.motion = walk;
        sWalk.speed = 1f / walkClipSpeed;
        sWalk.speedParameterActive = true;
        sWalk.speedParameter = "Speed";

        var sAttack = sm.AddState("Attack", new Vector3(300, 120, 0));
        sAttack.motion = attack;
        sAttack.speed = attackClipSpeed;

        var sDeath = sm.AddState("Death", new Vector3(600, 60, 0));
        sDeath.motion = death;
        sm.defaultState = sWalk;

        // Marche -> Attaque (à chaque coup, planche ou joueur), puis retour à la marche à la fin du clip
        var t1 = sWalk.AddTransition(sAttack);
        t1.hasExitTime = false; t1.duration = 0.15f;
        t1.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        var t2 = sAttack.AddTransition(sWalk);
        t2.hasExitTime = true; t2.exitTime = 0.95f; t2.duration = 0.2f;
        // nouveau coup pendant l'attaque : on la relance
        var t3 = sAttack.AddTransition(sAttack);
        t3.hasExitTime = false; t3.duration = 0.1f;
        t3.AddCondition(AnimatorConditionMode.If, 0, "Attack");

        // Mort depuis n'importe quel état
        var td = sm.AddAnyStateTransition(sDeath);
        td.hasExitTime = false; td.duration = 0.1f; td.canTransitionToSelf = false;
        td.AddCondition(AnimatorConditionMode.If, 0, "Die");

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        return ctrl;
    }
}
