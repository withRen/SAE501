"""Textures procédurales des arènes 1v1 (Quai 9, Alvéole 17) : albedo clair (teinté par le matériau) + normal map OpenGL.

Usage : python3 Tools/textures/generer_arenes.py  → Assets/Arenas/Textures/*.png (1024 px, raccordables).
  - Conteneur : tôle ondulée verticale (8 ondes par 2 m), coulures de rouille, salissures en bas.
  - Conteneur_Porte : bout de conteneur (2 battants, 4 barres de fermeture, poignées), pour une face de 2,6 m.
  - Sacs : sacs de sable empilés en quinconce (rangées de 0,2 m, sacs de 0,6 m), toile.
  - Baril : bandes de cerclage horizontales, peinture écaillée.
  - Danger : bandes jaunes et noires à 45° (spawns, portails, groupe électrogène).
"""
import os
import numpy as np
from PIL import Image

N = 1024
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Arenas", "Textures")
rng = np.random.default_rng(17)
yy, xx = np.mgrid[0:N, 0:N] / N          # 0..1, y vers le bas de l'image


def bruit(octaves=5, base=4):
    """Bruit fractal raccordable (somme de bruits de valeurs interpolés)."""
    acc = np.zeros((N, N))
    amp, tot = 1.0, 0.0
    for o in range(octaves):
        f = base * 2 ** o
        g = rng.random((f, f))
        g = np.pad(g, ((0, 1), (0, 1)), mode="wrap")
        x = xx * f; y = yy * f
        x0 = np.floor(x).astype(int); y0 = np.floor(y).astype(int)
        tx = x - x0; ty = y - y0
        tx = tx * tx * (3 - 2 * tx); ty = ty * ty * (3 - 2 * ty)
        a = g[y0, x0]; b = g[y0, x0 + 1]; c = g[y0 + 1, x0]; d = g[y0 + 1, x0 + 1]
        acc += amp * ((a * (1 - tx) + b * tx) * (1 - ty) + (c * (1 - tx) + d * tx) * ty)
        tot += amp; amp *= 0.5
    return acc / tot


def normale(h, force):
    """Normal map OpenGL (vert vers le haut) depuis une carte de hauteur 0..1, raccordable."""
    dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * force
    dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * force
    n = np.dstack([-dx, dy, np.ones_like(h)])
    n /= np.linalg.norm(n, axis=2, keepdims=True)
    return Image.fromarray(((n * 0.5 + 0.5) * 255).astype(np.uint8))


def albedo(rgb):
    return Image.fromarray((np.clip(rgb, 0, 1) * 255).astype(np.uint8))


def sauver(nom, a, n):
    os.makedirs(OUT, exist_ok=True)
    a.save(os.path.join(OUT, nom + "_Albedo.png"))
    n.save(os.path.join(OUT, nom + "_Normal.png"))
    print("écrit", nom)


def rouille(teinte_peinture, grime):
    """Peinture claire (teintée ensuite par le matériau) + rouille brune en taches et coulures verticales."""
    peinture = np.dstack([np.full((N, N), c) for c in teinte_peinture])
    taches = np.clip((bruit(6, 6) - 0.64) * 5, 0, 1) * 0.75
    coulures = np.clip((bruit(4, 3)[:, :] * np.clip(bruit(3, 24)[0:1, :].repeat(N, 0), 0, 1) - 0.42) * 5, 0, 1) * (0.3 + 0.7 * yy)
    r = np.clip(taches + coulures * 0.8, 0, 1)[..., None]
    brun = np.array([0.62, 0.36, 0.18])
    sale = (0.85 + 0.15 * bruit(5, 8))[..., None] * (1 - grime * np.clip(yy - 0.6, 0, 1)[..., None] * 1.5)
    return (peinture * (1 - r) + brun * r) * sale, r[..., 0]


def conteneur():
    ondes = 8
    p = (xx * ondes) % 1.0
    # profil trapézoïdal de la tôle ondulée
    h = np.clip(np.minimum(p, 1 - p) * 4 - 0.5, 0, 1)
    rgb, r = rouille((0.92, 0.92, 0.92), 0.5)
    rgb *= (0.9 + 0.1 * h)[..., None]                  # creux un peu plus sombres
    sauver("Conteneur", albedo(rgb), normale(h + 0.04 * bruit(5, 16) + 0.03 * r, 18))


