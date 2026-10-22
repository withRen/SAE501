import bpy, sys, mathutils
argv = sys.argv[sys.argv.index("--")+1:]
src, dst, length = argv[0], argv[1], float(argv[2])
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
# axes : longueur -> X, épaisseur -> Z (à plat, Blender Z haut)
vs = [v.co.copy() for v in o.data.vertices]
size = [max(v[i] for v in vs) - min(v[i] for v in vs) for i in range(3)]
order = sorted(range(3), key=lambda i: size[i])   # [épaisseur, largeur, longueur]
thin, wide, long_ = order
M = mathutils.Matrix.Identity(3)
rows = [None]*3
rows[0] = mathutils.Vector([1 if j == long_ else 0 for j in range(3)])
rows[1] = mathutils.Vector([1 if j == wide else 0 for j in range(3)])
rows[2] = mathutils.Vector([1 if j == thin else 0 for j in range(3)])
R = mathutils.Matrix(rows)
if R.determinant() < 0: R[1] = -R[1]
o.data.transform(R.to_4x4())
vs = [v.co for v in o.data.vertices]
mn = mathutils.Vector([min(v[i] for v in vs) for i in range(3)]); mx = mathutils.Vector([max(v[i] for v in vs) for i in range(3)])
c = (mn + mx) / 2
s = length / (mx.x - mn.x)
o.data.transform(mathutils.Matrix.Scale(s, 4) @ mathutils.Matrix.Translation(-c))
o.data.update()
o.name = o.data.name = "Cle_Ornee"
# UV : projection cubique en mètres (texture de métal répétée)
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.cube_project(cube_size=0.05)
bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.shade_auto_smooth(angle=0.6) if hasattr(bpy.ops.object, "shade_auto_smooth") else None
vs = [v.co for v in o.data.vertices]
print("TAILLE", [round(max(v[i] for v in vs) - min(v[i] for v in vs), 4) for i in range(3)], "TRIS", sum(len(p.vertices) - 2 for p in o.data.polygons))
bpy.ops.export_scene.fbx(filepath=dst, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
    axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH'}, use_mesh_modifiers=True, embed_textures=False, path_mode='STRIP', mesh_smooth_type='FACE')
