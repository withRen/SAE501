# Bande-son de la cinématique de fin (20 s), refaite à partir des SONS DU JEU et de deux sons de grille CC0 (Freesound).
# Remplace l'ancienne bande-son synthétisée (bruit et sinusoïdes) jugée « horrible » au test (docs/PROMPTS.md, journal 176).
#
# Usage : python3 Tools/blender/cinematique_son.py <video_sans_son_ou_ancienne.mp4> <video_sortie.mp4>
#   - lit les sons (ffmpeg doit être installé), mixe, règle le niveau (-16 LUFS, crête limitée à -1,5 dB) ;
#   - recopie l'image de la vidéo d'entrée sans la réencoder et remplace sa piste audio (AAC 48 kHz stéréo).
#
# Instants (mêmes valeurs que Tools/blender/cinematique_fin.py, branche feature/cinematiques, random.seed(7)) :
#   plan A 0 - 6,5 s (route, la caméra attend devant le portail), plan B 6,5 - 14 s (jardin, derrière les zombies),
#   plan C 14 - 20 s (gros plan sur les barreaux) ; le survivant claque la grille à 3,6 s ; les zombies arrivent contre la
#   grille entre 4,9 et 7,6 s et la secouent de 5,4 s à la fin ; fondu au noir de 18,4 à 20 s.
#
# Sons :
#   jeu      Assets/Audio/Zombie/Zombie_Voix_1.mp3 et _2.mp3   râles des zombies (5 + 1 râles, hauteur et place variées)
#            Assets/Audio/Music/Musique_Menu.mp3                thème du jeu, en fond, qui monte au plan C puis s'éteint
#            Assets/Resources/Sounds/flesh_zombie_hit.mp3        corps qui heurte la grille (quelques coups, très bas)
#   CC0      Tools/sons_cc0/freesound_274766_launemax_rattling_iron_gate.mp3   grille en fer secouée (lit des coups)
#            Tools/sons_cc0/freesound_494823_alex_hears_things_gate_close_rattle.mp3   grille qui claque
import os, subprocess, sys, tempfile
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
RATE = 48000
END = 20.0
N = int(END * RATE)
rng = np.random.default_rng(176)

def load(rel, rate=RATE):
    """Son -> tableau stéréo float (n, 2) via ffmpeg."""
    path = rel if os.path.isabs(rel) else os.path.join(ROOT, rel)
    raw = subprocess.run(["ffmpeg", "-v", "error", "-i", path, "-f", "f32le", "-ac", "2", "-ar", str(rate), "-"],
                         check=True, capture_output=True).stdout
    return np.frombuffer(raw, np.float32).reshape(-1, 2).astype(np.float64)

