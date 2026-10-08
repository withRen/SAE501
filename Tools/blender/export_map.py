# Export de la carte Blender vers Unity (projet SAE501 / The Last Escapor).
# Usage : /Applications/Blender.app/Contents/MacOS/Blender -b "carte.blend" --python Tools/blender/export_map.py
# Produit dans Assets/Room :
#   LastBoardStanding_Map.fbx  (objets visibles : maillages + repères vides)
#   materials.json             (matériaux Principled -> recréés en URP par RoomImport.cs)
#   map_meta.json              (lumières et caméras de vidéosurveillance)
#   Textures/*.jpg|png         (couleur, normales, émission)
# Le .blend n'est pas modifié (rien n'est sauvegardé).
import bpy, os, json, re, shutil
import numpy as np

OUT = os.path.expanduser("~/SAE501/Assets/Room")
TEX = OUT + "/Textures"
MAX_TRIS = 6000          # au-delà, un objet de décor est simplifié (Quest)
KEEP_FULL = ("Wall", "Floor", "Ceiling", "Trim", "Cornice", "Wainscot", "Frame", "Door", "Window", "Apoc_")   # structure (nom contenant) : jamais simplifiée

# on repart d'un dossier propre (les anciens fichiers de la salle sont remplacés)
for f in ("LastBoardStanding_Room.fbx", "LastBoardStanding_Map.fbx", "materials.json", "room_meta.json", "map_meta.json"):
    p = os.path.join(OUT, f)
    if os.path.exists(p): os.remove(p)
if os.path.isdir(TEX): shutil.rmtree(TEX)
os.makedirs(TEX, exist_ok=True)

# gestion des couleurs neutre pendant l'export : les images sont enregistrées par le moteur de rendu (save_render),
# qui appliquerait sinon la vue de la scène (AgX, exposition +2,5 dans la carte finale) et éclaircirait les textures.
# L'exposition d'origine est gardée pour les lumières (map_meta.json).
EXPOSURE = float(bpy.context.scene.view_settings.exposure)
bpy.context.scene.view_settings.view_transform = "Standard"
bpy.context.scene.view_settings.look = "None"
bpy.context.scene.view_settings.exposure = 0.0
bpy.context.scene.view_settings.gamma = 1.0

def safe(n): return re.sub(r"[^A-Za-z0-9_\-]", "_", os.path.splitext(n)[0])

# ---------- matériaux procéduraux (jardin : bruit -> rampe de couleur, relief) ----------
# Unity ne sait pas calculer ces matériaux : pour chaque objet concerné on déplie le maillage (UV) et on cuit
# sa couleur et son relief (bump) dans deux images, avec un matériau propre à l'objet qui les utilise.
def bake_procedural():
    scn = bpy.context.scene
    scn.render.engine = "CYCLES"
    scn.cycles.samples = 1
    scn.cycles.device = "CPU"
    done = 0
    for o in list(scn.objects):
        if o.type != "MESH" or not o.visible_get() or not o.material_slots: continue
        m = o.material_slots[0].material
        if not m or not m.node_tree: continue
        nt0 = m.node_tree
        b = next((n for n in nt0.nodes if n.type == "BSDF_PRINCIPLED"), None)
        if not b or not b.inputs["Base Color"].is_linked or b.inputs["Base Color"].links[0].from_node.type != "VALTORGB": continue
        if o.data.users > 1: o.data = o.data.copy()
        bpy.ops.object.select_all(action="DESELECT")
        o.select_set(True); bpy.context.view_layer.objects.active = o
        if len(o.data.uv_layers) == 0:
            bpy.ops.object.mode_set(mode="EDIT")
            bpy.ops.mesh.select_all(action="SELECT")
            bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.01)
            bpy.ops.object.mode_set(mode="OBJECT")
        mc = m.copy(); mc.name = safe(m.name + "__" + o.name)
        for sl in o.material_slots:
            if sl.material == m: sl.material = mc
        nt = mc.node_tree
        size = 1024 if max(o.dimensions) > 4 else 512
        col = bpy.data.images.new(mc.name + "_Couleur", size, size)
        nrm = bpy.data.images.new(mc.name + "_Normale", size, size); nrm.colorspace_settings.name = "Non-Color"
        tex = nt.nodes.new("ShaderNodeTexImage"); nt.nodes.active = tex
        tex.image = col
        bpy.ops.object.bake(type="DIFFUSE", pass_filter={"COLOR"}, margin=8)
        bsdf = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
        if bsdf.inputs["Normal"].is_linked:
            tex.image = nrm
            bpy.ops.object.bake(type="NORMAL", normal_space="TANGENT", margin=8)
        # le matériau utilise maintenant les images cuites
        for l in list(bsdf.inputs["Base Color"].links): nt.links.remove(l)
        tc = nt.nodes.new("ShaderNodeTexImage"); tc.image = col
        nt.links.new(tc.outputs["Color"], bsdf.inputs["Base Color"])
        if bsdf.inputs["Normal"].is_linked:
            for l in list(bsdf.inputs["Normal"].links): nt.links.remove(l)
            tn = nt.nodes.new("ShaderNodeTexImage"); tn.image = nrm
            nm = nt.nodes.new("ShaderNodeNormalMap")
            nt.links.new(tn.outputs["Color"], nm.inputs["Color"])
            nt.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
        nt.nodes.remove(tex)
        done += 1
    print("PROCEDURAUX CUITS", done)

