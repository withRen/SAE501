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
SMALL_TRIS = {"Garden_PathStone_": 800, "Garden_MossBoulder_": 1200}   # petits décors du jardin : budget plus serré
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

# ---------- images non empaquetées (Finalefinale : textures restées sur le PC de l'auteur) ----------
# SAE_TEXTURES=dossier[:dossier] : on y cherche chaque image manquante par son nom de fichier.
# SAE_FALLBACK_BLEND=ancien.blend : un matériau dont une image manque encore est remplacé par le matériau du même nom
# de cet ancien fichier (ex. herbe et pavés procéduraux de map_final_final.blend, cuits ensuite par bake_procedural).
def missing(img):
    return img.source == "FILE" and img.packed_file is None and not os.path.exists(bpy.path.abspath(img.filepath))

def relink_images():
    found = {}
    for d in filter(None, os.environ.get("SAE_TEXTURES", "").split(":")):
        for r, _, fs in os.walk(os.path.expanduser(d)):
            for f in fs: found.setdefault(f.lower(), os.path.join(r, f))
    for img in bpy.data.images:
        if not missing(img): continue
        p = found.get(os.path.basename(img.filepath.replace("\\", "/")).lower())
        if p:
            img.filepath = p; img.reload()
            print("IMAGE RETROUVÉE", img.name, "->", p)
    fb = os.environ.get("SAE_FALLBACK_BLEND")
    if not fb: return
    for m in list(bpy.data.materials):
        if not m.node_tree or not any(n.type == "TEX_IMAGE" and n.image and missing(n.image) for n in m.node_tree.nodes): continue
        with bpy.data.libraries.load(os.path.expanduser(fb), link=False) as (src, dst):
            if m.name not in src.materials: continue
            dst.materials = [m.name]
        if dst.materials and dst.materials[0]:
            old_name = m.name
            m.user_remap(dst.materials[0]); bpy.data.materials.remove(m)
            dst.materials[0].name = old_name
            print("MATÉRIAU REPRIS de", os.path.basename(fb), ":", old_name)

# ---------- feuillage répété (haie du jardin, sapins de la forêt) ----------
# Cuits en une seule image pour des dizaines de mètres, ils étaient flous (quelques pixels par mètre). On leur donne
# une texture de feuillage répétable (Tools/blender/make_hedge_texture.py) et des UV en mètres (projection sur le cube) :
# même coût au casque (une image de 1024 px), net de près.
TILED = {"Mat_Garden_DenseHedge": 1.5, "Mat_Garden_DarkFoliage": 2.5}   # matériau -> mètres par répétition
TILE_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "textures")
def tile_foliage():
    import bmesh
    if not os.path.exists(os.path.join(TILE_DIR, "haie_feuillage_couleur.png")): return
    col = bpy.data.images.load(os.path.join(TILE_DIR, "haie_feuillage_couleur.png"))
    nrm = bpy.data.images.load(os.path.join(TILE_DIR, "haie_feuillage_normale.png")); nrm.colorspace_settings.name = "Non-Color"
    for mname, size in TILED.items():
        m = bpy.data.materials.get(mname)
        if not m: continue
        nt = m.node_tree
        for n in list(nt.nodes):
            if n.type not in ("BSDF_PRINCIPLED", "OUTPUT_MATERIAL"): nt.nodes.remove(n)
        b = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
        tc = nt.nodes.new("ShaderNodeTexImage"); tc.image = col
        tn = nt.nodes.new("ShaderNodeTexImage"); tn.image = nrm
        nm = nt.nodes.new("ShaderNodeNormalMap")
        nt.links.new(tc.outputs["Color"], b.inputs["Base Color"])
        nt.links.new(tn.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs["Normal"], b.inputs["Normal"])
        b.inputs["Roughness"].default_value = 0.8
        for o in bpy.context.scene.objects:
            if o.type != "MESH" or not any(s.material == m for s in o.material_slots): continue
            if o.data.users > 1: o.data = o.data.copy()
            bm = bmesh.new(); bm.from_mesh(o.data)
            uv = bm.loops.layers.uv[0] if len(bm.loops.layers.uv) else bm.loops.layers.uv.new()
            mw = o.matrix_world; rot = mw.to_3x3()
            for f in bm.faces:
                nw = rot @ f.normal; ax = max(range(3), key=lambda i: abs(nw[i]))
                u, v = [(1, 2), (0, 2), (0, 1)][ax]
                for l in f.loops:
                    w = mw @ l.vert.co
                    l[uv].uv = (w[u] / size, w[v] / size)
            bm.to_mesh(o.data); bm.free()
        print("FEUILLAGE RÉPÉTÉ", mname, "tous les", size, "m")

