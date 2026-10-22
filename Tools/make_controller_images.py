# Dessine les manettes Meta Quest 3 (vue de dessus, schéma au trait) pour la page COMMANDES des menus.
# Sortie : Assets/Resources/Commandes/manette_gauche.png et manette_droite.png (512 x 768, traits blancs sur fond transparent ;
# le menu les teinte en ambre). Les points visés par les traits de rappel sont recopiés dans ControlsPage.cs (Points).
# Lancer : python3 Tools/make_controller_images.py
import os
from PIL import Image, ImageDraw, ImageFont

W, H, S = 512, 768, 4                   # taille finale, suréchantillonnage (traits lissés)
LINE = 9                                # épaisseur des traits (px de l'image finale)
FONT = "/System/Library/Fonts/Supplemental/Arial Bold.ttf"
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Commandes")

# manette droite ; la gauche est son miroir (A/B -> X/Y, bouton Meta -> bouton menu)
HEAD = (66, 85, 446, 415)               # tête (ellipse)
TRIGGER = (190, 40, 330, 120)           # gâchette, qui dépasse à l'avant
HANDLE = [(205, 380), (335, 380), (318, 700), (296, 728), (236, 728), (214, 700)]
GRIP = [(184, 420), (242, 420), (246, 640), (200, 640)]
STICK = (196, 232, 62, 40)              # centre x, y, rayon du socle, rayon du capuchon
BTN_LOW = (322, 300, 34)                # A (ou X)
BTN_HIGH = (352, 196, 34)               # B (ou Y)
SMALL = (250, 352, 18)                  # bouton Meta (droite) / menu (gauche)


def mirror_x(x, left):
    return W - x if left else x


def draw(left):
    img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    s = lambda v: int(round(v * S))
    lw = LINE * S
    white, soft = (255, 255, 255, 255), (255, 255, 255, 70)

    def box(b):
        x0, y0, x1, y1 = b
        x0, x1 = sorted((mirror_x(x0, left), mirror_x(x1, left)))
        return [s(x0), s(y0), s(x1), s(y1)]

    def poly(pts):
        return [(s(mirror_x(x, left)), s(y)) for x, y in pts]

    def circle(cx, cy, r, **kw):
        cx = mirror_x(cx, left)
        d.ellipse([s(cx - r), s(cy - r), s(cx + r), s(cy + r)], **kw)

    # gâchette (sous la tête, dépasse à l'avant), poignée, tête
    d.rounded_rectangle(box(TRIGGER), radius=s(30), fill=soft, outline=white, width=lw)
    d.polygon(poly(HANDLE), fill=(0, 0, 0, 0), outline=white, width=lw)
    d.polygon(poly(GRIP), fill=soft, outline=white, width=lw)
    d.ellipse(box(HEAD), fill=(0, 0, 0, 255), outline=white, width=lw)

    # joystick : socle et capuchon
    cx, cy, r0, r1 = STICK
    circle(cx, cy, r0, outline=white, width=lw)
    circle(cx, cy, r1, fill=soft, outline=white, width=lw)

    font = ImageFont.truetype(FONT, s(38))
    for (bx, by, br), label in ((BTN_LOW, "X" if left else "A"), (BTN_HIGH, "Y" if left else "B")):
        circle(bx, by, br, outline=white, width=lw)
        d.text((s(mirror_x(bx, left)), s(by + 1)), label, font=font, fill=white, anchor="mm")

    bx, by, br = SMALL
    circle(bx, by, br, outline=white, width=s(6))
    mx = mirror_x(bx, left)
    if left:                            # bouton menu : trois traits
        for k in (-6, 0, 6):
            d.line([(s(mx - 8), s(by + k)), (s(mx + 8), s(by + k))], fill=white, width=s(3))
    else:                               # bouton Meta : petit ovale
        d.ellipse([s(mx - 9), s(by - 5), s(mx + 9), s(by + 5)], outline=white, width=s(3))

    img = img.resize((W, H), Image.LANCZOS)
    # la tête est remplie de noir opaque pour cacher la gâchette et le haut de la poignée : on ne garde que les traits
    px = img.load()
    for y in range(H):
        for x in range(W):
            r, g, b, a = px[x, y]
            if a and r < 8:
                px[x, y] = (255, 255, 255, 0)
            elif a:
                px[x, y] = (255, 255, 255, int(a * r / 255))
    return img


def points(left):
    # points visés (u, v), origine en bas à gauche comme dans Unity
    def uv(x, y):
        return round(mirror_x(x, left) / W, 3), round(1 - y / H, 3)
    gx = (GRIP[0][0] + GRIP[3][0]) / 2
    return {
        "gachette": uv((TRIGGER[0] + TRIGGER[2]) / 2, TRIGGER[1] + 18),
        "grip": uv(gx, 540),
        "joystick": uv(STICK[0], STICK[1]),
        "bas": uv(BTN_LOW[0], BTN_LOW[1]),
        "haut": uv(BTN_HIGH[0], BTN_HIGH[1]),
        "petit": uv(SMALL[0], SMALL[1]),
    }


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for left, name in ((True, "manette_gauche"), (False, "manette_droite")):
        draw(left).save(os.path.join(OUT, name + ".png"))
        print(name, points(left))
