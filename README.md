# Journey Sesame

Jeu de plateforme 2D réalisé avec Unity dans le cadre d'un projet de cours. Le joueur traverse trois niveaux inspirés de l'université, en ramassant des pièces et en affrontant des ennemis et des obstacles.

**Jouer en ligne :** https://riimameddeb-del.itch.io/journey-sesame

## Présentation

Au lancement, le joueur choisit son personnage (Student ou Teacher), puis son arme (batte ou règle), puis le niveau à jouer. Les niveaux se débloquent un par un : le bouton passe du gris au bleu, puis au vert une fois le niveau réussi.

## Objectif d'un niveau

- Ramasser toutes les pièces.
- Éliminer tous les ennemis.
- Atteindre la ligne d'arrivée avant la fin du temps, avec au moins une vie.

Si les pièces ou les ennemis ne sont pas tous traités à l'arrivée, ou si le temps ou les vies sont épuisés, le joueur perd.

## Contrôles

| Action | Touche |
|---|---|
| Se déplacer |Fleche |
| Courir | Maj |
| Sauter | espace |
| Attaquer | F |
| Pause | Bouton Pause à l'écran |

## Fonctionnalités

- Menu d'accueil animé, avec fonds vidéo
- Choix du personnage, puis choix de l'arme
- Page de sélection des niveaux avec trois états (bloqué, actif, complété)
- 3 niveaux jouables, avec une caméra qui suit le joueur
- Obstacles : sol fragile qui s'effondre, piques, scies et masse mobiles
- Ennemis qui patrouillent
- Pièces à ramasser, compteur, minuteur, 3 vies, écrans de victoire et de Game Over
- Menu Pause (Continuer, Rejouer, Quitter)
- Musique et effets sonores
- Version jouable dans le navigateur (WebGL)

## Technologies

- Unity (2D), C#
- Export WebGL, publié sur itch.io

## Installation

1. Installer Unity Hub et la version d'Unity utilisée par le projet (indiquée dans `ProjectSettings/ProjectVersion.txt`).
2. Cloner ce dépôt : `git clone https://github.com/riimameddeb-del/JourneySesame.git`
3. Ouvrir le dossier avec Unity Hub (**Add project from disk**).
4. Ouvrir la scène `SampleScene` et lancer le jeu.

## Crédits

- **Conception et direction :** Rima Ameddeb
- **Développement (programmation) :** Claude AI
- **Fonds vidéo, décors des niveaux 2 et 3, briques et tuiles fragiles, boutons de niveau :** images générées avec Gemini et Google Flow
- **Décor du niveau 1 :** « Le kit de création de la ville » par upklyak, sur Magnific (Freepik)
- **Personnages et armes :** Easy Chara (Asset Store)
- **Ennemis et obstacles (scie, masse, piques), pièce :** BayatGames
- **Interface :** Hyper Casual UI Pack et Colorful UI Kit (Smashy Tech), Game GUI Buttons (BraveWarrior), Buttons Set (Asset Store)
- **Effets sonores :** FREE Casual Game SFX Pack (Asset Store)
- **Musiques (YouTube) :**
  - « stream cafe - boba date (1 hour) : cute music », chaîne Stream Cafe (accueil, menu, sélection des niveaux)
  - « chill café - video game music to relax/study/vibe », chaîne alf (niveaux)
