"""Génère les textures PBR « usure » (albedo, normal, métal/lissé) des objets restés en aplat.

Sources CC0 (Poly Haven) : rust_coarse_01 et dirty_concrete (dossier polyhaven_textures_v2),
stone_01_diff (textures.zip). Tout est calculé en mode « tuile » (bords raccordés).

Usage : python3 -I Tools/textures/generer_usure.py <dossier_sources> <dossier_sortie>
"""
import sys, os
import numpy as np
from PIL import Image

N = 1024
SRC, OUT = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(501)


def load(name, gray=False):
    im = Image.open(os.path.join(SRC, name)).convert('L' if gray else 'RGB').resize((N, N), Image.LANCZOS)
    return np.asarray(im, dtype=np.float32) / 255.0


def blur(a, sigma_x, sigma_y=None):
    """Flou gaussien périodique (FFT) : garde la texture raccordable."""
    sigma_y = sigma_x if sigma_y is None else sigma_y
    fy = np.fft.fftfreq(N)[:, None]
    fx = np.fft.fftfreq(N)[None, :]
    g = np.exp(-2 * (np.pi ** 2) * ((fx * sigma_x) ** 2 + (fy * sigma_y) ** 2))
    if a.ndim == 3:
        return np.stack([np.real(np.fft.ifft2(np.fft.fft2(a[..., c]) * g)) for c in range(a.shape[2])], -1)
    return np.real(np.fft.ifft2(np.fft.fft2(a) * g))


def noise(sigma, sx=None):
    n = blur(rng.standard_normal((N, N)).astype(np.float32), sigma if sx is None else sx, sigma)
    return (n - n.mean()) / (n.std() + 1e-6)


