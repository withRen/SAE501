# Cinématique de fin (victoire en mode Évasion) rendue dans Blender, jouée en vidéo par le jeu (EscapeVideo.cs).
# Usage :
#   /Applications/Blender.app/Contents/MacOS/Blender -b ~/Downloads/map_final_final.blend --python Tools/blender/cinematique_fin.py -- <dossier_sortie> [apercu]
# Avant : dans Unity, SAE501 > Cinématique > Exporter les poses pour Blender (écrit Tools/blender/cinematique/*.json).
# Le .blend de la carte n'est pas modifié : la scène montée est enregistrée dans <dossier_sortie>/cinematique_fin.blend,
# les images dans <dossier_sortie>/images/, puis ffmpeg en fait un MP4 (voir la fin du fichier).
#
# Déroulé (environ 20 s, 30 images/s) :
#   Plan A (0 - 6,5 s)  : depuis la route, dehors. Le survivant traverse le jardin en courant, passe la grille, qui claque derrière lui ;
#                         les zombies qui le poursuivent arrivent contre la grille.
#   Plan B (6,5 - 14 s) : depuis le jardin, derrière les zombies. Ils frappent la grille fermée pendant que le survivant
#                         s'éloigne sur la route et disparaît dans la nuit.
#   Plan C (14 - 20 s)  : gros plan dehors, face à la grille : les zombies tapent contre les barreaux, la grille tremble. Fondu au noir.
#
# Les animations viennent du jeu : Unity recale les clips Mixamo sur les squelettes (avatar Humanoid) et CinematicPoseExport.cs
# écrit, image par image, la position et la rotation de chaque os dans le monde Unity. Ici on les remet sur les mêmes modèles FBX.
import bpy, json, math, os, sys, random
import numpy as np
from mathutils import Matrix, Vector, Quaternion

ARGS = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
OUT = os.path.abspath(ARGS[0]) if ARGS else os.path.abspath("cinematique_out")
PREVIEW = len(ARGS) > 1 and ARGS[1] == "apercu"        # petite image, peu d'échantillons : pour régler les plans
HERE = os.path.dirname(os.path.abspath(__file__))
POSES = os.path.join(HERE, "cinematique")
FPS = 30
random.seed(7)

scene = bpy.context.scene
scene.render.fps = FPS

def sec(t): return 1 + int(round(t * FPS))

# ---------------- repère Unity -> Blender ----------------
# Unity (x, y, z) = Blender (-x, z, -y), donc Blender = (-xu, -zu, yu)
M = Matrix(((-1, 0, 0), (0, 0, -1), (0, 1, 0)))

def kabsch(src, dst):
    # rotation R (3x3) qui minimise |R src - dst| (nuages de points centrés)
    a = np.array(src); b = np.array(dst)
    a = a - a.mean(0); b = b - b.mean(0)
    u, s, vt = np.linalg.svd(a.T @ b)
    d = np.sign(np.linalg.det(vt.T @ u.T))
    r = vt.T @ np.diag([1, 1, d]) @ u.T
    return Matrix(r.tolist())

# ---------------- personnages ----------------
_model_cache = {}

def import_model(model_id):
    """Importe le FBX du modèle (une fois), l'habille avec les textures du jeu ; renvoie l'armature (modèle source à dupliquer)."""
    if model_id in _model_cache: return _model_cache[model_id]
    info = json.load(open(os.path.join(POSES, model_id + "_modele.json")))
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=info["fbx"], automatic_bone_orientation=False, ignore_leaf_bones=False)
    new = [o for o in bpy.data.objects if o not in before]
    arm = next(o for o in new if o.type == "ARMATURE")
    # LOD1, LOD2 : inutiles pour un rendu
    for o in new:
        if o.type == "MESH" and ("_LOD1" in o.name or "_LOD2" in o.name or "_LOD3" in o.name):
            bpy.data.objects.remove(o, do_unlink=True)
    new = [o for o in bpy.data.objects if o in set(new) or o not in before]
    # matériaux du jeu : même nom d'objet que dans Unity, un matériau Unity par emplacement (dans l'ordre)
    for o in [o for o in bpy.data.objects if o not in before and o.type == "MESH"]:
        mine = [m for m in info["materials"] if m["renderer"] == o.name.split(".")[0]]
        for i, slot in enumerate(o.material_slots):
            if i < len(mine): slot.material = make_material(model_id + "_" + mine[i]["material"], mine[i])
    _model_cache[model_id] = arm
    return arm

