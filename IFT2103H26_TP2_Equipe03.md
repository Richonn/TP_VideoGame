# IFT-2103 H26 — TP2 : Interactivité
## Équipe 03

| Membre | Fonctionnalité individuelle |
|---|---|
| Antonin LEPREST | Intelligence artificielle avancée |
| Marion KAUFFMANN | Personnalisation des entrées |
| Léandre CACARIE | Rendu interactif / Brouillard de guerre |

---

## 1. Flot applicatif

### 1.1 Diagramme

```
┌─────────────┐
│  MainMenu   │ ◄────────────────────────────────────────┐
└──────┬──────┘                                          │
       │ Play → sélection difficulté (Easy / Hard)       │
       ▼                                                  │
┌─────────────┐                                          │
│   Loading   │  (chargement asynchrone + délai 2 s)     │
└──────┬──────┘                                          │
       ▼                                                  │
┌─────────────────────────────────────────────┐          │
│                    GAME                     │          │
│                                             │          │
│  ┌─────────────┐      ┌──────────────────┐  │          │
│  │ Preparation │ ───► │     Defense      │  │          │
│  │             │ ◄─── │  (vague active)  │  │          │
│  └─────────────┘      └────────┬─────────┘  │          │
│                                │ base HP = 0 │          │
└────────────────────────────────┼────────────┘          │
                                 ▼                        │
                        ┌────────────────┐               │
                        │    GameOver    │ ──────────────►┘
                        │  (défaite)     │  Back to Menu
                        └───────┬────────┘
                                │ Replay
                                ▼
                           (retour Game)

 ── À tout moment : Échap ──► Pause (Time.timeScale = 0)
    Pause : Resume / Controls / Restart / Back to Menu / Quit
```

### 1.2 Description des phases

**Menu principal** — Écran d'accueil avec boutons Play et Quit. Le bouton Play ouvre une fenêtre de sélection de difficulté (Easy / Hard) construite par code.

**Chargement** — Scène dédiée avec `LoadingScreenController`. Le chargement est effectué via `SceneManager.LoadSceneAsync` avec `allowSceneActivation = false`. La barre de progression affiche le minimum entre la progression réelle de l'opération asynchrone et un délai minimum de 2 secondes, garantissant que l'écran est visible. Le pourcentage est affiché numériquement.

**Préparation** — Les deux joueurs se déplacent librement et placent des tours dans leur moitié respective de la carte. Un curseur vert/rouge indique en temps réel la validité du placement. La phase se déclenche manuellement en maintenant Tab (P1) ou B (P2) pendant 1,5 secondes, représenté par une barre de progression dans le HUD.

**Défense** — Les vagues d'ennemis se lancent automatiquement. Les joueurs ne peuvent plus placer de tours mais peuvent interagir avec les tours existantes (amélioration). Lorsque tous les ennemis de la vague sont éliminés, le jeu repasse en phase de préparation pour la vague suivante. Le mode est sans fin : le jeu continue jusqu'à la destruction de la base.

**Game Over** — Déclenché par `BaseController` lorsque les HP de la base atteignent zéro. L'écran affiche le numéro de la vague atteinte et propose de rejouer ou de retourner au menu.

**Pause** — Accessible via la touche Échap à tout moment pendant le jeu. `Time.timeScale` est mis à 0. Le menu propose : reprendre, voir/modifier les contrôles, redémarrer et retourner au menu principal.

---

## 2. Schéma de contrôle des agents

### 2.1 Architecture

```
┌─────────────────────────────────────────────────────┐
│                    InputManager                     │
│  (singleton DontDestroyOnLoad, New Input System)    │
│                                                     │
│  ProcessKeyboardInput()  ◄── KeyBindingManager      │
│  ProcessGamepadInput()   ◄── Gamepad.all[index]     │
│                                                     │
│  → PlayerInputData { MoveDirection, PlaceTower,     │
│                       Interact, LaunchWave }         │
└───────────────┬─────────────────────┬───────────────┘
                │ GetInput(1)          │ GetInput(2)
                ▼                     ▼
       ┌────────────────┐   ┌────────────────┐
       │ PlayerController│   │ PlayerController│
       │    (Joueur 1)  │   │    (Joueur 2)  │
       ├────────────────┤   ├────────────────┤
       │ - Mouvement    │   │ - Mouvement    │
       │ - Interaction  │   │ - Interaction  │
       └────────┬───────┘   └────────┬───────┘
                │                    │
                ▼                    ▼
       ┌────────────────┐   ┌────────────────┐
       │  TowerPlacer   │   │  TowerPlacer   │
       │  (placement    │   │  (placement    │
       │   phase prep)  │   │   phase prep)  │
       └────────────────┘   └────────────────┘

ReadySystem ◄── GetInput(1).LaunchWaveHeld || GetInput(2).LaunchWaveHeld
PauseMenu   ◄── Input.GetKeyDown(KeyCode.Escape)
```

