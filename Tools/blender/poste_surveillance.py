# Modèles détaillés du poste de surveillance (remplacent les boîtes de la carte) :
#   Poste_Meuble (étagère à écrans), Poste_Clavier (clavier + pupitre de commande des caméras),
#   Poste_Magnetoscope (enregistreur à LEDs), Poste_Radio (radio de secours), Poste_Moniteur (5e écran + pied).
# Coordonnées écrites en repère Unity (U(x, y, z)) : même place que les boîtes d'origine, sans retouche dans Unity.
# Emplacements de matériaux : Peint, Plastique, Acier, Chrome, Caoutchouc, Touche (touches beiges) (associés dans RoomBuilder).
# UV : projection cube (texel uniforme, ~0,6 m par répétition) pour les textures d'usure de Assets/Room/Textures/Usure.
# blender -b -P Tools/blender/poste_surveillance.py -- <dossier de sortie>
import bpy, bmesh, sys, os, math
from mathutils import Vector, Matrix

OUT = sys.argv[sys.argv.index("--") + 1]
os.makedirs(OUT, exist_ok=True)
SLOTS = ["Peint", "Plastique", "Acier", "Chrome", "Caoutchouc", "Touche"]


def U(x, y, z):
    """repère Unity -> repère Blender (export FBX axis_forward=-Z, axis_up=Y)."""
    return Vector((-x, -z, y))


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for s in SLOTS:
        if s not in bpy.data.materials:
            bpy.data.materials.new(s)


parts = []


def finish(o, slot, bevel=0.0, seg=2, angle=40):
    o.data.materials.append(bpy.data.materials[slot])
    if bevel > 0:
        m = o.modifiers.new("b", 'BEVEL')
        m.width = bevel
        m.segments = seg
        m.limit_method = 'ANGLE'
        m.angle_limit = math.radians(angle)
        m.harden_normals = False
    parts.append(o)
    return o


def box(c, s, slot, bevel=0.003, seg=2, rot=None):
    """boîte centrée en c (repère Unity), taille s (Unity), rotation Unity en degrés (x, y, z) facultative."""
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object
    o.scale = (s[0], s[2], s[1])
    if rot:
        # rotation Unity -> Blender : axe Y Unity = axe Z Blender, X Unity = -X Blender, Z Unity = -Y Blender
        o.rotation_euler = (math.radians(-rot[0]), math.radians(-rot[2]), math.radians(rot[1]))
    o.location = U(*c)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return finish(o, slot, bevel, seg)