_mat_cache = {}
def make_material(name, m):
    if name in _mat_cache: return _mat_cache[name]
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nt = mat.node_tree
    bsdf = nt.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (*m["color"], 1)
    bsdf.inputs["Roughness"].default_value = 0.75
    if "glass" in m["material"].lower():
        bsdf.inputs["Roughness"].default_value = 0.08
        bsdf.inputs["Base Color"].default_value = (0.03, 0.035, 0.04, 1)
    if m["base"] and os.path.exists(m["base"]):
        tex = nt.nodes.new("ShaderNodeTexImage")
        tex.image = bpy.data.images.load(m["base"], check_existing=True)
        nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    if m["normal"] and os.path.exists(m["normal"]):
        nt_ = nt.nodes.new("ShaderNodeTexImage")
        nt_.image = bpy.data.images.load(m["normal"], check_existing=True)
        nt_.image.colorspace_settings.name = "Non-Color"
        nm = nt.nodes.new("ShaderNodeNormalMap")
        nt.links.new(nt_.outputs["Color"], nm.inputs["Color"])
        nt.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
    _mat_cache[name] = mat
    return mat

def duplicate(src_arm, name):
    """Copie d'un personnage (armature + maillages), avec sa propre animation."""
    arm = src_arm.copy(); arm.data = src_arm.data; arm.name = name
    arm.animation_data_clear()
    scene.collection.objects.link(arm)
    for child in src_arm.children:
        c = child.copy(); c.parent = arm
        for mod in c.modifiers:
            if mod.type == "ARMATURE": mod.object = arm
        scene.collection.objects.link(c)
    return arm

def bone_order(arm):
    out = []
    def walk(b):
        out.append(b)
        for c in b.children: walk(c)
    for b in arm.data.bones:
        if b.parent is None: walk(b)
    return out

_actions = {}
FACING = {}      # direction du regard du modèle au repos (degrés autour de Z, 0 = +X)
def clip_action(arm, model_id, clip_id):
    """Action Blender (un cycle du clip, sur place) + vitesse d'avance du clip (m/s)."""
    key = (model_id, clip_id)
    if key in _actions: return _actions[key]
    d = json.load(open(os.path.join(POSES, f"{model_id}_{clip_id}.json")))
    names = d["bones"]; rest = d["rest"]; frames = d["frames"]
    A0 = arm.matrix_world.copy()
    A0r = A0.to_3x3().normalized()
    A0i = A0.inverted()
    pbones = {b.name: b for b in arm.data.bones}
    # recalage Unity -> Blender : on compare la pose de repos des deux côtés (positions des têtes d'os)
    common = [i for i, n in enumerate(names) if n in pbones]
    src = [M @ Vector(rest[i][:3]) for i in common]
    dst = [A0 @ pbones[names[i]].head_local for i in common]
    K = kabsch(src, dst)
    MK = K @ M
    cs = sum((MK @ Vector(rest[i][:3]) for i in common), Vector()) / len(common)
    cd = sum(dst, Vector()) / len(common)
    err = max(((MK @ Vector(rest[i][:3]) - cs) - (A0 @ pbones[names[i]].head_local - cd)).length for i in common)
    print(f"[cinematique] {model_id}_{clip_id} : {len(common)} os communs, écart de repos max {err*100:.1f} cm")

    def conv_rot(q):   # rotation Unity (x, y, z, w) -> matrice 3x3 dans le repère Blender
        r = Quaternion((q[6], q[3], q[4], q[5])).to_matrix()
        return MK @ r @ MK.transposed()

    rest_rot = {names[i]: conv_rot(rest[i]) for i in common}
    order = [b for b in bone_order(arm) if b.name in rest_rot]
    root0 = Vector(frames[0][0][:3])
    act = bpy.data.actions.new(f"{model_id}_{clip_id}")
    if hasattr(act, "id_root"): act.id_root = "OBJECT"
    arm.animation_data_create()
    arm.animation_data.action = act
    for fi, fr in enumerate(frames):
        drift = Vector(fr[0][:3]) - root0
        drift.y = 0          # on garde la hauteur (sauts, chutes), on retire l'avance au sol : le cycle est joué sur place
        pose_arm = {}
        for b in order:
            i = names.index(b.name)
            p = Vector(fr[i][:3]) - drift
            Rw = conv_rot(fr[i]) @ rest_rot[b.name].transposed() @ (A0r @ b.matrix_local.to_3x3())
            Rw = Rw.normalized()
            pw = MK @ p - cs + cd
            m = (A0r.transposed() @ Rw).to_4x4()
            m.translation = A0i @ pw
            pose_arm[b.name] = m
        for b in order:
            pb = arm.pose.bones[b.name]
            if b.parent and b.parent.name in pose_arm:
                rel = b.parent.matrix_local.inverted() @ b.matrix_local
                basis = rel.inverted() @ pose_arm[b.parent.name].inverted() @ pose_arm[b.name]
            else:
                basis = b.matrix_local.inverted() @ pose_arm[b.name]
            pb.matrix_basis = basis
            pb.keyframe_insert("location", frame=fi + 1, group=b.name)
            pb.keyframe_insert("rotation_quaternion", frame=fi + 1, group=b.name)
    arm.animation_data.action = None
    move = (Vector(frames[-1][0][:3]) - root0); move.y = 0
    speed = move.length / max(d["length"], 1e-3)
    fwd = MK @ Vector((0, 0, 1))
    FACING[model_id] = math.degrees(math.atan2(fwd.y, fwd.x))
    _actions[key] = (act, len(frames), speed)
    return _actions[key]

