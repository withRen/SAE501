# Texture de la pile (pile ronde type D, 34 x 61 mm, étiquette militaire vert olive), dessinée avec Pillow.
# U = tour de la pile, V = longueur (0 = fond, pôle -, 1 = pôle +). Le maillage est fait par RoomBuilder.BatteryMesh.
#   python3 Tools/make_battery_texture.py
# Sorties : Assets/Pile/pile_couleur.png (couleur), Assets/Pile/pile_metal_lissage.png (R métal, A lissage)
import os, random
from PIL import Image, ImageDraw, ImageFilter, ImageFont

W, H = 1024, 512
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Pile")
os.makedirs(OUT, exist_ok=True)
random.seed(501)

# bandes le long de la pile (V, du fond vers le pôle +) : (fin, couleur, métal, lissage)
STEEL, OLIVE, BLACK = (150, 150, 146), (74, 82, 52), (28, 29, 26)
bands = [(0.05, STEEL, 255, 150), (0.80, OLIVE, 0, 110), (0.92, BLACK, 0, 120), (1.0, STEEL, 255, 160)]

col = Image.new("RGB", (W, H)); ms = Image.new("RGBA", (W, H))
dc, dm = ImageDraw.Draw(col), ImageDraw.Draw(ms)
v0 = 0.0
for v1, c, metal, smooth in bands:
    y0, y1 = int(H * (1 - v1)), int(H * (1 - v0))   # V = 0 en bas de l'image
    dc.rectangle([0, y0, W, y1], fill=c)
    dm.rectangle([0, y0, W, y1], fill=(metal, metal, metal, smooth))
    v0 = v1

def font(size):
    for f in ["/System/Library/Fonts/Supplemental/Arial Bold.ttf", "/System/Library/Fonts/Helvetica.ttc", "/Library/Fonts/Arial.ttf"]:
        if os.path.exists(f): return ImageFont.truetype(f, size)
    return ImageFont.load_default()

# marquages (texte couché le long de la pile, comme sur une vraie pile), deux fois autour
txt = Image.new("RGBA", (int(H * 0.75), W // 2), (0, 0, 0, 0))
dt = ImageDraw.Draw(txt)
ink = (226, 220, 190, 255)
dt.text((24, 40), "BA-30", font=font(74), fill=ink)
dt.text((24, 140), "PILE ALCALINE  1,5 V", font=font(30), fill=ink)
dt.text((24, 190), "TYPE D  -  LR20", font=font(30), fill=ink)
dt.text((24, 300), "+", font=font(70), fill=ink)
dt.text((24, 400), "NE PAS RECHARGER", font=font(22), fill=ink)
txt = txt.rotate(90, expand=True)
for k in range(2):
    col.paste(txt, (k * W // 2 + 40, int(H * (1 - 0.78))), txt)

# usure : taches de crasse, rayures, bords des bandes noircis
grime = Image.new("L", (W, H), 0); dg = ImageDraw.Draw(grime)
for _ in range(350):
    x, y, r = random.randrange(W), random.randrange(H), random.uniform(3, 22)
    dg.ellipse([x - r, y - r, x + r, y + r], fill=random.randrange(15, 60))
for _ in range(140):
    x, y = random.randrange(W), random.randrange(H)
    dg.line([x, y, x + random.uniform(-40, 40), y + random.uniform(-6, 6)], fill=random.randrange(60, 140), width=1)
grime = grime.filter(ImageFilter.GaussianBlur(6))
dark = Image.new("RGB", (W, H), (34, 30, 22))
col = Image.composite(dark, col, grime.point(lambda g: int(g * 0.8)))
# rayures claires sur l'acier et l'étiquette
dc = ImageDraw.Draw(col)
for _ in range(60):
    x, y = random.randrange(W), random.randrange(H)
    dc.line([x, y, x + random.uniform(-25, 25), y + random.uniform(-3, 3)], fill=(170, 168, 150), width=1)

# la crasse rend la surface plus mate
r, g, b, a = ms.split()
a = Image.composite(Image.new("L", (W, H), 60), a, grime)
ms = Image.merge("RGBA", (r, g, b, a))

col.save(os.path.join(OUT, "pile_couleur.png"))
ms.save(os.path.join(OUT, "pile_metal_lissage.png"))
print("ok", OUT)
