"""Textures PBR du portail de sortie du jardin (fer forgé peint en noir, vieilli) : albedo, normal, métal/lissé.

Projection prévue (Assets/Editor/PortailFer.cs) : u = 1 m de largeur (raccordé gauche-droite), v = toute la hauteur
du battant (0 = pied, 1 = haut, 2,9 m). L'usure dépend donc de la hauteur : rouille et terre au pied (éclaboussures
de pluie, herbe), éclats de peinture partout, coulures de rouille qui descendent sous chaque éclat.

Source CC0 (Poly Haven) : rust_coarse_01 (dossier polyhaven_textures_v2).
Usage : python3 -I Tools/textures/generer_portail.py <dossier_sources> <dossier_sortie>
"""
import sys, os
import numpy as np
from PIL import Image

N = 1024
SRC, OUT = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(777)
H = 2.9                                               # hauteur du battant (m)
y = (1.0 - (np.arange(N) + 0.5) / N)[:, None] * H     # hauteur de chaque ligne (ligne 0 = haut de l'image = haut du portail)
Y = np.broadcast_to(y, (N, N))


def load(name, gray=False):
    im = Image.open(os.path.join(SRC, name)).convert('L' if gray else 'RGB').resize((N, N), Image.LANCZOS)
    return np.asarray(im, dtype=np.float32) / 255.0


def blur(a, sx, sy=None):
    sy = sx if sy is None else sy
    fy = np.fft.fftfreq(N)[:, None]
    fx = np.fft.fftfreq(N)[None, :]
    g = np.exp(-2 * (np.pi ** 2) * ((fx * sx) ** 2 + (fy * sy) ** 2))
    if a.ndim == 3:
        return np.stack([np.real(np.fft.ifft2(np.fft.fft2(a[..., c]) * g)) for c in range(a.shape[2])], -1)
    return np.real(np.fft.ifft2(np.fft.fft2(a) * g))


def noise(sx, sy=None):
    n = blur(rng.standard_normal((N, N)).astype(np.float32), sx, sy)
    return (n - n.mean()) / (n.std() + 1e-6)


def smooth(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


def normal_from_height(h, strength):
    dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 0.5 * strength
    dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 0.5 * strength
    n = np.stack([-dx, dy, np.ones_like(h)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return n * 0.5 + 0.5


def save_rgb(a, name):
    Image.fromarray((np.clip(a, 0, 1) * 255 + 0.5).astype(np.uint8)).save(os.path.join(OUT, name))


rust = load('rust_coarse_01_diff_1k.jpg')
rust_l = rust.mean(-1)
rust_dark = rust * np.array([0.55, 0.42, 0.33])        # rouille ancienne, brun foncé
rust_fresh = rust * np.array([0.85, 0.6, 0.42])       # rouille orangée des éclats récents

# --- éclats de peinture : plus nombreux au pied (pluie, herbe) et sous les traverses -------------
foot = smooth(0.55, 0.0, Y)                            # 1 au sol, 0 au-dessus de 55 cm
chips_n = noise(9) * 0.55 + noise(2.5) * 0.3 + noise(30) * 0.35
chips = smooth(1.3 - foot * 1.0, 1.6 - foot * 1.0, chips_n)
chip_rim = np.clip(smooth(1.1 - foot * 1.0, 1.4 - foot * 1.0, chips_n) - chips, 0, 1)
pitting = smooth(1.6, 2.2, noise(1.2)) * (0.4 + foot)  # piqûres de rouille sous la peinture

# --- coulures : la rouille descend sous chaque éclat (lignes vers le bas de l'image) -------------
src = np.clip(chips + pitting * 0.5, 0, 1) * (0.6 + 0.4 * np.clip(noise(0.6, 4) * 0.5 + 0.5, 0, 1))
streak = np.zeros_like(src)
acc = np.zeros(N, np.float32)
decay = 0.955 + 0.03 * np.clip(noise(3, 1000)[0] * 0.5 + 0.5, 0, 1) ** 2   # longueur variable selon la colonne
for r in range(N):
    acc = np.maximum(src[r], acc * decay)
    streak[r] = acc
streak = blur(streak, 1.2, 3.0) * np.clip(noise(1.5, 30) * 0.5 + 0.5, 0, 1)
streak = np.clip(streak - chips, 0, 1)

# --- base : peinture noire satinée, légèrement passée par le soleil en haut ----------------------
var = noise(120) * 0.5 + noise(25) * 0.5
paint = np.array([0.045, 0.046, 0.05])[None, None, :] * (1 + var[..., None] * 0.12)
paint = paint + np.array([0.03, 0.03, 0.03]) * smooth(1.2, 2.9, Y)[..., None]        # haut un peu délavé
a = paint * (1 - chip_rim[..., None] * 0.35)                                            # bord d'éclat soulevé, plus sombre
fresh = np.clip(noise(6) * 0.5 + 0.5, 0, 1)[..., None]
rust_col = rust_dark * (1 - fresh) + rust_fresh * fresh
a = a * (1 - chips[..., None]) + rust_col * chips[..., None]
a = a * (1 - pitting[..., None] * 0.6) + rust_dark * pitting[..., None] * 0.6
a = a + (np.array([0.32, 0.17, 0.08]) - a) * (streak[..., None] * 0.3)               # coulures orangées
# terre et mousse au pied (éclaboussures, herbe humide)
splash = foot * np.clip(0.6 + noise(4) * 0.4 + noise(40) * 0.3, 0, 1)
moss = smooth(0.25, 0.0, Y) * smooth(0.6, 1.4, noise(8))
a = a * (1 - splash[..., None] * 0.45) + np.array([0.16, 0.13, 0.09]) * splash[..., None] * 0.45
a = a * (1 - moss[..., None] * 0.7) + np.array([0.09, 0.12, 0.05]) * moss[..., None] * 0.7
save_rgb(a, 'Portail_Fer_Albedo.png')

# --- relief : éclats en creux, rouille granuleuse, piqûres, coups de marteau du fer forgé ----------
hammer = noise(18) * 0.06 + noise(5) * 0.03
h = -chips * 0.7 + chip_rim * 0.2 + (rust_l - rust_l.mean()) * 0.6 * np.clip(chips + pitting, 0, 1) - pitting * 0.25 + hammer
save_rgb(normal_from_height(h, 7.0), 'Portail_Fer_Normal.png')

# --- métal / lissé : peinture satinée, rouille mate, fer nu luisant au bord des éclats ------------
bare = np.clip(chip_rim * 0.5 * (1 - foot), 0, 1)
metal = np.clip(bare * 0.8, 0, 1)
smooth_ = np.clip(0.28 - chips * 0.3 - pitting * 0.2 - streak * 0.12 - splash * 0.2 + bare * 0.2 + (var * 0.03), 0.04, 0.8)
rgba = np.stack([metal, metal, metal, smooth_], -1)
Image.fromarray((np.clip(rgba, 0, 1) * 255 + 0.5).astype(np.uint8)).save(os.path.join(OUT, 'Portail_Fer_MS.png'))
print('ok', sorted(f for f in os.listdir(OUT) if f.startswith('Portail')))