### 2.2 Tableau des contrôles par défaut

| Action | Joueur 1 (Clavier) | Joueur 2 (Manette) |
|---|---|---|
| Déplacement | Z / S / Q / D | Stick gauche |
| Placer une tour | E | A (South) |
| Interagir / Améliorer | F | Y (North) |
| Lancer la vague | Tab (maintenu) | B (East) (maintenu) |
| Pause | Échap | — |

### 2.3 Zones de jeu

La carte est divisée verticalement en deux moitiés. Chaque joueur ne peut placer des tours que dans sa propre zone (P1 : x ∈ [-20, 0], P2 : x ∈ [0, 20]). Les deux joueurs peuvent se déplacer librement sur toute la carte.

### 2.4 Interaction avec les tours

Lorsqu'un joueur s'approche d'une tour (rayon d'interaction : 1,5 unités), une invite flottante en espace monde apparaît au-dessus de son personnage. En appuyant sur la touche Interagir, un menu d'amélioration s'ouvre et affiche la portée et les dégâts actuels. Le bouton d'amélioration coûte 100 unités d'or au joueur concerné et augmente la portée de 0,5 et les dégâts de 1.

---

## 3. Agent autonome — Intelligence artificielle ennemie

### 3.1 Description PEAS

| Composante | Description |
|---|---|
| **Performance** | Atteindre la base en vie, infliger des dégâts, survivre le plus longtemps possible |
| **Environnement** | Grille 20×10 (2 unités/cellule), tours en tant qu'obstacles, base en tant que cible, autres agents |
| **Actuateurs** | Déplacement (`Rigidbody2D.MoveTowards` le long du chemin), attaque (`BaseController.TakeDamage`) |
| **Capteurs** | `GridManager` (structure de la grille), `AStarPathfinder` (calcul du chemin), `OnTriggerEnter2D` (détection de la base), `GridManager.OnGridUpdated` (événement de mise à jour) |

### 3.2 Machine à états

L'ennemi possède cinq états distincts :

```
              ┌──────────────────────────────────────────┐
              │              Phase Defense               │
              ▼                                          │
          WAITING                                        │
              │  Phase Defense démarre                   │
              ▼                                          │
          MOVING ◄──────────────────────────────┐        │
              │  OnTriggerEnter2D("Base")        │        │
              ▼                                  │        │
          ARRIVED                                │        │
          (attaque en boucle)                    │        │
                                                 │        │
          MOVING ──► FindPath() = null ──► BLOCKED        │
                                            │             │
                              OnGridUpdated─┘             │
                              RecalculatePath()           │
                                                          │
          Any ──► HP ≤ 0 ──► DEAD ──────────────────────►┘
```

| État | Comportement |
|---|---|
| `WAITING` | Immobile, animation idle. Attend l'événement `PhaseChanged → Defense`. |
| `MOVING` | Se déplace vers le prochain waypoint A* à sa vitesse. Animation de déplacement active. |
| `ARRIVED` | Inflige `baseDamage` points de dégâts à la base toutes les `attackCooldown` secondes. Animation d'attaque. |
| `BLOCKED` | Aucun chemin disponible (grille saturée). Attend un recalcul déclenché par `OnGridUpdated`. |
| `DEAD` | Récompense les deux joueurs en or, invoque `OnEnemyDied` et se détruit. |

### 3.3 Pathfinding A*

L'algorithme A* est implémenté par l'équipe dans `AStarPathfinder.cs`. Il opère sur la grille de `GridManager` (20×10 nœuds).

**Coûts de déplacement :**
- Orthogonal : 10
- Diagonal : 14

**Heuristique Octile :**
```
h(n) = 14 × min(dx, dy) + 10 × |dx - dy|
```
Cette heuristique est admissible et cohérente : elle ne surestime jamais le coût réel, garantissant l'optimalité du chemin trouvé.

**Recalcul dynamique :** Chaque ennemi souscrit à `GridManager.OnGridUpdated`. Lorsqu'une tour est placée, la grille est mise à jour et tous les ennemis recalculent leur chemin immédiatement, permettant au jeu de réagir en temps réel aux décisions des joueurs.

**Pénalité de contournement (Flanker) :** Les ennemis de type Flanker reçoivent un paramètre de pénalité (`towerPenalty = 50`) lors du calcul A*. Les nœuds adjacents aux tours voient leur coût augmenté de `penaltyBase × 3`, et les nœuds à distance 2 de `penaltyBase × 1`. Le Flanker contourne donc activement les tours plutôt que de les affronter.

### 3.4 Types d'ennemis

