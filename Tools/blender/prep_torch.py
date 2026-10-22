# Lampe torche Poly Haven (small_plastic_torch, glTF) -> FBX pour Unity.
# Longueur sur l'axe Y de Blender, verre (ampoule) du côté +Y, origine au milieu, taille réelle,
# allégée à ~6 000 triangles (Decimate) pour le Quest. Deux matériaux gardés : corps, verre.
# blender -b -P Tools/blender/prep_torch.py -- small_plastic_torch_1k.gltf Assets/Lampe/Lampe_Torche.fbx
import bpy, sys, os, mathutils
argv = sys.argv[sys.argv.index("--")+1:]
src, dst = argv[0], argv[1]
target_tris = 6000
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
bpy.ops.object.select_all(action='DESELECT')
for o in meshes: o.select_set(True)
bpy.context.view_layer.objects.active = meshes[0]
if len(meshes) > 1: bpy.ops.object.join()
o = bpy.context.view_layer.objects.active
bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
for x in list(bpy.context.scene.objects):
    if x != o: bpy.data.objects.remove(x)
me = o.data
vs = [v.co.copy() for v in me.vertices]
size = [max(v[i] for v in vs) - min(v[i] for v in vs) for i in range(3)]
ax = max(range(3), key=lambda i: size[i])
# centre du verre (matériau dont le nom contient « glass ») : il indique le bout qui éclaire
gi = next((i for i, m in enumerate(me.materials) if m and "glass" in m.name.lower()), None)
gpts = [me.vertices[vi].co for p in me.polygons if p.material_index == gi for vi in p.vertices] if gi is not None else []
mid = (max(v[ax] for v in vs) + min(v[ax] for v in vs)) / 2
lens_positive = (sum(v[ax] for v in gpts) / len(gpts) > mid) if gpts else True
d = mathutils.Vector((0, 0, 0)); d[ax] = 1 if lens_positive else -1
R = d.rotation_difference(mathutils.Vector((0, 1, 0))).to_matrix().to_4x4()
me.transform(R)
vs = [v.co for v in me.vertices]
mn = mathutils.Vector([min(v[i] for v in vs) for i in range(3)]); mx = mathutils.Vector([max(v[i] for v in vs) for i in range(3)])
me.transform(mathutils.Matrix.Translation(-(mn + mx) / 2))
me.update()
tris = sum(len(p.vertices) - 2 for p in me.polygons)
if tris > target_tris:
    mod = o.modifiers.new("Decimate", 'DECIMATE'); mod.ratio = target_tris / tris
    bpy.ops.object.modifier_apply(modifier=mod.name)
o.name = me.name = os.path.splitext(os.path.basename(dst))[0]
vs = [v.co for v in o.data.vertices]
print("TAILLE", [round(max(v[i] for v in vs) - min(v[i] for v in vs), 4) for i in range(3)],
      "TRIS", tris, "->", sum(len(p.vertices) - 2 for p in o.data.polygons), "MATERIAUX", [m.name for m in o.data.materials])
bpy.ops.export_scene.fbx(filepath=dst, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
    axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH'}, use_mesh_modifiers=True, embed_textures=False, path_mode='STRIP', mesh_smooth_type='FACE')
