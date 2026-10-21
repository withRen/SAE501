# Bande-son de la cinématique de fin, générée par code (comme ProcAudio.cs dans le jeu : pas de fichiers audio achetés).
# Usage : python3 Tools/blender/cinematique_son.py <dossier_sortie>   (lit <dossier_sortie>/temps.json écrit par cinematique_fin.py)
# Écrit <dossier_sortie>/son.wav (48 kHz, stéréo) : nuit et vent, pas du survivant, grille qui claque, coups des zombies
# contre la grille (aux mêmes instants que les secousses de l'image), râles, nappe grave qui monte jusqu'au fondu.
import json, os, sys, wave
import numpy as np

OUT = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else "cinematique_out")
T = json.load(open(os.path.join(OUT, "temps.json")))
RATE = 48000
END = T["end"]
N = int(END * RATE)
rng = np.random.default_rng(11)
L = np.zeros(N); R = np.zeros(N)
t_all = np.arange(N) / RATE

def lowpass(x, a):
    # filtre passe-bas d'ordre 1 (a proche de 1 : très sourd)
    y = np.empty_like(x); acc = 0.0
    for i in range(len(x)):
        acc = a * acc + (1 - a) * x[i]; y[i] = acc
    return y

def add(sig, t0, gain=1.0, pan=0.0):
    i = int(t0 * RATE)
    if i >= N: return
    sig = sig[: N - i]
    L[i:i + len(sig)] += sig * gain * (1 - max(0, pan))
    R[i:i + len(sig)] += sig * gain * (1 + min(0, pan))

def env(n, attack, decay):
    t = np.arange(n) / RATE
    return np.minimum(1, t / max(attack, 1e-4)) * np.exp(-t * decay)

# --- nuit : vent et souffle, très bas
wind = lowpass(rng.standard_normal(N), 0.995) * 6
wind *= 0.6 + 0.4 * np.sin(t_all * 0.7) ** 2
L += wind * 0.05; R += lowpass(rng.standard_normal(N), 0.995) * 6 * 0.05

# --- pas du survivant (course) : forts au passage devant la caméra du plan A, puis lointains
def step():
    n = int(0.09 * RATE)
    s = rng.standard_normal(n) * env(n, 0.002, 60)
    return lowpass(s, 0.75) * 2.5 + np.sin(2 * np.pi * 70 * np.arange(n) / RATE) * env(n, 0.001, 45) * 0.5
gate = T["gate"]
t = 0.1
while t < END:
    d = abs(t - (gate + 1.6))                 # au plus près de la caméra du plan A
    g = 0.9 / (1 + d * d * 0.8)
    if t > T["shots"][1]: g = 0.25 * max(0, 1 - (t - T["shots"][1]) / 5)     # plan B : il s'éloigne
    if g > 0.01: add(step(), t + rng.uniform(-0.01, 0.01), g, pan=np.clip((t - gate - 1.6) * 0.5, -0.8, 0.8))
    t += 0.31

# --- métal : grille qui claque, coups contre les barreaux
def metal(dur, base, decay, partials=(1, 2.76, 5.40, 8.93, 13.3)):
    n = int(dur * RATE); tt = np.arange(n) / RATE
    s = sum(np.sin(2 * np.pi * base * p * tt + rng.uniform(0, 6)) * np.exp(-tt * decay * (1 + k * 0.6)) / (1 + k)
            for k, p in enumerate(partials))
    s += rng.standard_normal(n) * env(n, 0.001, 90) * 0.8                    # impact
    return s * 0.6
def rattle(dur):
    n = int(dur * RATE)
    s = np.zeros(n)
    for k in range(int(dur * 28)):
        i = int(rng.uniform(0, dur) * RATE)
        c = metal(0.08, rng.uniform(900, 2200), 60, (1, 2.3, 4.1))
        c = c[: n - i]; s[i:i + len(c)] += c * rng.uniform(0.1, 0.35) * (1 - i / n)
    return s
add(metal(2.2, 180, 3.0), T["slam"], 1.0, pan=0.1)
add(rattle(0.9), T["slam"] + 0.05, 0.7)
add(metal(1.2, 240, 5.0), T["slam"] + 0.17, 0.25)                           # rebond
for h in T["hits"]:
    add(metal(0.9, rng.uniform(150, 230), rng.uniform(5, 8)), h, rng.uniform(0.35, 0.6), pan=rng.uniform(-0.4, 0.4))
    add(rattle(0.35), h + 0.02, 0.4)
    # coup sourd (corps qui heurte la grille)
    n = int(0.25 * RATE)
    add(lowpass(rng.standard_normal(n), 0.9) * env(n, 0.002, 25) * 3, h, 0.5)

# --- râles des zombies : bruit filtré, modulé (voix rauque), à partir de leur arrivée
def groan(dur, pitch):
    n = int(dur * RATE); tt = np.arange(n) / RATE
    saw = ((tt * pitch * (1 + 0.15 * np.sin(tt * 5))) % 1.0) * 2 - 1
    s = saw * 0.5 + rng.standard_normal(n) * 0.6
    s = lowpass(s, 0.93)
    shape = np.sin(np.pi * np.minimum(1, tt / dur)) ** 0.7 * (0.7 + 0.3 * np.sin(tt * 23))
    return s * shape * 1.4
for z in T["zombies"]:
    t = z - 0.6
    while t < END - 0.8:
        add(groan(rng.uniform(0.8, 1.8), rng.uniform(55, 95)), t, rng.uniform(0.25, 0.45), pan=rng.uniform(-0.7, 0.7))
        t += rng.uniform(1.6, 3.4)

# --- nappe grave qui monte (tension), coupée au fondu
drone = (np.sin(2 * np.pi * 41 * t_all) + 0.5 * np.sin(2 * np.pi * 61.5 * t_all + 1) + 0.3 * np.sin(2 * np.pi * 82.3 * t_all)) * 0.12
drone *= np.clip((t_all - 4) / 10, 0, 1)
L += drone; R += drone

# fondus d'entrée et de sortie (comme l'image), normalisation
fade = np.clip(t_all / 0.8, 0, 1) * np.clip((END - t_all) / 1.6, 0, 1)
L *= fade; R *= fade
peak = max(np.abs(L).max(), np.abs(R).max(), 1e-6)
L = L / peak * 0.89; R = R / peak * 0.89
data = (np.stack([L, R], 1) * 32767).astype(np.int16)
with wave.open(os.path.join(OUT, "son.wav"), "wb") as w:
    w.setnchannels(2); w.setsampwidth(2); w.setframerate(RATE); w.writeframes(data.tobytes())
print("[cinematique] son écrit :", os.path.join(OUT, "son.wav"), f"{END:.1f} s")