| Type | Vitesse | HP | Dégâts | Or | Comportement |
|---|---|---|---|---|---|
| **Rush** | 3,5 | 2 | 1 | 10 | Charge directement vers la base par le chemin le plus court |
| **Tank** | 1,0 | 10 | 3 | 25 | Avance lentement, absorbe les tirs, inflige des dégâts élevés |
| **Flanker** | 2,5 | 4 | 1 | 15 | Contourne les tours grâce à la pénalité A* |

### 3.5 Niveaux de difficulté

La difficulté est choisie au menu principal (Easy / Hard) et stockée dans `GameManager.Difficulty`.

**Easy :**
- Stats de base (voir tableau ci-dessus)
- Composition progressive : Rush uniquement (vagues 1-2), Rush + Tank (vagues 3-4), Rush + Tank + Flanker (vague 5+)
- Nombre d'ennemis : 5 + (vague − 1) × 2

**Hard :**
- HP × 1,75 (arrondi), vitesse × 1,25 appliqués à l'instantiation dans `ApplyDifficultyModifiers()`
- Nombre d'ennemis × 1,5 par vague
- Composition agressive dès la vague 1 : Rush + Tank (vague 1), Rush + Tank + Flanker (vague 2+)

---

## 4. Fonctionnalités individuelles

### 4.1 Intelligence artificielle avancée — Antonin LEPREST

Cette fonctionnalité couvre la conception et l'implémentation des comportements variés des agents ennemis ainsi que le système de difficulté.

**Comportements distincts par type :**
Chaque type d'ennemi possède un profil de statistiques et une stratégie de navigation différente, configurés à l'instantiation dans `ConfigureByType()`. Le Flanker est le seul type à utiliser la pénalité de contournement de tours dans le calcul A*, produisant des trajectoires réellement différentes des Rush et Tank.

**Réactivité à l'environnement :**
Les ennemis recalculent leur chemin dès qu'une tour est placée (`GridManager.OnGridUpdated → RecalculatePath()`). Cela signifie que les décisions des joueurs en phase de préparation ont un impact immédiat sur les trajectoires des ennemis en cours de vague, créant une interaction dynamique entre les deux équipes.

**Deux niveaux de difficulté :**
Le niveau est sélectionnable au menu principal. En mode Hard, les modificateurs sont appliqués au moment de l'instantiation de chaque ennemi, permettant une implémentation propre sans duplication de code. La composition des vagues change également, introduisant des unités lourdes et des contourneurs beaucoup plus tôt.

**Architecture :**
- `GameManager` : stocke et expose `DifficultyLevel`
- `EnemyAI` : `ConfigureByType()` définit les stats de base, `ApplyDifficultyModifiers()` applique les multiplicateurs de difficulté
- `WaveManager` : `PickPrefab()` sélectionne le type d'ennemi selon la vague et la difficulté, `LaunchWave()` calcule le compte d'ennemis

---

### 4.2 Personnalisation des entrées — Marion KAUFFMANN

Cette fonctionnalité couvre la conception d'un gestionnaire de contrôles personnalisé, le rebind des touches, et la configuration des axes de la manette.

**Architecture du gestionnaire de contrôles :**
`KeyBindingManager` centralise toutes les liaisons clavier et manette dans un dictionnaire indexé par `ActionType` (enum). `InputManager` consulte `KeyBindingManager` à chaque frame pour résoudre les entrées, isolant complètement la logique de jeu de la configuration des touches.

**Rebind clavier (Joueur 1) :**
7 actions sont rebindables : Move_Up, Move_Down, Move_Left, Move_Right, PlaceTower, Interact, LaunchWave. Le rebind se fait en cliquant le bouton de l'action concernée puis en appuyant sur la nouvelle touche (timeout de 5 secondes). Les touches Échap et Entrée annulent le rebind.

**Rebind manette (Joueur 2) :**
3 actions sont rebindables : PlaceTower, Interact, LaunchWave. Le rebind attend l'appui d'un bouton de la manette courante.

**Choix du contrôleur par joueur :**
Dans le panneau Controls (accessible depuis la pause), chaque joueur peut basculer entre Keyboard et Gamepad. En mode Gamepad, un sélecteur ◀/▶ permet de choisir la manette physique par index parmi les périphériques connectés (`Gamepad.all`).

**Configuration des axes de la manette :**
- Dead zone : 0,0 à 0,5, pas de 0,05 — les entrées stick en dessous du seuil sont ignorées
- Sensibilité : 0,1 à 3,0, pas de 0,1 — amplificateur appliqué à la magnitude du stick après la dead zone

**Persistance :**
Toutes les liaisons (clavier, manette) et paramètres (dead zone, sensibilité, type de contrôleur, index de manette) sont sauvegardés via `PlayerPrefs` et rechargés automatiquement au démarrage. Un bouton "Reset Defaults" réinitialise l'ensemble des valeurs.