def seg(x, t0, t1, fade=0.03):
    s = x[int(t0 * RATE):int(t1 * RATE)].copy()
    f = min(int(fade * RATE), len(s) // 2)
    if f > 0:
        ramp = np.linspace(0, 1, f)[:, None]
        s[:f] *= ramp; s[-f:] *= ramp[::-1]
    return s

def pitch(x, k):
    """Change la hauteur (et la durée) d'un facteur k par rééchantillonnage : k < 1 plus grave."""
    n = int(len(x) / k)
    src = np.arange(n) * k
    return np.stack([np.interp(src, np.arange(len(x)), x[:, c]) for c in range(2)], 1)

def lowpass(x, cutoff):
    """Passe-bas d'ordre 2 (deux passes d'ordre 1) : sons lointains, étouffés."""
    a = np.exp(-2 * np.pi * cutoff / RATE)
    y = x.copy()
    for _ in range(2):
        out = np.empty_like(y); acc = np.zeros(2)
        for i in range(len(y)):
            acc = a * acc + (1 - a) * y[i]; out[i] = acc
        y = out
    return y

mix = np.zeros((N, 2))

def add(sig, t0, gain=1.0, pan=0.0):
    i = int(t0 * RATE)
    if i >= N or len(sig) == 0: return
    if i < 0: sig = sig[-i:]; i = 0
    sig = sig[: N - i]
    g = np.array([min(1, 1 - pan), min(1, 1 + pan)]) * gain     # pan -1 gauche, +1 droite
    mix[i:i + len(sig)] += sig * g

def env_at(t, keys):
    """Automation de volume : interpolation linéaire entre des points (t, gain)."""
    ts, gs = zip(*keys)
    return np.interp(t, ts, gs)

# ---------------- grille ----------------
rattle = load("Tools/sons_cc0/freesound_274766_launemax_rattling_iron_gate.mp3")
slam = load("Tools/sons_cc0/freesound_494823_alex_hears_things_gate_close_rattle.mp3")

# claquement : l'attaque du son (0,45 s dans le fichier) tombe sur l'image du claquement (3,6 s)
add(seg(slam, 0.0, 4.2, 0.01), 3.6 - 0.45, 1.7, pan=0.1)

# secousses : les passages où la grille est secouée (repérés dans le fichier), enchaînés en fondu ; deux couches décalées
# pour cinq zombies (la seconde plus à gauche, un peu plus grave : grille plus large, plus lourde)
BURSTS = [(5.55, 7.9), (8.65, 9.5), (11.6, 13.65), (15.45, 17.25), (17.55, 20.3), (22.5, 24.65), (28.05, 30.1), (31.75, 32.9)]
def bed(order, t_start, t_end):
    out = np.zeros((N, 2)); t = t_start; k = 0
    while t < min(t_end, END - 0.1):
        a, b = BURSTS[order[k % len(order)]]; k += 1
        s = seg(rattle, a - 0.05, b + 0.25, 0.12)
        i = int(t * RATE); s = s[: N - i]
        out[i:i + len(s)] += s
        t += (b - a) + 0.05
    return out
t = np.arange(N) / RATE
level = env_at(t, [(0, 0), (5.3, 0), (5.45, 0.55), (6.4, 0.6), (6.6, 0.8), (13.9, 0.8), (14.1, 1.0), (20, 1.0)])[:, None]
layer1 = bed([3, 0, 4, 2, 5, 6, 1, 7], 5.4, END)
layer2 = pitch(bed([4, 6, 2, 7, 0, 5, 3, 1], 5.9, END), 0.9)[int(5.9 * RATE * 0.1):][:N]
layer2 = np.pad(layer2, ((0, N - len(layer2)), (0, 0)))
mix += layer1 * level * np.array([0.9, 1.0])
mix += layer2 * level * np.array([0.75, 0.4]) * 0.8

# corps qui heurtent la grille : quelques coups sourds (instants de cinematique_fin.py), très bas
flesh = load("Assets/Resources/Sounds/flesh_zombie_hit.mp3")
HITS = [5.4, 6.577, 7.789, 9.133, 10.4, 11.515, 12.669, 13.947, 15.307, 16.848, 18.113, 19.079]
for h in HITS:
    add(lowpass(pitch(flesh, rng.uniform(0.7, 0.85)), 1800), h, 0.18 if h < 14 else 0.26, pan=rng.uniform(-0.4, 0.4))

# ---------------- zombies : râles ----------------
v1 = load("Assets/Audio/Zombie/Zombie_Voix_1.mp3")
v2 = load("Assets/Audio/Zombie/Zombie_Voix_2.mp3")
GROANS = [seg(v1, a, b, 0.05) for a, b in [(0.05, 1.25), (1.55, 2.75), (3.05, 3.95), (4.25, 6.75), (7.25, 8.1)]] + [seg(v2, 0, 1.41, 0.03)]
tz = 0.6; k = 0
order = rng.permutation(len(GROANS))
while tz < END - 1.0:
    g = pitch(GROANS[order[k % len(GROANS)]], rng.uniform(0.82, 1.08)); k += 1
    if tz < 6.5:                   # plan A : la horde arrive du fond du jardin, on l'entend venir de loin
        g = lowpass(g, 900 + 300 * tz); gain = 0.2 + 0.06 * tz
    elif tz < 14:                  # plan B : on est derrière eux
        gain = 0.55
    else:                          # plan C : contre les barreaux, face à nous
        gain = 0.75
    add(g, tz, gain, pan=rng.uniform(-0.6, 0.6))
    tz += rng.uniform(0.9, 1.7) if tz > 5 else rng.uniform(1.3, 2.0)

# ---------------- musique ----------------
music = load("Assets/Audio/Music/Musique_Menu.mp3")[:N]
music = np.pad(music, ((0, N - len(music)), (0, 0)))
mlev = env_at(t, [(0, 0.0), (1.0, 0.13), (3.4, 0.13), (4.6, 0.22), (13.5, 0.22), (14.5, 0.36), (18.4, 0.36), (20, 0.0)])[:, None]
mix += music * mlev

# ---------------- fondus (comme l'image) et niveau ----------------
fade = (np.clip(t / 0.8, 0, 1) * np.clip((END - t) / 1.6, 0, 1))[:, None]
mix *= fade
tmp = tempfile.mkdtemp()
raw = os.path.join(tmp, "son.f32")
(mix.astype(np.float32)).tofile(raw)
wav = os.path.join(tmp, "son.wav")
# niveau : gain fixe pour -16 LUFS intégrés (mesuré, sans compression : les nuances du mixage restent), crête limitée à -1,5 dB
inp = ["-f", "f32le", "-ar", str(RATE), "-ac", "2", "-i", raw]
meas = subprocess.run(["ffmpeg", "-hide_banner", *inp, "-af", "ebur128", "-f", "null", "-"], capture_output=True, text=True).stderr
lufs = float(meas.rsplit("I:", 1)[1].split("LUFS")[0])
subprocess.run(["ffmpeg", "-v", "error", "-y", *inp, "-af", f"volume={-16 - lufs:.2f}dB,alimiter=limit=0.84:level=false",
                "-ar", str(RATE), wav], check=True)
if len(sys.argv) >= 3:
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", sys.argv[1], "-i", wav, "-map", "0:v:0", "-map", "1:a:0",
                    "-c:v", "copy", "-c:a", "aac", "-b:a", "192k", "-ar", str(RATE), "-shortest", "-movflags", "+faststart",
                    sys.argv[2]], check=True)
    print("[cinematique] vidéo écrite :", sys.argv[2])
else:
    out = os.path.join(os.getcwd(), "cinematique_son.wav")
    os.replace(wav, out)
    print("[cinematique] son écrit :", out)