def nla(arm, model_id, segments):
    """segments : liste (clip, début s, fin s, vitesse de lecture). Les cycles se répètent jusqu'à la fin du segment ;
    chaque segment a sa piste, au-dessus de la précédente, avec un court fondu d'entrée (passage de la course aux coups)."""
    arm.animation_data_create()
    for k, (clip, t0, t1, rate) in enumerate(segments):
        act, n, _ = clip_action(arm, model_id, clip)
        track = arm.animation_data.nla_tracks.new()
        strip = track.strips.new(f"{clip}_{k}", sec(t0), act)
        strip.scale = 1.0 / rate
        strip.repeat = max(0.1, (sec(t1) - sec(t0)) / ((n - 1) * strip.scale))
        strip.extrapolation = "HOLD_FORWARD" if k == 0 else "HOLD_FORWARD"
        if k > 0:
            strip.blend_in = 6

def linear(obj):
    ad = obj.animation_data
    if not ad or not ad.action: return
    for fc in fcurves(ad.action):
        for kp in fc.keyframe_points: kp.interpolation = "LINEAR"

def fcurves(action):
    if hasattr(action, "fcurves") and action.fcurves is not None and len(action.fcurves): return action.fcurves
    out = []
    for layer in getattr(action, "layers", []):
        for strip in layer.strips:
            for cb in strip.channelbags: out.extend(cb.fcurves)
    return out

def ground_z(x, y):
    """Hauteur du sol (terrain vallonné du jardin) sous (x, y)."""
    hit = scene.ray_cast(bpy.context.evaluated_depsgraph_get(), Vector((x, y, 1.5)), Vector((0, 0, -1)))   # sous les arbres et l'arche de la grille
    return hit[1].z if hit[0] else 0.0

def carrier(name, keys, model_id, heading=180.0):
    """Empty qui porte un personnage le long de son chemin (posé sur le sol), tourné vers heading (degrés, 180 = vers -X : la route)."""
    yaw = heading - FACING[model_id]
    e = bpy.data.objects.new(name, None)
    scene.collection.objects.link(e)
    e.rotation_mode = "XYZ"
    for t, x, y in keys:
        e.location = (x, y, ground_z(x, y))
        e.rotation_euler = (0, 0, math.radians(yaw))
        e.keyframe_insert("location", frame=sec(t))
        e.keyframe_insert("rotation_euler", frame=sec(t))
    linear(e)
    return e

