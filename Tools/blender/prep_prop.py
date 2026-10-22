# Prépare un objet de décor Poly Haven (glTF) pour Unity : un seul maillage, debout (Z haut dans Blender),
# taille réelle gardée, origine au centre du fond, UV d'origine gardées, export FBX (Y haut).
# blender -b -P Tools/blender/prep_prop.py -- source.gltf destination.fbx
import bpy, sys, os, mathutils
argv = sys.argv[sys.argv.index("--")+1:]
src, dst = argv[0], argv[1]
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
vs = [v.co for v in o.data.vertices]
mn = mathutils.Vector([min(v[i] for v in vs) for i in range(3)]); mx = mathutils.Vector([max(v[i] for v in vs) for i in range(3)])
o.data.transform(mathutils.Matrix.Translation(-mathutils.Vector(((mn.x + mx.x) / 2, (mn.y + mx.y) / 2, mn.z))))
o.data.update()
o.name = o.data.name = os.path.splitext(os.path.basename(dst))[0]
print("TAILLE", [round(mx[i] - mn[i], 4) for i in range(3)], "TRIS", sum(len(p.vertices) - 2 for p in o.data.polygons))
bpy.ops.export_scene.fbx(filepath=dst, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
    axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH'}, use_mesh_modifiers=True, embed_textures=False, path_mode='STRIP', mesh_smooth_type='FACE')