bake_procedural()
saved = {}
def save_img(img, keep_alpha=False):
    if img is None: return None
    key = (img.name, keep_alpha)
    if key in saved: return saved[key]
    ext = "png" if keep_alpha else "jpg"
    fn = safe(img.name) + "." + ext
    s = bpy.context.scene
    st = s.render.image_settings
    old = (st.file_format, st.color_mode, st.quality)
    st.file_format = "PNG" if keep_alpha else "JPEG"
    st.color_mode = "RGBA" if keep_alpha else "RGB"
    st.quality = 88
    img.save_render(filepath=os.path.join(TEX, fn), scene=s)
    st.file_format, st.color_mode, st.quality = old
    saved[key] = fn
    return fn

def mean_channel(img, ch):
    px = np.array(img.pixels[:], dtype=np.float32).reshape(-1, img.channels)
    return float(px[:, min(ch, img.channels - 1)].mean())

def src(socket):
    if not socket.is_linked: return None
    return socket.links[0].from_node

used = set()
for o in bpy.context.scene.objects:
    if o.type == "MESH" and o.visible_get():
        for s in o.material_slots:
            if s.material: used.add(s.material.name)

mats = {}
for m in bpy.data.materials:
    if m.name not in used or not m.node_tree: continue
    b = next((n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED"), None)
    if not b: continue
    alpha_linked = b.inputs["Alpha"].is_linked
    d = {"base_color": list(b.inputs["Base Color"].default_value), "rough": float(b.inputs["Roughness"].default_value),
         "metal": float(b.inputs["Metallic"].default_value), "alpha": float(b.inputs["Alpha"].default_value),
         "blend": getattr(m, "blend_method", "OPAQUE"), "tiling": [1, 1], "base_tex": None, "normal_tex": None,
         "emission_tex": None, "alpha_linked": alpha_linked, "emission_strength": 0.0}
    n = src(b.inputs["Base Color"])
    # couleur procédurale (bruit -> rampe de couleur, jardin de la carte finale) : couleur moyenne de la rampe
    if n and n.type == "VALTORGB":
        el = n.color_ramp.elements
        d["base_color"] = [sum(e.color[i] for e in el) / len(el) for i in range(3)] + [1.0]
    if n and n.type == "TEX_IMAGE":
        d["base_tex"] = save_img(n.image, keep_alpha=alpha_linked)
        if n.inputs["Vector"].is_linked and n.inputs["Vector"].links[0].from_node.type == "MAPPING":
            sc = n.inputs["Vector"].links[0].from_node.inputs["Scale"].default_value
            d["tiling"] = [float(sc[0]), float(sc[1])]
    nm = src(b.inputs["Normal"])
    if nm and nm.type == "NORMAL_MAP":
        t = src(nm.inputs["Color"])
        if t and t.type == "TEX_IMAGE": d["normal_tex"] = save_img(t.image)
    r = src(b.inputs["Roughness"])
    if r and r.type == "TEX_IMAGE" and r.image: d["rough"] = mean_channel(r.image, 0)
    elif r and r.type == "SEPARATE_COLOR":
        t = src(r.inputs["Color"])
        if t and t.type == "TEX_IMAGE" and t.image: d["rough"] = mean_channel(t.image, 1)
    mt = src(b.inputs["Metallic"])
    if mt and mt.type == "TEX_IMAGE" and mt.image: d["metal"] = mean_channel(mt.image, 0)
    elif mt and mt.type == "SEPARATE_COLOR":
        t = src(mt.inputs["Color"])
        if t and t.type == "TEX_IMAGE" and t.image: d["metal"] = mean_channel(t.image, 2)
    e = src(b.inputs["Emission Color"])
    if e and e.type == "TEX_IMAGE": d["emission_tex"] = save_img(e.image)
    d["emission_color"] = list(b.inputs["Emission Color"].default_value)[:3]
    d["emission_strength"] = float(b.inputs["Emission Strength"].default_value)
    mats[m.name] = d
json.dump(mats, open(OUT + "/materials.json", "w"), indent=1)

# lumières et caméras (repère Blender : X droite, Y avant, Z haut ; conversion en Unity par RoomImport/RoomBuilder)
from mathutils import Vector
def fwd(o): return list(o.matrix_world.to_3x3() @ Vector((0, 0, -1)))
# exposition de la vue (Color Management) : la carte est éclairée pour cette exposition, RoomBuilder l'applique aux intensités
meta = {"lights": [], "cameras": [], "exposure": EXPOSURE}
for o in bpy.context.scene.objects:
    if not o.visible_get(): continue
    if o.type == "LIGHT":
        L = o.data
        meta["lights"].append({"name": o.name, "type": L.type, "energy": L.energy, "color": list(L.color),
            "loc": list(o.matrix_world.translation), "dir": fwd(o),
            "spot_size": getattr(L, "spot_size", 0.0), "spot_blend": getattr(L, "spot_blend", 0.0),
            "radius": getattr(L, "shadow_soft_size", 0.0), "shadow": bool(getattr(L, "use_shadow", False))})
    elif o.type == "CAMERA":
        meta["cameras"].append({"name": o.name, "loc": list(o.matrix_world.translation), "dir": fwd(o),
            "up": list(o.matrix_world.to_3x3() @ Vector((0, 1, 0))), "fov": o.data.angle})
json.dump(meta, open(OUT + "/map_meta.json", "w"), indent=1)

# simplification des objets de décor trop lourds (uniquement à l'export)
dg = bpy.context.evaluated_depsgraph_get()
before = after = 0
simplified = []
for o in bpy.context.scene.objects:
    if o.type != "MESH" or not o.visible_get(): continue
    ev = o.evaluated_get(dg); me = ev.to_mesh()
    t = sum(len(p.vertices) - 2 for p in me.polygons); ev.to_mesh_clear()
    before += t
    if t > MAX_TRIS and not any(k in o.name for k in KEEP_FULL):
        mod = o.modifiers.new("ExportDecimate", "DECIMATE")
        mod.ratio = MAX_TRIS / t
        simplified.append((o.name, t))
        after += MAX_TRIS
    else:
        after += t

for o in bpy.context.scene.objects:
    o.select_set(o.visible_get() and o.type in ("MESH", "EMPTY"))
bpy.ops.export_scene.fbx(filepath=OUT + "/LastBoardStanding_Map.fbx", use_selection=True, object_types={"MESH", "EMPTY"},
    global_scale=1.0, apply_unit_scale=True, apply_scale_options="FBX_SCALE_NONE", bake_space_transform=True,
    axis_forward="-Z", axis_up="Y", use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False,
    path_mode="STRIP", embed_textures=False)
print("EXPORT OK", len(mats), "matériaux,", len(set(saved.values())), "textures,", len(meta["lights"]), "lumières,", len(meta["cameras"]), "caméras")
print("TRIANGLES avant", before, "après ~", after, "; objets simplifiés :", len(simplified))