def cyl(c, r, h, slot, axis='y', verts=16, bevel=0.0):
    """cylindre (repère Unity), axe 'x', 'y' ou 'z' Unity."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=h)
    o = bpy.context.active_object
    if axis == 'x':
        o.rotation_euler = (0, math.radians(90), 0)
    elif axis == 'z':
        o.rotation_euler = (math.radians(90), 0, 0)
    o.location = U(*c)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return finish(o, slot, bevel, 1, 50)


def wedge(c, s, front_h, slot, bevel=0.004):
    """boîte dont la face avant (+z Unity) est plus basse (pupitre incliné) : s = (largeur, hauteur arrière, profondeur)."""
    o = box(c, s, slot, 0)
    o.modifiers.clear()
    me = o.data
    for v in me.vertices:
        # Blender -Y = Unity +z (avant) ; Blender +Z = Unity y (haut)
        if v.co.y < 0 and v.co.z > 0:
            v.co.z -= (s[1] - front_h)
    return finish_mod(o, bevel)


def finish_mod(o, bevel):
    if bevel > 0:
        m = o.modifiers.new("b", 'BEVEL'); m.width = bevel; m.segments = 2
        m.limit_method = 'ANGLE'; m.angle_limit = math.radians(30)
    return o


def export(name, pivot=None):
    global parts
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:
        o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    for o in parts:
        bpy.context.view_layer.objects.active = o
        for m in list(o.modifiers):
            bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = o.data.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)   # sommets en coordonnées absolues (repère Unity)
    # matériaux : un emplacement par nom (le join les duplique)
    used = []
    for s in o.material_slots:
        if s.material and s.material.name not in used:
            used.append(s.material.name)
    idx = {n: i for i, n in enumerate(used)}
    remap = [idx[s.material.name] for s in o.material_slots]
    for p in o.data.polygons:
        p.material_index = remap[p.material_index]
    while len(o.material_slots) > len(used):
        o.active_material_index = len(o.material_slots) - 1
        bpy.ops.object.material_slot_remove()
    for i, n in enumerate(used):
        o.material_slots[i].material = bpy.data.materials[n]
    # UV : projection cube à densité constante
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.0002)
    bpy.ops.uv.cube_project(cube_size=0.6, correct_aspect=True, scale_to_bounds=False)
    bpy.ops.object.mode_set(mode='OBJECT')
    for p in o.data.polygons:
        p.use_smooth = True
    o.data.shade_smooth_by_angle(angle=math.radians(35)) if hasattr(o.data, "shade_smooth_by_angle") else None
    if pivot is not None:
        o.data.transform(Matrix.Translation(-U(*pivot)))
    tris = sum(len(p.vertices) - 2 for p in o.data.polygons)
    print("MODELE", name, "tris", tris, "verts", len(o.data.vertices), "mats", used)
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, name + ".fbx"), use_selection=False, apply_unit_scale=True,
                             apply_scale_options='FBX_SCALE_UNITS', axis_forward='-Z', axis_up='Y', bake_space_transform=True,
                             object_types={'MESH'}, use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
    parts = []


def smooth_shading_fix():
    pass


# ======================================================================== meuble à écrans
reset()
# plateau : tôle pliée (dessus + retombées avant/arrière), bords arrondis
box((-0.725, 1.300, 6.915), (1.65, 0.018, 0.47), "Peint", 0.004)
box((-0.725, 1.278, 7.144), (1.65, 0.045, 0.012), "Peint", 0.003)
box((-0.725, 1.278, 6.686), (1.65, 0.045, 0.012), "Peint", 0.003)
for x in (-1.5, -0.72, 0.06):
    for z in (6.745, 7.095):                      # montants en tube carré
        box((x, 1.03, z), (0.035, 0.50, 0.035), "Peint", 0.004)
        box((x, 0.795, z), (0.07, 0.012, 0.07), "Acier", 0.003)          # semelle
        for dx in (-0.022, 0.022):                                      # vis de la semelle
            cyl((x + dx, 0.803, z + 0.022), 0.006, 0.006, "Acier", 'y', 8)
    box((x, 0.93, 6.92), (0.025, 0.025, 0.33), "Peint", 0.003)           # traverse basse
    box((x, 1.255, 6.92), (0.025, 0.025, 0.33), "Peint", 0.003)          # traverse haute
    # écharpe en diagonale
    box((x, 1.09, 6.92), (0.012, 0.03, 0.42), "Peint", 0.002, rot=(-34, 0, 0))
# lisse arrière (rigidité)
box((-0.72, 1.12, 6.745), (1.56, 0.03, 0.012), "Peint", 0.002)
# goulotte de câbles sous le plateau, à l'arrière
box((-0.725, 1.255, 6.72), (1.45, 0.035, 0.03), "Plastique", 0.004)
for i in range(7):
    cyl((-1.38 + i * 0.22, 1.255, 6.738), 0.006, 0.004, "Acier", 'z', 8)
export("Poste_Meuble")

# ======================================================================== clavier + pupitre
reset()
cx, cy, cz = -0.55, 0.79, 7.31
# socle métallique incliné (arrière 4,5 cm, avant 2 cm)
wedge((cx, cy + 0.0225, cz), (0.80, 0.045, 0.22), 0.022, "Peint", 0.005)
box((cx, cy + 0.003, cz), (0.81, 0.006, 0.225), "Caoutchouc", 0.002)     # semelle caoutchouc
slope = math.degrees(math.atan2(0.023, 0.22))


def ytop(z):
    # hauteur du dessus du socle à la profondeur z (avant = cz + 0.11)
    t = (z - (cz - 0.11)) / 0.22
    return cy + 0.045 - 0.023 * t


# clavier (partie gauche) : 5 rangées de touches + barre d'espace
kx0, kw, pitch = cx - 0.37, 0.016, 0.0195
rows = [(14, 0), (14, 0.004), (13, 0.008), (12, 0.012)]
for r, (n, off) in enumerate(rows):
    z = cz - 0.075 + r * 0.032
    for k in range(n):
        x = kx0 + off + k * pitch + kw / 2
        box((x, ytop(z) + 0.006, z), (kw, 0.010, kw), "Touche", 0.0025, 1, rot=(slope, 0, 0))
# rangée du bas : modificateurs + espace
z = cz + 0.053
box((kx0 + 0.02, ytop(z) + 0.006, z), (0.034, 0.010, 0.016), "Touche", 0.0025, 1, rot=(slope, 0, 0))
box((kx0 + 0.14, ytop(z) + 0.006, z), (0.15, 0.010, 0.016), "Touche", 0.0025, 1, rot=(slope, 0, 0))
box((kx0 + 0.26, ytop(z) + 0.006, z), (0.034, 0.010, 0.016), "Touche", 0.0025, 1, rot=(slope, 0, 0))
# séparation clavier / pupitre
box((cx + 0.02, ytop(cz) + 0.002, cz), (0.004, 0.004, 0.20), "Acier", 0.001)
# pupitre de commande PTZ (partie droite) : manette, boutons, molette, petit afficheur
jx, jz = cx + 0.28, cz + 0.01
cyl((jx, ytop(jz) + 0.004, jz), 0.032, 0.008, "Caoutchouc", 'y', 20, 0.002)        # soufflet
cyl((jx, ytop(jz) + 0.035, jz), 0.006, 0.06, "Acier", 'y', 10)                        # tige
bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, radius=0.017, location=U(jx, ytop(jz) + 0.07, jz))
o = bpy.context.active_object; o.scale = (1, 1, 1.25); bpy.ops.object.transform_apply(scale=True); finish(o, "Plastique")
for i in range(4):                                                                 # boutons ronds
    bx = cx + 0.09 + (i % 2) * 0.04
    bz = cz - 0.03 + (i // 2) * 0.05
    cyl((bx, ytop(bz) + 0.004, bz), 0.011, 0.006, "Plastique", 'y', 14, 0.002)
    cyl((bx, ytop(bz) + 0.002, bz), 0.014, 0.004, "Chrome", 'y', 14)
box((cx + 0.30, ytop(cz - 0.075) + 0.004, cz - 0.075), (0.11, 0.006, 0.035), "Plastique", 0.002, rot=(slope, 0, 0))   # afficheur
box((cx + 0.30, ytop(cz - 0.075) + 0.002, cz - 0.075), (0.12, 0.004, 0.045), "Chrome", 0.002, rot=(slope, 0, 0))
cyl((cx + 0.37, ytop(cz + 0.06) + 0.008, cz + 0.06), 0.014, 0.016, "Plastique", 'y', 18, 0.002)   # molette de zoom
# câble qui part à l'arrière
cyl((cx - 0.30, cy + 0.012, cz - 0.16), 0.004, 0.10, "Caoutchouc", 'z', 8)
export("Poste_Clavier")

# ======================================================================== enregistreur à LEDs
reset()
x0, x1, yb, yt, zb, zf = 0.15, 0.95, 0.79, 0.94, 6.80, 7.246
cxm, cym, czm = (x0 + x1) / 2, (yb + yt) / 2, (zb + zf) / 2
box((cxm, cym + 0.004, czm - 0.01), (x1 - x0 - 0.02, yt - yb - 0.012, zf - zb - 0.03), "Peint", 0.006)   # capot
box((cxm, cym, zf - 0.012), (x1 - x0, yt - yb, 0.024), "Peint", 0.004)                                 # façade
for ex in (x0 - 0.012, x1 + 0.012):                                                                   # oreilles de baie
    box((ex, cym, zf - 0.006), (0.026, yt - yb, 0.004), "Acier", 0.002)
    for ey in (yb + 0.03, yt - 0.03):
        cyl((ex, ey, zf - 0.003), 0.005, 0.006, "Chrome", 'z', 10)
# fente cassette + trappe
box((0.52, 0.905, zf), (0.34, 0.022, 0.006), "Plastique", 0.002)
box((0.52, 0.905, zf - 0.002), (0.36, 0.032, 0.004), "Chrome", 0.002)
# boutons de transport
for i in range(5):
    box((0.30 + i * 0.045, 0.822, zf + 0.002), (0.034, 0.014, 0.008), "Plastique", 0.002)
# molette et interrupteur
cyl((0.18, 0.86, zf + 0.008), 0.016, 0.016, "Plastique", 'z', 18, 0.002)
box((0.90, 0.905, zf + 0.004), (0.03, 0.018, 0.008), "Chrome", 0.002)
# cache des LEDs (bande sombre derrière les voyants)
box((0.54, 0.860, zf + 0.0005), (0.70, 0.026, 0.002), "Plastique", 0.0)
# ouïes d'aération sur le dessus
for i in range(14):
    box((0.25 + i * 0.045, yt + 0.0005, 6.92), (0.012, 0.004, 0.16), "Plastique", 0.0015)
# pieds caoutchouc
for fx in (x0 + 0.05, x1 - 0.05):
    for fz in (zb + 0.04, zf - 0.04):
        cyl((fx, yb + 0.003, fz), 0.014, 0.006, "Caoutchouc", 'y', 12)
# connecteurs à l'arrière
for i in range(6):
    cyl((0.25 + i * 0.1, 0.87, zb - 0.004), 0.008, 0.014, "Chrome", 'z', 12)
export("Poste_Magnetoscope")

# ======================================================================== radio (repère local de Bunker_Emergency_Radio, avant = +z)
reset()
box((0, 0, 0), (0.30, 0.20, 0.155), "Peint", 0.014, 3)                         # boîtier aux coins arrondis
box((0, 0, 0.0785), (0.284, 0.184, 0.004), "Chrome", 0.002)                   # jonc de façade
# grille de haut-parleur (lamelles)
for i in range(9):
    box((0.055, -0.065 + i * 0.016, 0.081), (0.15, 0.007, 0.005), "Plastique", 0.0015)
box((0.055, 0.0, 0.0795), (0.165, 0.155, 0.003), "Plastique", 0.0)           # fond de la grille
# cadran d'accord : fenêtre + aiguille
box((-0.085, 0.045, 0.081), (0.09, 0.05, 0.004), "Plastique", 0.002)
box((-0.085, 0.045, 0.0835), (0.002, 0.042, 0.002), "Chrome", 0.0)
# boutons crantés
for i, y in enumerate((-0.005, -0.055)):
    cyl((-0.085, y, 0.088), 0.017, 0.016, "Plastique", 'z', 20, 0.002)
    cyl((-0.085, y, 0.080), 0.021, 0.004, "Chrome", 'z', 20)
# interrupteur à bascule
box((-0.125, -0.075, 0.083), (0.012, 0.02, 0.008), "Chrome", 0.002)
# poignée de transport : deux pattes + barre
for x in (-0.07, 0.07):
    box((x, 0.115, 0), (0.016, 0.03, 0.03), "Acier", 0.003)
cyl((0, 0.135, 0), 0.009, 0.16, "Caoutchouc", 'x', 12, 0.002)
# antenne télescopique (3 brins), pivot à l'arrière droit
cyl((0.135, 0.11, -0.05), 0.008, 0.02, "Chrome", 'y', 12)
cyl((0.135, 0.20, -0.05), 0.0045, 0.17, "Chrome", 'y', 10)
cyl((0.135, 0.33, -0.05), 0.0032, 0.10, "Chrome", 'y', 8)
bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=6, radius=0.005, location=U(0.135, 0.382, -0.05))
finish(bpy.context.active_object, "Chrome")
# ouïes latérales
for i in range(5):
    box((-0.151, -0.04 + i * 0.02, -0.01), (0.004, 0.006, 0.09), "Plastique", 0.0)
# pieds
for fx in (-0.11, 0.11):
    for fz in (-0.055, 0.055):
        cyl((fx, -0.098, fz), 0.012, 0.006, "Caoutchouc", 'y', 10)
export("Poste_Radio")

# ======================================================================== 5e écran (repère local de CCTV_Monitor_05 : écran à z = 0, face visible vers -z)
reset()
W, H = 0.47, 0.33
# cadre avant (4 montants biseautés autour de l'écran 0,42 x 0,236)
box((0, 0.1435, 0.006), (W, 0.043, 0.024), "Peint", 0.005)
box((0, -0.1395, 0.006), (W, 0.051, 0.024), "Peint", 0.005)
box((-0.2245, 0.002, 0.006), (0.021, 0.236, 0.024), "Peint", 0.004)
box((0.2245, 0.002, 0.006), (0.021, 0.236, 0.024), "Peint", 0.004)
box((0, 0.002, 0.012), (0.43, 0.246, 0.006), "Plastique", 0.0)               # fond derrière la dalle
# coque arrière en tronc de pyramide (tube cathodique)
bpy.ops.mesh.primitive_cube_add(size=1)
o = bpy.context.active_object
o.scale = (W - 0.02, 0.17, H - 0.02)
o.location = U(0, 0.0, 0.105)
bpy.ops.object.transform_apply(location=True, scale=True)
for v in o.data.vertices:
    if v.co.y > U(0, 0, 0.105).y:      # Blender +Y = Unity -z (avant) : on garde l'avant large
        continue
    v.co.x *= 0.62
    v.co.z = (v.co.z - U(0, 0.0, 0).z) * 0.62 + U(0, 0.0, 0).z
finish(o, "Peint", 0.01, 2, 30)
# ouïes à l'arrière
for i in range(7):
    box((-0.09 + i * 0.03, 0.06, 0.19), (0.012, 0.09, 0.004), "Plastique", 0.0)
# boutons et voyant sous l'écran
for i in range(4):
    box((0.08 + i * 0.03, -0.142, -0.008), (0.018, 0.012, 0.006), "Plastique", 0.0015)
box((-0.15, -0.142, -0.008), (0.06, 0.016, 0.003), "Chrome", 0.001)            # plaque de marque
# pied : colonne + rotule + socle posé sur le bureau (bas du socle à y = -0.38)
box((0, -0.19, 0.11), (0.08, 0.03, 0.10), "Acier", 0.004)
cyl((0, -0.25, 0.11), 0.018, 0.12, "Acier", 'y', 14, 0.002)
cyl((0, -0.315, 0.11), 0.026, 0.02, "Chrome", 'y', 16, 0.003)
box((0, -0.37, 0.10), (0.22, 0.016, 0.18), "Peint", 0.006)
for fx in (-0.09, 0.09):
    for fz in (0.03, 0.17):
        cyl((fx, -0.379, fz), 0.01, 0.004, "Caoutchouc", 'y', 10)
export("Poste_Moniteur")