def attach(arm, parent):
    mw = arm.matrix_world.copy()
    arm.parent = parent
    arm.matrix_parent_inverse = Matrix.Identity(4)
    arm.matrix_basis = mw

# ---------------- décor : la grille ----------------
GATE_X = -41.35
leaf_l = bpy.data.objects["Garden_ExitGate_Left"]     # charnière en y = -7,12, le battant va vers +Y
leaf_r = bpy.data.objects["Garden_ExitGate_Right"]    # charnière en y = -4,48, le battant va vers -Y
lock = bpy.data.objects["Garden_ExitGate_Padlock"]
lock.parent = leaf_l
lock.matrix_parent_inverse = leaf_l.matrix_world.inverted()

T_SLAM = 3.25          # la grille claque derrière le survivant
OPEN = 95              # ouverte vers le jardin (+X) : les zombies la poussent ensuite contre ses butées
def gate_key(t, angle_l, angle_r):
    leaf_l.rotation_euler = (0, 0, math.radians(-angle_l)); leaf_l.keyframe_insert("rotation_euler", frame=sec(t))
    leaf_r.rotation_euler = (0, 0, math.radians(angle_r)); leaf_r.keyframe_insert("rotation_euler", frame=sec(t))
gate_key(0, OPEN, OPEN)
gate_key(T_SLAM, OPEN, OPEN)
gate_key(T_SLAM + 0.35, -2, 2)        # claque, rebondit un peu
gate_key(T_SLAM + 0.5, 0.6, -0.6)
gate_key(T_SLAM + 0.65, 0, 0)
# coups des zombies : la grille tremble (petits chocs irréguliers) jusqu'à la fin
t = 5.4
HITS = []              # instants des coups (bande-son : cinematique_son.py)
while t < 20.5:
    HITS.append(round(t, 3))
    a = random.uniform(0.8, 2.2)
    gate_key(t, -a, a * random.uniform(0.6, 1.2))
    gate_key(t + 0.08, a * 0.3, -a * 0.25)
    gate_key(t + 0.2, 0, 0)
    t += random.uniform(0.28, 0.6)
for o in (leaf_l, leaf_r):
    for fc in fcurves(o.animation_data.action):
        for kp in fc.keyframe_points: kp.interpolation = "BEZIER"

# ---------------- le survivant ----------------
surv_src = import_model("survivant")
surv = surv_src
RUN = 0.8 if os.environ.get("SURV_CLIP", "sprint") == "course" else 1.05   # « sprint » (Mixamo, 3,1 m/s) à 105 % : 3,2 m/s ; « course » (4,1 m/s, bras tendus de zombie) à 80 %
SURV_CLIP = os.environ.get("SURV_CLIP", "sprint")
_, _, run_speed = clip_action(surv, "survivant", SURV_CLIP)
v = run_speed * RUN
x0, y0 = -33.0, -5.6            # sort de l'allée du jardin
x_end = -70.0
t_gate = (x0 - GATE_X) / v      # passage de la grille
t_end = t_gate + 2.2 + (GATE_X - 7.0 - x_end) / v
s_car = carrier("Survivant_Chemin", [(0, x0, y0), (t_gate, GATE_X, -5.85), (t_gate + 2.2, GATE_X - 7.0, -4.45), (t_end, x_end, -6.2)], "survivant")
attach(surv, s_car)
nla(surv, "survivant", [(SURV_CLIP, 0, t_end + 1, RUN)])
print(f"[cinematique] survivant : {v:.2f} m/s, passe la grille à {t_gate:.2f} s, au bout de la route à {t_end:.2f} s")