def smooth(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


def scratches(count, length, seed):
    r = np.random.default_rng(seed)
    s = np.zeros((N, N), np.float32)
    for _ in range(count):
        x, y = r.uniform(0, N, 2)
        ang = r.uniform(0, np.pi)
        L = r.uniform(0.3, 1.0) * length
        k = np.arange(int(L))
        xs = (x + np.cos(ang) * k).astype(int) % N
        ys = (y + np.sin(ang) * k).astype(int) % N
        s[ys, xs] = np.maximum(s[ys, xs], r.uniform(0.4, 1.0))
    return np.clip(blur(s, 0.7) * 3, 0, 1)


def normal_from_height(h, strength):
    dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 0.5 * strength
    dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 0.5 * strength
    # convention OpenGL (Y+) comme Unity ; l'axe image va vers le bas
    n = np.stack([-dx, dy, np.ones_like(h)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return n * 0.5 + 0.5


def save_rgb(a, name):
    Image.fromarray((np.clip(a, 0, 1) * 255 + 0.5).astype(np.uint8)).save(os.path.join(OUT, name))


def save_ms(metal, smooth_, name):
    rgba = np.stack([metal, metal, metal, smooth_], -1)
    Image.fromarray((np.clip(rgba, 0, 1) * 255 + 0.5).astype(np.uint8)).save(os.path.join(OUT, name))


# --- masques communs -------------------------------------------------------
rust = load('rust_coarse_01_diff_1k.jpg')
rust_l = rust.mean(-1)
grime_src = 1.0 - load('dirty_concrete_diff_1k.jpg', gray=True)
grime = smooth(0.25, 0.75, blur(grime_src, 6) * 0.6 + 0.4 * (noise(90) * 0.18 + 0.5))   # saleté en nappes
dust_fine = np.clip(noise(1.2) * 0.5 + 0.5, 0, 1)
chips_n = noise(10) * 0.6 + noise(3) * 0.4
chips = smooth(1.3, 1.6, chips_n)                    # éclats de peinture (~5 %)
chip_rim = np.clip(smooth(1.0, 1.3, chips_n) - chips, 0, 1)
var_low = noise(140) * 0.5 + noise(40) * 0.5           # variation lente de la teinte
scr_paint = scratches(70, 45, 1) * 0.7
scr_steel = scratches(350, 120, 2) * 0.6
brushed = noise(0.6, 60)                              # brossé horizontal
DIRT = np.array([0.20, 0.17, 0.13])

# --- famille peinture (métal peint, émail, abat-jour) ---------------------
h = -chips * 0.9 + chip_rim * 0.25 + (rust_l - rust_l.mean()) * 0.35 * chips + noise(1.5) * 0.04 + noise(25) * 0.05
save_rgb(normal_from_height(h, 6.0), 'Usure_Peinture_Normal.png')
metal_p = np.clip(scr_paint * 0.9 + chips * 0.25, 0, 1)
smooth_p = np.clip(0.42 - grime * 0.22 - chips * 0.28 + scr_paint * 0.15 + (dust_fine - 0.5) * 0.06, 0.05, 0.9)
save_ms(metal_p, smooth_p, 'Usure_Peinture_MS.png')


def painted(color, name, dirt=0.6, chip=1.0):
    c = np.array(color, np.float32)
    base = c[None, None, :] * (1 + var_low[..., None] * 0.07) * (0.95 + dust_fine[..., None] * 0.1)
    a = base * (1 - chips[..., None] * chip) + rust * 0.85 * chips[..., None] * chip
    a = a * (1 - chip_rim[..., None] * 0.25 * chip)
    a = a + (np.array([0.55, 0.55, 0.56]) - a) * (scr_paint[..., None] * 0.35)
    g = grime[..., None] * dirt
    a = a * (1 - g) + DIRT * g
    save_rgb(a, f'Usure_{name}_Albedo.png')


# couleurs = couleurs d'origine des matériaux (espace sRGB)
painted([0.12, 0.12, 0.12], 'MetalSombre')            # MetalDark, Dark : cadres CCTV, meuble, magnétoscope, clavier
painted([0.21, 0.24, 0.13], 'RadioKaki')              # RadioBody, Radio : radios kaki
painted([0.25, 0.22, 0.18], 'MetalBrun')              # DoorMetal : caisses, fûts, portes
painted([0.20, 0.27, 0.16], 'VertMoteur')             # GeneratorPaint
painted([0.42, 0.06, 0.04], 'RougeCarburant')         # FuelRed
painted([0.16, 0.17, 0.18], 'BoitierCamera')          # Mat_CCTV_CameraBody
painted([0.12, 0.14, 0.12], 'AbatJour')               # LampShade
painted([0.80, 0.78, 0.70], 'Email', dirt=0.55, chip=0.6)   # FridgeEnamel, SinkEnamel
painted([0.42, 0.38, 0.30], 'PlanTravail', dirt=0.5, chip=0.3)  # Countertop

# --- famille acier nu (pieds, poignées, chrome, étagères) -----------------
h = brushed * 0.08 + scr_steel * -0.35 + noise(2) * 0.03 + smooth(1.9, 2.3, noise(14)) * 0.3
save_rgb(normal_from_height(h, 5.0), 'Usure_Acier_Normal.png')
pits = smooth(1.9, 2.3, noise(14))
smooth_s = np.clip(0.55 + brushed * 0.05 - grime * 0.3 - pits * 0.35 + scr_steel * 0.1, 0.05, 0.9)
save_ms(np.clip(1.0 - pits * 0.7 - grime * 0.15, 0, 1), smooth_s, 'Usure_Acier_MS.png')


def steel(color, name):
    c = np.array(color, np.float32)
    a = c[None, None, :] * (1 + brushed[..., None] * 0.05 + var_low[..., None] * 0.06)
    a = a * (1 - pits[..., None]) + rust * 0.8 * pits[..., None]
    a = a + (np.array([0.75, 0.75, 0.76]) - a) * (scr_steel[..., None] * 0.35)
    g = grime[..., None] * 0.5
    a = a * (1 - g) + DIRT * g
    save_rgb(a, f'Usure_{name}_Albedo.png')


steel([0.45, 0.46, 0.48], 'Acier')                    # Metal (généré)
steel([0.60, 0.60, 0.60], 'Chrome')                   # Chrome
steel([0.32, 0.32, 0.34], 'AcierSombre')              # HammerSteel

# --- famille plastique / caoutchouc noir ----------------------------------
h = noise(0.8) * 0.06 + noise(6) * 0.03 + scr_paint * -0.1
save_rgb(normal_from_height(h, 4.0), 'Usure_Plastique_Normal.png')
save_ms(np.zeros((N, N)), np.clip(0.32 - grime * 0.2 + (dust_fine - 0.5) * 0.05, 0.05, 0.9), 'Usure_Plastique_MS.png')
c = np.array([0.045, 0.045, 0.045])
a = c[None, None, :] * (1 + var_low[..., None] * 0.1) + scr_paint[..., None] * 0.08
g = grime[..., None] * 0.35
save_rgb(a * (1 - g) + DIRT * g, 'Usure_Plastique_Albedo.png')

# --- papier (cartes, feuilles au sol) -------------------------------------
fib = noise(0.5, 3) * 0.5 + noise(3) * 0.5
stains = smooth(0.9, 1.6, noise(45)) * 0.6 + smooth(1.4, 2.0, noise(12)) * 0.4
crease = np.abs(noise(70)) < 0.04
h = noise(60) * 0.6 + noise(15) * 0.25 + fib * 0.03 - crease * 0.15
save_rgb(normal_from_height(blur(h, 1.0), 4.0), 'Usure_Papier_Normal.png')
c = np.array([0.78, 0.74, 0.62])
a = c[None, None, :] * (1 + fib[..., None] * 0.03 + var_low[..., None] * 0.05)
a = a * (1 - stains[..., None] * 0.35) + np.array([0.45, 0.33, 0.18]) * stains[..., None] * 0.35
g = grime[..., None] * 0.35
save_rgb(a * (1 - g) + DIRT * g, 'Usure_Papier_Albedo.png')

# --- pierre du jardin : albedo de textures.zip ----------------------------
if os.path.exists(os.path.join(SRC, 'stone_01_diff_1k.jpg')):
    save_rgb(load('stone_01_diff_1k.jpg'), 'Usure_Pierre_Albedo.png')

print('ok', sorted(os.listdir(OUT)))
