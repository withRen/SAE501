# Texture répétable de haie / feuillage dense (projet SAE501), faite à partir de l'atlas de feuilles PolyHaven island_tree_02.
# Usage : python3 Tools/blender/make_hedge_texture.py <island_tree_02_leaves_diff_1k.png>
# Produit Tools/blender/textures/haie_feuillage_couleur.png et haie_feuillage_normale.png (1024 px, raccord sur les bords).
import sys, os, random
import numpy as np
from PIL import Image, ImageFilter

SIZE = 1024
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "textures")
atlas = Image.open(sys.argv[1]).convert("RGBA")

# découpe des feuilles de l'atlas (zones opaques séparées)
a = np.array(atlas)[:, :, 3] > 128
from collections import deque
seen = np.zeros_like(a); leaves = []
H, W = a.shape
for y in range(0, H, 4):
    for x in range(0, W, 4):
        if a[y, x] and not seen[y, x]:
            q = deque([(y, x)]); seen[y, x] = 1; ys = [y]; xs = [x]
            while q:
                cy, cx = q.popleft()
                for ny, nx in ((cy+1, cx), (cy-1, cx), (cy, cx+1), (cy, cx-1)):
                    if 0 <= ny < H and 0 <= nx < W and a[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = 1; q.append((ny, nx)); ys.append(ny); xs.append(nx)
            if len(ys) > 5000:
                leaves.append(atlas.crop((min(xs), min(ys), max(xs) + 1, max(ys) + 1)))
print("feuilles trouvées :", len(leaves))

rnd = random.Random(3)
col = Image.new("RGBA", (SIZE, SIZE), (14, 22, 9, 255))        # fond : creux sombre de la haie
height = Image.new("L", (SIZE, SIZE), 0)
LAYERS = 6
for layer in range(LAYERS):
    shade = 0.35 + 0.65 * (layer + 1) / LAYERS                  # les couches du fond sont plus sombres
    for _ in range(260):
        lf = rnd.choice(leaves)
        s = rnd.uniform(0.10, 0.17) * SIZE / max(lf.size)
        lf = lf.resize((max(2, int(lf.width * s)), max(2, int(lf.height * s))), Image.LANCZOS).rotate(rnd.uniform(0, 360), expand=True, resample=Image.BICUBIC)
        rgb = np.array(lf).astype(np.float32)
        rgb[:, :, :3] *= shade * rnd.uniform(0.8, 1.05)
        lf = Image.fromarray(np.clip(rgb, 0, 255).astype(np.uint8))
        hmask = lf.split()[3].point(lambda v: int((layer + 1) * 255 / LAYERS) if v > 128 else 0)
        x, y = rnd.randrange(SIZE), rnd.randrange(SIZE)
        for dx in (-SIZE, 0, SIZE):                               # raccord : la feuille déborde de l'autre côté
            for dy in (-SIZE, 0, SIZE):
                px, py = x + dx, y + dy
                if px + lf.width < 0 or py + lf.height < 0 or px > SIZE or py > SIZE: continue
                col.alpha_composite(lf, (px, py)) if px >= 0 and py >= 0 else col.paste(lf, (px, py), lf)
                height.paste(hmask, (px, py), lf.split()[3])

# teinte de nuit : vert plus sombre et moins jaune que l'atlas (proche de l'ancienne haie)
rgb = np.array(col.convert("RGB")).astype(np.float32) * np.array([0.62, 0.78, 0.58])
col = Image.fromarray(np.clip(rgb, 0, 255).astype(np.uint8))
col.save(os.path.join(OUT, "haie_feuillage_couleur.png"))
# normale à partir du relief des couches (vert = haut, convention OpenGL comme les textures PolyHaven nor_gl)
h = np.array(height.filter(ImageFilter.GaussianBlur(1.5))).astype(np.float32) / 255.0
dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 3.0
dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 3.0
n = np.dstack((-dx, dy, np.ones_like(h)))
n /= np.linalg.norm(n, axis=2, keepdims=True)
Image.fromarray(((n * 0.5 + 0.5) * 255).astype(np.uint8)).save(os.path.join(OUT, "haie_feuillage_normale.png"))
print("textures écrites dans", OUT)