def conteneur_porte():
    h = np.zeros((N, N))
    rgb, r = rouille((0.88, 0.88, 0.88), 0.4)
    # cadre et joint central entre les 2 battants
    bord = (xx < 0.04) | (xx > 0.96) | (yy < 0.05) | (yy > 0.95)
    joint = np.abs(xx - 0.5) < 0.006
    h[bord] = 1.0; h[joint] = -0.6
    # nervures horizontales des battants (embouti)
    h += 0.5 * np.clip(np.sin(yy * np.pi * 10) * 2 - 1.2, 0, 1) * ~bord
    # 4 barres de fermeture verticales + poignées
    for cx in (0.16, 0.34, 0.66, 0.84):
        barre = (np.abs(xx - cx) < 0.012) & (yy > 0.06) & (yy < 0.94)
        h[barre] = 1.4
        rgb[barre] = rgb[barre] * 0.55
        poignee = (np.abs(yy - 0.55) < 0.012) & (xx > cx - 0.012) & (xx < cx + 0.07)
        h[poignee] = 1.6; rgb[poignee] *= 0.5
    rgb[bord] *= 0.75
    sauver("Conteneur_Porte", albedo(rgb), normale(np.clip(h, -1, 2) * 0.5 + 0.02 * bruit(5, 16), 15))


def sacs():
    rangs = 6                                   # 1,2 m de texture : rangées de 0,2 m
    sacs_par_rang = 2                           # sacs de 0,6 m
    ry = yy * rangs
    ligne = np.floor(ry).astype(int)
    fy = ry - ligne
    fx = (xx * sacs_par_rang + (ligne % 2) * 0.5) % 1.0
    # sac bombé : ellipse arrondie dans chaque cellule
    dx = (fx - 0.5) / 0.5; dy = (fy - 0.5) / 0.5
    d = np.clip(1 - (np.abs(dx) ** 3 + np.abs(dy) ** 2), 0, 1)
    h = np.sqrt(d)
    toile = 0.82 + 0.1 * bruit(6, 32) + 0.05 * np.sin(xx * N * 0.9) * np.sin(yy * N * 0.9)
    teinte = np.array([0.86, 0.78, 0.6])
    rgb = teinte * toile[..., None] * (0.45 + 0.55 * h)[..., None]
    sauver("Sacs", albedo(rgb), normale(h * 0.9 + 0.05 * bruit(6, 32), 10))


def baril():
    # 2 cerclages par mètre (la texture couvre 1 m)
    h = np.zeros((N, N))
    for cy in (0.18, 0.5, 0.82):
        h += np.clip(1 - np.abs(yy - cy) / 0.02, 0, 1)
    rgb, r = rouille((0.95, 0.93, 0.9), 0.3)
    ecaille = np.clip((bruit(6, 10) - 0.62) * 8, 0, 1)
    rgb = rgb * (1 - ecaille[..., None] * 0.6)
    rgb *= (0.85 + 0.15 * h)[..., None]
    sauver("Baril", albedo(rgb), normale(h + 0.05 * bruit(5, 16), 15))


def danger():
    # bandes jaunes et noires à 45° (5 paires par mètre), peinture usée
    bande = ((xx + yy) * 5) % 1.0 < 0.5
    jaune = np.array([0.95, 0.75, 0.1]); noir = np.array([0.06, 0.06, 0.06])
    rgb = np.where(bande[..., None], jaune, noir)
    use = np.clip((bruit(6, 10) - 0.6) * 5, 0, 1)[..., None]
    rgb = rgb * (1 - use) + np.array([0.35, 0.33, 0.3]) * use
    h = 0.03 * bruit(6, 24) + 0.1 * use[..., 0]
    sauver("Danger", albedo(rgb * (0.9 + 0.1 * bruit(5, 8))[..., None]), normale(h, 15))


if __name__ == "__main__":
    conteneur(); conteneur_porte(); sacs(); baril(); danger()
