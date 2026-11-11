# Arbre du jardin et de la forêt pour le Quest, à partir de l'arbre Poly Haven island_tree_02 (CC0, ~1 070 000 triangles).
#
# L'export de la carte (export_map.py) simplifiait chaque objet à 6 000 triangles par « collapse » global : les troncs
# devenaient des pointes noires étirées (faces retournées, UV écrasées). Ici chaque partie est traitée à part :
#   - tronc : simplifié seul (collapse), normales recalculées vers l'extérieur, ombrage lisse ;
#   - rameaux : on garde une part des rameaux (tirage fixe), puis simplification ;
#   - feuilles : chaque feuille (grille de 24 triangles) devient un quad de 2 triangles posé sur ses 4 coins (mêmes UV),
#     on garde une part des feuilles, agrandies autour de leur centre pour garder un feuillage dense.
# Deux niveaux : LOD0 (~16 000 triangles) et LOD1 (~5 000). Export : Assets/Nature/Arbres/Arbre_Quest.fbx
#
# Usage : Blender -b --python Tools/blender/arbre_quest.py -- <island_tree_02_1k.gltf> <sortie.fbx>
import bpy, bmesh, sys, random
import numpy as np
from mathutils import Vector

src, out = sys.argv[-2], sys.argv[-1]

LODS = {
    #           tronc   rameaux gardés  rameaux simplif.  feuilles gardées  agrandissement
    "LOD0": dict(trunk=0.16, twig_keep=0.20, twig_ratio=0.07, leaf_keep=0.16, leaf_scale=2.3),
    "LOD1": dict(trunk=0.05, twig_keep=0.10, twig_ratio=0.05, leaf_keep=0.06, leaf_scale=3.6),
}


def islands(bm, faces):
    seen, res = set(), []
    fs = set(faces)
    for f in faces:
        if f in seen:
            continue
        stack, isl = [f], []
        seen.add(f)
        while stack:
            g = stack.pop()
            isl.append(g)
            for e in g.edges:
                for h in e.link_faces:
                    if h in fs and h not in seen:
                        seen.add(h)
                        stack.append(h)
        res.append(isl)
    return res


def build(name, p, base):
    ob = base.copy()
    ob.data = base.data.copy()
    ob.name = ob.data.name = name
    bpy.context.collection.objects.link(ob)
    mats = [m.name for m in ob.data.materials]
    iT, iL, iB = mats.index("island_tree_02"), mats.index("island_tree_02_leaves"), mats.index("island_tree_02_branches")
    rnd = random.Random(501)

    bm = bmesh.new()
    bm.from_mesh(ob.data)
    uv = bm.loops.layers.uv.active

    # feuilles : un quad par feuille gardée, les autres supprimées
    leaves = islands(bm, [f for f in bm.faces if f.material_index == iL])
    quads = []
    for isl in leaves:
        if rnd.random() > p["leaf_keep"]:
            continue
        # la feuille est une grille courbée : on cherche le plan qui envoie ses UV sur ses positions
        # (moindres carrés : position = centre + A x (uv - uv moyen)), puis on pose les 4 coins de son cadre d'UV dessus
        loops = [l for f in isl for l in f.loops]
        P = np.array([l.vert.co[:] for l in loops])
        U = np.array([l[uv].uv[:] for l in loops])
        pc, uc = P.mean(0), U.mean(0)
        A, *_ = np.linalg.lstsq(U - uc, P - pc, rcond=None)      # 2 x 3
        umin, umax = U.min(0), U.max(0)
        corners = [(umin[0], umin[1]), (umax[0], umin[1]), (umax[0], umax[1]), (umin[0], umax[1])]
        pts = []
        for c in corners:
            co = pc + (np.array(c) - uc) @ A * p["leaf_scale"]
            pts.append((Vector(co.tolist()), Vector(c)))
        quads.append(pts)
    bmesh.ops.delete(bm, geom=list({f for isl in leaves for f in isl}), context="FACES")
    for pts in quads:
        vs = [bm.verts.new(co) for co, _ in pts]
        f = bm.faces.new(vs)
        f.material_index = iL
        f.smooth = True
        for l, (_, u) in zip(f.loops, pts):
            l[uv].uv = u

    # rameaux : une part gardée
    twigs = islands(bm, [f for f in bm.faces if f.material_index == iB])
    drop = [f for isl in twigs if rnd.random() > p["twig_keep"] for f in isl]
    bmesh.ops.delete(bm, geom=list(set(drop)), context="FACES")
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context="VERTS")
    bm.to_mesh(ob.data)
    bm.free()

    # simplification par matériau (tronc, rameaux) : chaque partie est détachée, simplifiée seule puis recollée
    # (un seul modificateur avec groupe de sommets viserait le total de l'objet et écraserait le tronc)
    bpy.ops.object.select_all(action="DESELECT")
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob
    parts = []
    for idx, ratio in ((iT, p["trunk"]), (iB, p["twig_ratio"])):
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="DESELECT")
        bpy.ops.object.mode_set(mode="OBJECT")
        for f in ob.data.polygons:
            f.select = f.material_index == idx
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.separate(type="SELECTED")
        bpy.ops.object.mode_set(mode="OBJECT")
        part = [o for o in bpy.context.selected_objects if o != ob][0]
        mod = part.modifiers.new("Dec", "DECIMATE")
        mod.decimate_type = "COLLAPSE"
        mod.ratio = ratio
        mod.use_collapse_triangulate = True
        bpy.context.view_layer.objects.active = part
        bpy.ops.object.modifier_apply(modifier=mod.name)
        part.select_set(False)
        parts.append(part)
        bpy.context.view_layer.objects.active = ob
    bpy.ops.object.select_all(action="DESELECT")
    for o in parts + [ob]:
        o.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.join()

    # normales du tronc et des rameaux vers l'extérieur, ombrage lisse partout
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="DESELECT")
    bpy.ops.object.mode_set(mode="OBJECT")
    for f in ob.data.polygons:
        f.select = f.material_index != iL
        f.use_smooth = True
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    tris = sum(len(f.vertices) - 2 for f in ob.data.polygons)
    per = {m: sum(len(f.vertices) - 2 for f in ob.data.polygons if f.material_index == i) for i, m in enumerate(mats)}
    print(f"[arbre_quest] {name} : {tris} triangles {per}")
    return ob


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
base = [o for o in bpy.data.objects if o.type == "MESH"][0]
made = [build(n, p, base) for n, p in LODS.items()]
bpy.data.objects.remove(base)
for o in made:
    o.location = (0, 0, 0)
bpy.ops.object.select_all(action="DESELECT")
for o in made:
    o.select_set(True)
bpy.ops.export_scene.fbx(filepath=out, use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         bake_space_transform=True, axis_forward="-Z", axis_up="Y",
                         mesh_smooth_type="FACE", use_tspace=True, path_mode="STRIP", embed_textures=False)
print("[arbre_quest] export :", out)