# ---------------- les zombies ----------------
zsrc = {"zombie": import_model("zombie"), "cogneur": import_model("cogneur")}
for a in zsrc.values(): a.location = (0, 0, -50)     # modèles sources rangés sous la carte
bpy.context.view_layer.update()
# (modèle, y contre la grille, arrivée contre la grille (s), coup : clip et vitesse). Ils courent tous dès le début,
# partis d'autant plus loin dans le jardin qu'ils arrivent tard.
ZOMBIES = [
    ("zombie",  -6.35, 4.9, "attaque", 1.5),
    ("cogneur", -5.25, 5.3, "poings", 1.0),
    ("zombie",  -4.85, 5.9, "poings", 0.9),
    ("cogneur", -6.85, 6.6, "attaque", 1.3),
    ("zombie",  -5.85, 7.6, "attaque", 1.6),
]
for k, (mid, zy, t_arrive, hit, hit_rate) in enumerate(ZOMBIES):
    arm = duplicate(zsrc[mid], f"Zombie_{k}")
    arm.location = (0, 0, 0)
    bpy.context.view_layer.update()
    _, _, zspeed = clip_action(arm, mid, "course")
    rate = 0.85 + random.uniform(-0.05, 0.1)
    zv = zspeed * rate
    stop_x = GATE_X + 0.42 + (0.3 if k == 4 else 0.0)
    zx = stop_x + zv * t_arrive
    zy0 = y0 + random.uniform(-0.8, 0.8)
    car = carrier(f"Zombie_{k}_Chemin", [(0, zx, zy0), (t_arrive, stop_x, zy), (25, stop_x, zy)], mid)
    attach(arm, car)
    nla(arm, mid, [("course", 0, t_arrive + 0.1, rate), (hit, t_arrive, 25, hit_rate * random.uniform(0.9, 1.1))])
    print(f"[cinematique] zombie {k} ({mid}) part de x = {zx:.1f}, contre la grille à {t_arrive:.2f} s")

# ---------------- lumière ----------------
# lune froide et rasante depuis la route, plus une lampe chaude au-dessus de la grille (déjà dans la carte) : on la renforce un peu
moon = bpy.data.lights.new("Lune_Cinematique", "SUN")
moon.energy = 1.0
moon.color = (0.6, 0.7, 1.0)
moon.angle = math.radians(1.5)
mo = bpy.data.objects.new("Lune_Cinematique", moon)
scene.collection.objects.link(mo)
mo.rotation_euler = (math.radians(58), 0, math.radians(-115))
gl = bpy.data.objects.get("Light_Garden_ExitGate")
if gl: gl.data.energy *= 1.6
rim = bpy.data.lights.new("Contre_Jour", "AREA")
rim.energy = 120; rim.color = (0.55, 0.65, 1.0); rim.size = 3
ro = bpy.data.objects.new("Contre_Jour", rim)
scene.collection.objects.link(ro)
ro.location = (-36.5, -5.8, 4.2)
ro.rotation_euler = (math.radians(-60), 0, math.radians(90))
# dehors, la route n'a aucune lampe dans la carte : un vieux lampadaire (projecteur) éclaire la grille et le début de la route
for name, loc, energy, size, aim in (("Lampadaire_Route", (-45.2, -7.9, 4.3), 900, 75, (GATE_X - 0.5, -5.8, 0.6)),
                                     ("Lampadaire_Loin", (-53.0, -7.6, 4.0), 500, 80, (-55.0, -6.3, 0.0))):
    sl = bpy.data.lights.new(name, "SPOT")
    sl.energy = energy; sl.color = (0.75, 0.82, 1.0); sl.spot_size = math.radians(size); sl.spot_blend = 0.6
    sl.shadow_soft_size = 0.4
    so = bpy.data.objects.new(name, sl)
    scene.collection.objects.link(so)
    so.location = loc
    tr = so.constraints.new("TRACK_TO")
    aim_o = bpy.data.objects.new(name + "_Cible", None); scene.collection.objects.link(aim_o); aim_o.location = aim
    tr.target = aim_o; tr.track_axis = "TRACK_NEGATIVE_Z"; tr.up_axis = "UP_Y"