relink_images()
tile_foliage()
bake_procedural()
saved = {}
def save_img(img, keep_alpha=False):
    if img is None: return None
    if not img.has_data and img.packed_file is None and not os.path.exists(bpy.path.abspath(img.filepath)):
        print("IMAGE MANQUANTE (non empaquetée dans le .blend) :", img.name, img.filepath)
        return None
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
    if img.size[0] == 0: return 0.0 if ch == 2 or ch == 0 and "metal" in img.name.lower() else 0.8   # image manquante
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

# arbres du jardin (Finalefinale : Garden_RealTree_*, ~248 000 triangles dont 244 000 de feuilles) : la simplification
# classique écrase les cartes de feuilles. On garde une feuille sur LEAF_STEP (tirage régulier) et on l'agrandit pour
# conserver un feuillage aussi couvrant ; le tronc et les branches restent intacts. Le maillage est partagé par les arbres.
LEAF_STEP = 20
def thin_tree_leaves():
    import bmesh, random
    rnd = random.Random(7)
    done = set()
    for o in bpy.context.scene.objects:
        if o.type != "MESH" or not o.visible_get() or o.data.name in done: continue
        li = [i for i, s in enumerate(o.material_slots) if s.material and "leaves" in s.material.name.lower()]
        if not li or len(o.data.polygons) <= MAX_TRIS: continue
        done.add(o.data.name)
        bm = bmesh.new(); bm.from_mesh(o.data)
        leaf = [f for f in bm.faces if f.material_index in li]
        seen = set(); islands = []
        for f in leaf:
            if f in seen: continue
            isl = []; st = [f]
            while st:
                g = st.pop()
                if g in seen: continue
                seen.add(g); isl.append(g)
                for e in g.edges:
                    for h in e.link_faces:
                        if h not in seen and h.material_index in li: st.append(h)
            islands.append(isl)
        rnd.shuffle(islands)
        scale = LEAF_STEP ** 0.5
        kill = []
        for k, isl in enumerate(islands):
            if k % LEAF_STEP:
                kill += isl
                continue
            vs = {v for f in isl for v in f.verts}
            c = sum((v.co for v in vs), Vector()) / len(vs)
            for v in vs: v.co = c + (v.co - c) * scale
        bmesh.ops.delete(bm, geom=kill, context="FACES")
        bm.to_mesh(o.data); bm.free()
        print("FEUILLAGE ALLÉGÉ", o.data.name, len(islands), "feuilles ->", len(islands) // LEAF_STEP + 1)
    return done
thinned = thin_tree_leaves()

# ---------- surfaces superposées (scintillement / « z-fighting ») ----------
# Dans le fichier, des faces de deux objets sont dans le même plan et tournées du même côté : encadrements de portes et
# de fenêtres au ras des murs, sols qui se chevauchent sous les portes, meubles collés, planches posées dans une caisse.
# Au casque, le GPU ne sait pas laquelle dessiner devant : la texture « clignote » ou mélange les deux.
# Correction : les encadrements sont gonflés de 4 mm selon leurs normales ; crédences et placards avancent de 3 mm ;
# pour tout autre couple, le plus petit des deux objets recule de 3 mm derrière l'autre.
FRAME_KEYS = ("Frame",)
FRONT_KEYS = ("Backsplash", "KitchenCabinets", "Countertop")
def fix_coplanar():
    import collections
    dg = bpy.context.evaluated_depsgraph_get()
    buckets = collections.defaultdict(list)
    objs = [o for o in bpy.context.scene.objects if o.type == "MESH" and o.visible_get() and not o.parent
            and not o.name.startswith(("Garden_", "Exterior_", "Ext_")) and o.data.name not in thinned
            and not any(k in o.name for k in ("Apoc_", "Rug", "Runner", "Papers", "Debris", "Puddle", "OilSpill"))]   # décor au sol : RoomBuilder.FixFloorDecals
    for o in objs:
        me = o.evaluated_get(dg).to_mesh(); mw = o.matrix_world; nmat = mw.to_3x3().inverted().transposed()
        if len(me.polygons) < 60000:
            for p in me.polygons:
                n = (nmat @ p.normal).normalized(); vs = [mw @ me.vertices[i].co for i in p.vertices]
                if p.area * abs(mw.determinant()) ** (2 / 3) < 0.002: continue
                buckets[(round(n.x, 1), round(n.y, 1), round(n.z, 1), round(n.dot(vs[0]) / 0.004))].append((o, vs, n))
        o.evaluated_get(dg).to_mesh_clear()
    def box(vs, n):
        a = Vector((1, 0, 0)) if abs(n.x) < 0.9 else Vector((0, 1, 0)); u = n.cross(a).normalized(); v = n.cross(u)
        us = [x.dot(u) for x in vs]; ws = [x.dot(v) for x in vs]
        return min(us), max(us), min(ws), max(ws)
    area = collections.Counter(); normal = {}
    for fs in buckets.values():
        if len({f[0].name for f in fs}) < 2: continue
        bx = [(f[0], box(f[1], f[2]), f[2]) for f in fs]
        for i in range(len(bx)):
            for j in range(i + 1, len(bx)):
                (a, ba, n), (b, bb, _) = bx[i], bx[j]
                if a == b or ("Wall" in a.name and "Wall" in b.name): continue
                w = min(ba[1], bb[1]) - max(ba[0], bb[0]); h = min(ba[3], bb[3]) - max(ba[2], bb[2])
                if w > 0.02 and h > 0.02:
                    k = tuple(sorted((a.name, b.name))); area[k] += w * h; normal[k] = n
    def vol(o): d = o.dimensions; return d.x * d.y * d.z + 1e-6
    done = set()
    for (an, bn), ar in area.most_common():
        if ar < 0.01: break
        a, b = bpy.data.objects[an], bpy.data.objects[bn]; n = normal[(an, bn)]
        mover = next((o for o in (a, b) if any(k in o.name for k in FRAME_KEYS + FRONT_KEYS)), None) or min((a, b), key=vol)
        if mover.name in done: continue
        done.add(mover.name)
        if any(k in mover.name for k in FRAME_KEYS):
            # gonfler de 4 mm selon les normales (« Shrink/Fatten ») : chaque face de l'encadrement passe devant la face du mur
            # dans le même plan, y compris l'appui de fenêtre au ras du bas de l'ouverture et les montants
            import bmesh
            if mover.data.users > 1: mover.data = mover.data.copy()
            bm = bmesh.new(); bm.from_mesh(mover.data)
            d = 0.004 / (max(mover.matrix_world.to_scale()) or 1.0)
            moves = [(v, v.normal * d * v.calc_shell_factor()) for v in bm.verts]
            for v, off in moves: v.co += off
            bm.to_mesh(mover.data); bm.free()
            how = "gonflé"
        else:
            sign = 1 if any(k in mover.name for k in FRONT_KEYS) else -1
            mover.location += n * (0.003 * sign)
            how = "avancé" if sign > 0 else "reculé"
        print("SUPERPOSITION", mover.name, how, "(contre", (b if mover == a else a).name, "%.2f m²)" % ar)
fix_coplanar()

# simplification des objets de décor trop lourds (uniquement à l'export)
dg = bpy.context.evaluated_depsgraph_get()
before = after = 0
simplified = []
for o in bpy.context.scene.objects:
    if o.type != "MESH" or not o.visible_get(): continue
    ev = o.evaluated_get(dg); me = ev.to_mesh()
    t = sum(len(p.vertices) - 2 for p in me.polygons); ev.to_mesh_clear()
    before += t
    cap = next((v for k, v in SMALL_TRIS.items() if o.name.startswith(k)), MAX_TRIS)
    if t > cap and not any(k in o.name for k in KEEP_FULL) and o.data.name not in thinned:
        mod = o.modifiers.new("ExportDecimate", "DECIMATE")
        mod.ratio = cap / t
        simplified.append((o.name, t))
        after += cap
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
