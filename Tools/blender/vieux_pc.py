# Vieux PC de décor (années 90) posé sur le bureau du poste de surveillance : écran cathodique beige, clavier, tour.
# Fait pour le projet (journal 232), pas d'asset externe. Repère local Unity : origine au pied du PC, au niveau du plateau,
# écran tourné vers +z (vers la pièce). Placé par Assets/Editor/VieuxPC.cs.
# Emplacements de matériaux : Coque (plastique beige jauni), Sombre (fentes, intérieur du cadre), Touche, Ecran (terminal
# émissif, UV 0-1 sur la dalle), Led (voyant vert).
# blender -b -P Tools/blender/vieux_pc.py -- <dossier de sortie>
import bpy, bmesh, sys, os, math
from mathutils import Vector

OUT = sys.argv[sys.argv.index("--") + 1]
os.makedirs(OUT, exist_ok=True)
SLOTS = ["Coque", "Sombre", "Touche", "Ecran", "Led"]


def U(x, y, z):
    """repère Unity -> repère Blender (export FBX axis_forward=-Z, axis_up=Y)."""
    return Vector((-x, -z, y))


bpy.ops.wm.read_factory_settings(use_empty=True)
for s in SLOTS:
    bpy.data.materials.new(s)
parts = []


def finish(o, slot, bevel=0.0, seg=2):
    o.data.materials.append(bpy.data.materials[slot])
    if bevel > 0:
        m = o.modifiers.new("b", 'BEVEL'); m.width = bevel; m.segments = seg
        m.limit_method = 'ANGLE'; m.angle_limit = math.radians(40)
    parts.append(o)
    return o


def box(c, s, slot, bevel=0.004, seg=2, taper=None):
    """boîte centrée en c, taille s (repère Unity). taper=(sx, sy) : face arrière (-z Unity) réduite (dos du tube cathodique)."""
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object
    o.scale = (s[0], s[2], s[1])
    bpy.ops.object.transform_apply(scale=True)
    if taper:
        for v in o.data.vertices:
            if v.co.y > 0:                       # Blender +Y = Unity -z (arrière)
                v.co.x *= taper[0]; v.co.z *= taper[1]
                v.co.z -= s[1] * 0.04            # dos légèrement plus bas
    o.location = U(*c)
    bpy.ops.object.transform_apply(location=True)
    return finish(o, slot, bevel, seg)


def cyl(c, r, h, slot, verts=24, bevel=0.003):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=h)
    o = bpy.context.active_object
    o.location = U(*c)
    bpy.ops.object.transform_apply(location=True)
    return finish(o, slot, bevel, 2)


# ---------------------------------------------------------------- écran cathodique (centre x = 0)
cyl((0, 0.012, 0.0), 0.12, 0.024, "Coque")                               # pied rond
box((0, 0.045, 0.0), (0.12, 0.05, 0.12), "Coque", 0.01)                  # cou
box((0, 0.27, 0.06), (0.40, 0.38, 0.09), "Coque", 0.018, 3)              # cadre avant
box((0, 0.255, -0.12), (0.36, 0.34, 0.28), "Coque", 0.03, 3, taper=(0.62, 0.62))   # dos du tube
box((0, 0.285, 0.103), (0.335, 0.265, 0.006), "Sombre", 0.002)          # creux autour de la dalle
box((0, 0.285, 0.107), (0.31, 0.24, 0.004), "Ecran", 0.0)                # dalle (UV 0-1, plus bas)
box((0.15, 0.105, 0.106), (0.03, 0.012, 0.004), "Sombre", 0.001)         # bouton marche
box((0.115, 0.105, 0.107), (0.008, 0.008, 0.004), "Led", 0.0)            # voyant
for i in range(5):                                                       # aérations sur le dessus du dos
    box((0, 0.42, -0.05 - i * 0.04), (0.18, 0.004, 0.012), "Sombre", 0.0)

# ---------------------------------------------------------------- clavier (devant l'écran)
kb = box((0, 0.016, 0.27), (0.44, 0.032, 0.16), "Coque", 0.008)
for v in kb.data.vertices:                                               # incliné : bord avant plus bas
    if v.co.y < -0.27 + 0.0 and v.co.z > 0.016:                          # (avant = Blender -Y)
        v.co.z -= 0.012
KX, KZ, KS = 0.028, 0.026, 0.022
for row in range(4):
    for col in range(14):
        x = -0.19 + col * KX
        z = 0.215 + row * KZ
        y = 0.036 - row * 0.003
        w = KS * (2.2 if (row == 3 and col == 6) else 1.0)
        if row == 3 and col in (5, 7):
            continue                                                     # place de la barre d'espace
        box((x + (KX * 0.0), y, z), (w, 0.01, KS), "Touche", 0.0015, 1)

# ---------------------------------------------------------------- tour (à droite de l'écran)
TX = 0.36
box((TX, 0.20, -0.02), (0.18, 0.40, 0.42), "Coque", 0.008)
box((TX, 0.33, 0.193), (0.13, 0.03, 0.008), "Sombre", 0.002)            # lecteur 5"1/4
box((TX, 0.27, 0.193), (0.10, 0.018, 0.008), "Sombre", 0.002)           # lecteur de disquettes
box((TX, 0.268, 0.198), (0.07, 0.004, 0.003), "Coque", 0.0)             # fente
box((TX - 0.05, 0.10, 0.193), (0.02, 0.02, 0.006), "Sombre", 0.002)     # bouton
box((TX + 0.04, 0.10, 0.195), (0.01, 0.01, 0.004), "Led", 0.0)         # voyant
for i in range(6):
    box((TX, 0.16 + i * 0.012, 0.193), (0.12, 0.004, 0.004), "Sombre", 0.0)   # grille

# ---------------------------------------------------------------- fusion, UV, export
bpy.ops.object.select_all(action='DESELECT')
for o in parts:
    bpy.context.view_layer.objects.active = o
    for m in list(o.modifiers):
        bpy.ops.object.modifier_apply(modifier=m.name)
for o in parts:
    o.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.join()
o = bpy.context.active_object
o.name = o.data.name = "Vieux_PC"
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

bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.cube_project(cube_size=0.4, correct_aspect=True, scale_to_bounds=False)
bpy.ops.object.mode_set(mode='OBJECT')

# dalle : UV 0-1 sur la face avant (Unity x -> u, y -> v)
me = o.data
uv = me.uv_layers.active.data
ei = used.index("Ecran")
x0, x1, y0, y1 = -0.155, 0.155, 0.285 - 0.12, 0.285 + 0.12
for p in me.polygons:
    if p.material_index != ei:
        continue
    for li in p.loop_indices:
        co = me.vertices[me.loops[li].vertex_index].co          # Blender : x = -x Unity, z = y Unity
        ux, uy = -co.x, co.z
        uv[li].uv = ((ux - x0) / (x1 - x0), (uy - y0) / (y1 - y0))

for p in me.polygons:
    p.use_smooth = False
tris = sum(len(p.vertices) - 2 for p in me.polygons)
print("MODELE Vieux_PC tris", tris, "verts", len(me.vertices), "mats", used)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "Vieux_PC.fbx"), use_selection=False, apply_unit_scale=True,
                         apply_scale_options='FBX_SCALE_UNITS', axis_forward='-Z', axis_up='Y', bake_space_transform=True,
                         object_types={'MESH'}, use_mesh_modifiers=True, mesh_smooth_type='FACE', path_mode='STRIP')