---

### 4.3 Rendu interactif / Brouillard de guerre — Léandre CACARIE

Cette fonctionnalité couvre le brouillard de guerre basé sur la position des agents ainsi que la minimap via rendu caméra.

**Brouillard de guerre — `FogOfWarManager` :**
Une grille de `SpriteRenderer` est créée à l'initialisation, recouvrant l'intégralité de la carte (20×10 tuiles). Chaque tuile est une image opaque de couleur sombre (`sortingOrder = 10`).

**Révélation logique (culling basé sur la position) :**
À chaque frame, `UpdateVision()` calcule les zones révélées à partir de trois sources :
- Position des joueurs (rayon 5 unités)
- Position de chaque tour (rayon = portée de la tour)
- Position de la base (rayon 3 unités)

La révélation est purement logique : aucun calcul de ligne de vue (ray casting) n'est utilisé, ce qui garantit des performances stables quelle que soit la densité de la scène.

**Transition visuelle animée :**
Les tuiles ne passent pas de 0 à 1 instantanément. `AnimateTiles()` utilise `Mathf.MoveTowards` à chaque frame pour faire fondre l'alpha vers sa cible (`fadeSpeed = 4`), produisant un effet de révélation et de recouvrement progressif.

**Culling des ennemis :**
`CullEnemies()` parcourt tous les ennemis présents dans la scène et désactive leur `SpriteRenderer` si leur case est dans le brouillard (`targetAlpha ≥ 0.5`). Les ennemis sont invisibles jusqu'à ce qu'un joueur ou une tour les révèle.

**Priorité de rendu des joueurs :**
Les personnages des joueurs sont élevés à `sortingOrder = 15` (supérieur aux tuiles de brouillard à 10), garantissant qu'ils restent toujours visibles au-dessus du brouillard.

**Minimap — `MinimapController` :**
Une caméra orthographique dédiée (`MinimapCamera`) est créée par code, centrée sur la grille, avec un `cullingMask` qui exclut uniquement les couches UI et IgnoreRaycast. Elle rend la scène complète (carte, tours, ennemis) vers une `RenderTexture` affichée en tant que `RawImage` en bas au centre de l'écran (dans un canvas `ScreenSpaceOverlay`).

Des points colorés sont superposés sur la minimap pour indiquer la position exacte des joueurs en temps réel (P1 : bleu, P2 : rouge), leur position UV étant calculée à partir des coordonnées monde normalisées sur la grille.

---

## 5. Fonctionnalités en avance pour TP3

Le TP3 porte sur la rétroaction audiovisuelle (animations, particules, musique, effets sonores). Les éléments suivants ont été intégrés lors du développement du TP2 et constituent une base pour ce livrable.

### 5.1 Animation des agents

Les ennemis (`EnemyAI`) et les personnages joueurs (`PlayerController`) disposent chacun d'un composant `Animator` Unity. Les ennemis utilisent un paramètre entier `stateAnim` dont la valeur est mise à jour par `UpdateAnimator()` à chaque changement d'état de la machine à états :

| Valeur | Animation |
|---|---|
| 0 | Idle (WAITING, BLOCKED, DEAD) |
| 1 | Déplacement (MOVING) |
| 2 | Attaque (ARRIVED) |

Les transitions d'animation sont ainsi directement coordonnées aux événements de la simulation. Cette architecture est compatible avec les critères du TP3 : « les agents ont plus d'une animation » et « l'animation des agents est coordonnée aux événements de la simulation ».

### 5.2 Animation de l'interface graphique

**Barre de chargement :** `LoadingScreenController` anime en temps réel l'`anchorMax.x` d'un `RectTransform` pour représenter la progression. La valeur affichée est le minimum entre la progression asynchrone réelle et le temps écoulé, créant une animation contrainte et fluide.

**Brouillard de guerre :** `FogOfWarManager.AnimateTiles()` constitue un système d'animation procédurale de l'interface : chacune des 200 tuiles transite de façon indépendante vers son alpha cible, produisant une animation distribuée sans recours à un moteur d'animation externe.

Ces deux systèmes sont entièrement codés par l'équipe, sans utilisation de l'`Animator` Unity ni de tweens de bibliothèque tierce, ce qui correspond au critère TP3 « l'animation est effectuée par du code écrit par l'équipe ».

### 5.3 Ce qui reste à implémenter pour TP3

- Effets de particules (mort ennemi, tir de tour, destruction de la base)
- Musique de fond (menu et gameplay)
- Effets sonores (placement de tour, tir, dégâts, game over)
- Fonctionnalité individuelle : génération procédurale de l'environnement / personnalisation de l'avatar / musique dynamique réactive

---

*Document généré pour la remise du TP2 — IFT-2103 H26, Équipe 03*
*Deadline : 29 mars 2026*
