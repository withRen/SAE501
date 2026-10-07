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
public static class ZombieMixamo
{
    public const string ControllerPath = "Assets/Generated/Zombie_Mixamo.controller";

    // vitesse (m/s) à laquelle le clip de marche Mixamo avance à vitesse normale : sert à caler les pieds sur le sol
    const float WalkClipSpeed = 0.55f;

    // attaque : dans le clip Mixamo (2,63 s), la main frappe à ~1,05 s (mesuré). Jouée x1,5 pour tenir dans l'intervalle
    // entre deux coups (1,5 s) : le coup tombe alors à 0,7 s, moment où Zombie applique les dégâts (attackHitDelay).
    const float AttackClipSpeed = 1.5f;
    const float AttackHitTimeInClip = 1.05f;
    public const float AttackHitDelay = AttackHitTimeInClip / AttackClipSpeed;

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

        SetupClip(walk, "Zombie_Walk", loop: true, canFall: false);
        SetupClip(attack, "Zombie_Attack", loop: false, canFall: false);
        SetupClip(death, "Zombie_Death", loop: false, canFall: true);

        return BuildController(LoadClip(walk), LoadClip(attack), LoadClip(death));
    }

    static string FindModel(string fileName) =>
        AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).Equals(fileName, StringComparison.OrdinalIgnoreCase));

    static void SetupClip(string path, string clipName, bool loop, bool canFall)
    {
        var imp = (ModelImporter)AssetImporter.GetAtPath(path);
        bool changed = false;
        if (imp.animationType != ModelImporterAnimationType.Human)
        {
            imp.animationType = ModelImporterAnimationType.Human;         // squelette Humanoid : le clip passe sur le Shirtless Zombie
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            imp.materialImportMode = ModelImporterMaterialImportMode.None; // on ne garde que le mouvement
            imp.importAnimation = true;
            imp.SaveAndReimport();
            changed = true;
        }
        if (!changed && imp.clipAnimations.Length > 0 && imp.clipAnimations[0].name == clipName) return; // déjà réglé

        var clips = imp.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.name = clipName;
            c.loopTime = loop;
            c.lockRootRotation = true;      // ne tourne pas tout seul
            c.lockRootPositionXZ = true;    // reste sur place : c'est le NavMeshAgent qui déplace le zombie
            c.lockRootHeightY = !canFall;   // la mort peut descendre jusqu'au sol
            c.keepOriginalOrientation = true;
            c.keepOriginalPositionY = true;
            c.keepOriginalPositionXZ = false;
        }
        imp.clipAnimations = clips;
        imp.SaveAndReimport();
    }

    static AnimationClip LoadClip(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));

    static AnimatorController BuildController(AnimationClip walk, AnimationClip attack, AnimationClip death)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");

        // on réutilise le fichier s'il existe (même identifiant : pas de changements inutiles dans git)
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath)
                   ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
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
        sWalk.speed = 1f / WalkClipSpeed;
        sWalk.speedParameterActive = true;
        sWalk.speedParameter = "Speed";

        var sAttack = sm.AddState("Attack", new Vector3(300, 120, 0));
        sAttack.motion = attack;
        sAttack.speed = AttackClipSpeed;

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
