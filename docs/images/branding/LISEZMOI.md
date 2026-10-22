# Identité visuelle « The Last Escapor »

Couleurs : nuit `#06090F` → `#18233A`, os `#ECE4D3`, rouge `#FF3B2F`.
Polices (Google Fonts, libres) : **Anton** (ESCAPOR), **Barlow Condensed** ExtraBold (THE LAST, textes).

## Logo / icône
Fenêtre barricadée par deux planches, deux yeux rouges dans la fente (même code que les yeux des zombies en jeu).

| Fichier | Usage |
|---|---|
| `icone/icon_1024_carre.png` | Unity Player Settings (Default Icon), Quest / Android, iOS (pas de transparence, la plateforme arrondit) |
| `icone/icon_1024_arrondi.png` | Bureau, README, présentations |
| `icone/icon_macos_1024.png`, `icone/icon_macos.icns` | Build Mac |
| `icone/icon_windows.ico` | Build Windows (16 à 256 px, version simplifiée en dessous de 64 px) |
| `icone/android_adaptive/` | Icône adaptative Android (premier plan dans la zone sûre + fond) |
| `icone/tailles/` | 16 à 512 px ; 16-64 px utilisent la version sans veinage ni clous |
| `icone/icon.svg`, `icone/icon_petites_tailles.svg` | Vectoriel source |
| `logo/` | Logotype horizontal / vertical (transparent = pour fond sombre) |

## Cover
Plan de la cinématique de fin (les zombies bloqués derrière la grille), grille redessinée (barreaux ronds en fer forgé, rouille, bagues, traverse rivetée, plaque boulonnée : `sources/grille.py`), étalonné nuit, yeux rouges ajoutés.
- `cover/cover_github_1280x640.png` : Settings > General > Social preview (832 Ko, sous la limite de 1 Mo)
- `cover/cover_2560x1280.png` : README en haute définition
- `cover/cover_1920x1080.png` : 16:9 (itch.io, diaporama, vidéo)

`sources/` : scripts de génération (Node + Playwright, Python + Pillow).
