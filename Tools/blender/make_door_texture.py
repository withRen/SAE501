# Texture de porte à panneaux (projet SAE501), dans le style de la carte : bois peint écaillé (PolyHaven
# distressed_painted_planks, déjà utilisé par les portes du fichier Blender).
# Usage : python3 Tools/blender/make_door_texture.py <distressed_painted_planks_diff_1k.jpg> <distressed_painted_planks_nor_gl_1k.jpg>
# Produit Assets/Textures/Porte/porte_couleur.png et porte_normale.png (512 x 1024 : une porte de 1 m x 2 m).
import sys, os
import numpy as np
from PIL import Image, ImageFilter, ImageDraw

W, H = 512, 1024
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Textures", "Porte")
os.makedirs(OUT, exist_ok=True)
planks = Image.open(sys.argv[1]).convert("RGB")
pnorm = Image.open(sys.argv[2]).convert("RGB")

# bois des montants (vertical) et des traverses (horizontal), tiré de la texture des planches
vert = planks.resize((W, H))
horiz = planks.rotate(90, expand=True).resize((W, H))
vnor = pnorm.resize((W, H))
hn = np.array(pnorm.rotate(90, expand=True).resize((W, H))).astype(np.float32)
hn[:, :, [0, 1]] = hn[:, :, [1, 0]]; hn[:, :, 1] = 255 - hn[:, :, 1]      # la normale tourne avec l'image
hnor = Image.fromarray(hn.astype(np.uint8))

# plan de la porte (pixels) : montants 70 px, traverses haute 70, milieu 90, basse 130 ; 4 panneaux
S, T_TOP, T_MID, T_BOT = 70, 70, 90, 130
mid_y = int(H * 0.42)
panels = []
for (y0, y1) in ((T_TOP, mid_y - T_MID // 2), (mid_y + T_MID // 2, H - T_BOT)):
    panels += [(S, y0, W // 2 - 20, y1), (W // 2 + 20, y0, W - S, y1)]
mask_h = Image.new("L", (W, H), 0); d = ImageDraw.Draw(mask_h)
d.rectangle((0, 0, W, T_TOP), 255); d.rectangle((0, mid_y - T_MID // 2, W, mid_y + T_MID // 2), 255); d.rectangle((0, H - T_BOT, W, H), 255)
# cadre (montants et traverses) : bois peint lisse, sans les rainures des planches (flou dans le sens du fil)
frame_v = vert.resize((W // 8, H)).resize((W, H), Image.BICUBIC)          # fil vertical : on étire, les rainures disparaissent
frame_h = horiz.resize((W, H // 8)).resize((W, H), Image.BICUBIC)
frame = Image.composite(frame_h, frame_v, mask_h)
mask_p = Image.new("L", (W, H), 0); dp = ImageDraw.Draw(mask_p)
for r in panels: dp.rectangle(r, 255)
panel = Image.fromarray((np.array(vert.crop((40, 0, 40 + W, H)).resize((W, H))).astype(np.float32) * 0.88).astype(np.uint8))
col = Image.composite(panel, frame, mask_p)
flat = Image.new("RGB", (W, H), (128, 128, 255))
nor = Image.composite(vnor, flat, mask_p)

# relief : panneaux en creux de 12 mm avec un chanfrein de 18 px
height = np.ones((H, W), np.float32)
for (x0, y0, x1, y1) in panels:
    for i in range(18):
        t = 1.0 - (i + 1) / 18 * 0.6
        height[y0 + i:y1 - i, x0 + i:x1 - i] = t
height_img = Image.fromarray((height * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(1.2))
h = np.array(height_img).astype(np.float32) / 255
dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 6; dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 6
bevel = np.dstack((-dx, dy, np.ones_like(h))); bevel /= np.linalg.norm(bevel, axis=2, keepdims=True)
base = np.array(nor).astype(np.float32) / 127.5 - 1
n = base + np.dstack((bevel[:, :, 0], bevel[:, :, 1], np.zeros_like(h)))
n /= np.linalg.norm(n, axis=2, keepdims=True)
Image.fromarray(((n * 0.5 + 0.5) * 255).astype(np.uint8)).save(os.path.join(OUT, "porte_normale.png"))

# couleur : creux des panneaux et joints assombris (occlusion), crasse en bas, plaque de propreté métallique
c = np.array(col).astype(np.float32)
ao = np.array(height_img.filter(ImageFilter.GaussianBlur(6))).astype(np.float32) / 255
c *= (0.72 + 0.28 * ao)[:, :, None]
grime = np.clip((np.arange(H)[:, None] - H * 0.7) / (H * 0.3), 0, 1) ** 1.5
c *= (1 - 0.35 * grime)[:, :, None]
c[:, :, :] *= np.array([0.95, 0.97, 1.0])
kick0 = H - 70
plate = np.array(planks.resize((W, 70)).convert("L")).astype(np.float32)
c[kick0:, :, :] = (plate[:, :, None] * 0.35 + np.array([70, 66, 60])) * 0.8     # tôle grise rayée
c[kick0:kick0 + 3, :, :] *= 0.5
Image.fromarray(np.clip(c, 0, 255).astype(np.uint8)).save(os.path.join(OUT, "porte_couleur.png"))
print("textures écrites dans", os.path.abspath(OUT))