# ---------------- caméras ----------------
def camera(name, loc, target, lens, t0, t1, end_loc=None):
    cd = bpy.data.cameras.new(name)
    cd.lens = lens
    cd.clip_start = 0.05; cd.clip_end = 200
    cd.dof.use_dof = True
    cd.dof.aperture_fstop = 2.8
    co = bpy.data.objects.new(name, cd)
    scene.collection.objects.link(co)
    tgt = bpy.data.objects.new(name + "_Cible", None)
    scene.collection.objects.link(tgt)
    tgt.location = target
    c = co.constraints.new("TRACK_TO"); c.target = tgt; c.track_axis = "TRACK_NEGATIVE_Z"; c.up_axis = "UP_Y"
    cd.dof.focus_object = tgt
    co.location = loc
    co.keyframe_insert("location", frame=sec(t0))
    if end_loc:
        co.location = end_loc
        co.keyframe_insert("location", frame=sec(t1))
    m = scene.timeline_markers.new(name, frame=sec(t0))
    m.camera = co
    return co, tgt

SHOT_B, SHOT_C, END = 6.5, 14.0, 20.0
camA, tgtA = camera("Plan_A_Route", (-49.4, -6.15, 1.5), (GATE_X, -5.8, 1.25), 30, 0, SHOT_B, end_loc=(-50.0, -6.2, 1.45))
# la cible du plan A suit le survivant quand il approche, puis revient sur la grille
tgtA.keyframe_insert("location", frame=sec(0))
tgtA.location = (GATE_X - 1.0, -5.85, 1.35); tgtA.keyframe_insert("location", frame=sec(t_gate))
tgtA.location = (GATE_X - 3.0, -5.9, 1.3); tgtA.keyframe_insert("location", frame=sec(t_gate + 1.2))
tgtA.location = (GATE_X, -5.8, 1.3); tgtA.keyframe_insert("location", frame=sec(5.6))
camB, tgtB = camera("Plan_B_Jardin", (-36.0, -5.3, 2.35), (-54.0, -6.0, 0.9), 42, SHOT_B, SHOT_C, end_loc=(-36.8, -5.45, 2.3))
# la cible du plan B suit le survivant qui s'éloigne sur la route
tgtB.keyframe_insert("location", frame=sec(SHOT_B))
tgtB.location = (-62.0, -6.2, 0.9); tgtB.keyframe_insert("location", frame=sec(SHOT_C))
camC, tgtC = camera("Plan_C_Barreaux", (-43.9, -5.55, 1.45), (GATE_X + 0.5, -5.85, 1.5), 32, SHOT_C, END, end_loc=(-43.1, -5.6, 1.5))
scene.camera = camA

# ---------------- fondus (exposition de la vue) ----------------
vs = scene.view_settings
base_exp = vs.exposure
def exp_key(t, e):
    vs.exposure = e; vs.keyframe_insert("exposure", frame=sec(t))
exp_key(0, base_exp - 8); exp_key(0.8, base_exp)
exp_key(END - 1.6, base_exp); exp_key(END, base_exp - 10)

# ---------------- rendu ----------------
scene.frame_start = 1
scene.frame_end = sec(END)
r = scene.render
r.engine = "BLENDER_EEVEE" if "BLENDER_EEVEE" in [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items] else "BLENDER_EEVEE_NEXT"
r.resolution_x, r.resolution_y = (960, 540) if PREVIEW else (1920, 1080)
r.resolution_percentage = 100
r.image_settings.file_format = "PNG"
r.filepath = os.path.join(OUT, "images", "f_")
ee = scene.eevee
for attr, val in (("taa_render_samples", 16 if PREVIEW else 32), ("use_shadows", True), ("use_raytracing", False),
                  ("use_volumetric_shadows", False)):
    if hasattr(ee, attr): setattr(ee, attr, val)
r.use_motion_blur = not PREVIEW
os.makedirs(OUT, exist_ok=True)
# instants utiles à la bande-son (cinematique_son.py) : claquement de la grille, coups des zombies, passage du survivant, plans
json.dump({"slam": T_SLAM + 0.35, "hits": HITS, "gate": t_gate, "run_speed": v, "end": END, "shots": [0, SHOT_B, SHOT_C],
           "zombies": [z[2] for z in ZOMBIES]}, open(os.path.join(OUT, "temps.json"), "w"), indent=1)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "cinematique_fin.blend"), copy=True)
print("[cinematique] scène prête :", scene.frame_end, "images, moteur", r.engine)
