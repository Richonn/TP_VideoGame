# TowerDefenseCoop — Suivi du projet

> Cours : IFT-2103 H26 — Programmation de jeu vidéo
> Équipe : 3 membres
> Moteur : Unity 6 (Universal 2D)

---

## TP2 — Interactivité (29 mars 2026)

### 🔧 Setup & Architecture
- [x] Créer le projet Unity 6 (Universal 2D)
- [x] Organiser la structure des dossiers (Scripts, Prefabs, Sprites, Audio…)
- [x] Créer la scène principale (map, séparateur, base centrale)

### 🗺️ Flot d'application `/15`
- [x] Scène **Menu principal** (bouton Jouer, Quitter)
- [x] Scène **Jeu** (boucle principale)
- [x] Scène **Game Over** (victoire / défaite + retour menu)
- [x] Transitions entre scènes
- [x] Phase de **préparation** (placement des tours, timer) — compte comme étape de configuration
- [x] Phase de **défense** (vagues d'ennemis actives, joueurs se baladent)
- [x] **Menu de pause** (Escape → pause, boutons : Resume / Settings / Controls / Restart / Back to Menu / Quit)
- [x] Retour au menu principal possible depuis la pause
- [x] **Écran de chargement** (3 pts) + progression du chargement (5 pts) → `LoadingScreenController.cs`, délai minimum 2s, barre de progression animée

### 🕹️ Contrôle des agents (multijoueur local) `/15`
- [x] Mouvement top-down Joueur 1 (clavier — ZQSD)
- [x] Mouvement Joueur 2 (manette Switch — Joy-Con ou Pro Controller)
- [x] Migrer vers le nouveau **Input System** (package Unity) pour les deux joueurs
- [x] Coder le **gestionnaire de contrôles maison** (traitement des inputs, pas juste les lire)
- [x] Isolation P1/P2 — clavier réservé à P1, manette (`Gamepad.current`) réservée à P2
- [x] Mouvement libre sur toute la map (les deux joueurs)
- [x] Placement des tours limité à la demi-map respective de chaque joueur
- [x] Action : poser une tour (bouton dédié par joueur) — bloc **2×2 cellules**
- [x] Touches binaires simulant un axe (ZQSD → vecteur directionnel normalisé)
- [ ] Action : améliorer / vendre une tour

### 👥 Multijoueur `/10`
- [x] Deux joueurs humains contrôlent des agents distincts
- [x] Chaque joueur a son contrôleur assigné (P1=clavier, P2=manette)
- [x] **Écran scindé** (split-screen local — caméra P1 gauche, caméra P2 droite)
- [x] Les agents actifs sont toujours visibles (caméras suivent les joueurs)

### 🤖 Agent autonome `/10`
- [x] Créer le prefab `Enemy` (dans Unity Editor)
- [x] `EnemyAI.cs` — suivi de chemin + comportement (marche, arrivée à la base, chemin bloqué)
- [x] Les ennemis réagissent aux actions des joueurs (recalcul du chemin quand une tour est posée)
- [x] `WaveManager.cs` — spawn des vagues, difficulté croissante
- [x] `BaseController.cs` — PV de la base, déclenchement Game Over
- [x] Comportement sans bugs majeurs

### 🔍 Recherche de chemin `/10`
- [x] Implémenter le **pathfinding A\*** maison (grille + algo)
  > ⚠️ NavMesh autorisé comme source de données seulement — l'algo doit être codé par l'équipe
- [x] `GridManager.cs` — grille de navigation
- [x] `AStarPathfinder.cs` — algorithme A\* (heuristique octile, 8-directionnel)
- [x] Grille de walkability mise à jour dynamiquement (tours = obstacles)
- [x] Chemin recalculé en temps réel
- [x] Gestion des cas bloqués (chemin impossible)
- [x] Algorithme approprié (A\*)

### 🖥️ Interface graphique `/20`
- [x] Écran d'accueil suivi d'un menu principal
- [x] Lors de la mise en pause, affichage d'un menu
- [x] HUD Joueur 1 (ressources, zone gauche)
- [x] HUD Joueur 2 (ressources, zone droite)
- [x] HP de la base centrale (barre de vie partagée)
- [x] Indicateur de vague actuelle et vague suivante
- [x] Timer de la phase de préparation
- [x] Écran de Game Over (score, vagues survécues)
- [x] Feedback visuel au survol d'une case (placement de tour valide/invalide)
- [x] Affichage de la portée des tours (cercle en pointillés — `TowerRangeDisplay`)
- [x] Quadrillage de la map (lignes transparentes — `GridOverlay`)
- [x] Sprites ennemis et tours (assets Tiny Sword)
- [x] Séparation logique jeu / affichage UI / données (GameManager, HUDManager, ResourceManager)
- [x] Configurations modifiables par menu (rebind touches via Controls dans pause)
- [x] **Écran de chargement** (voir Flot d'application)

### 🌫️ Rendu interactif avancé — Membre 3 `/20`
- [ ] **Brouillard de guerre** (culling logique)
  > Les ennemis sont cachés tant qu'ils sont hors de portée des tours ou des joueurs
  > ⚠️ Doit être basé sur la position des agents, PAS sur la caméra
- [ ] Le culling logique est présenté visuellement
- [ ] Les éléments importants sont rendus au travers des obstacles (optionnel)
- [ ] Minimap par rendu de caméra (optionnel, vaut jusqu'à 9 pts)

### 🤖 IA avancée — Membre 1 `/20`
- [ ] Comportements variés : rush, tank, contournement (au moins 2 niveaux de difficulté)
- [ ] La planification ne présente pas de bugs majeurs
- [ ] Au moins deux niveaux de difficulté
- [ ] Analyse dans le rapport

### ⭐ Personnalisation des entrées — Membre 2 `/20`
- [x] Le schéma de contrôle passe par un gestionnaire codé par l'équipe (`KeyBindingManager`)
- [x] Les touches peuvent être réassignées (clavier P1 — `ControlsRebindingUI`)
- [x] Les propriétés sont enregistrées et récupérées (`PlayerPrefs`)
- [ ] Le comportement des axes peut être personnalisé (sensibilité, inversion, dead zone exposée au joueur)
- [ ] Chaque joueur peut choisir son contrôleur (P1/P2 hardcodés actuellement)
- [ ] Rebind des boutons manette (P2)

### 📄 Document TP2
- [ ] Diagramme du flot d'application
- [ ] Schéma de contrôle des agents (clavier + manette)
- [ ] Description de l'IA de l'agent autonome (A\*)
- [ ] Description des fonctionnalités supplémentaires individuelles

---

## TP3 — Rétroaction audiovisuelle (30 avril 2026)

### 🎬 Animation `/15`
- [ ] Animation des ennemis (marche, mort)
- [ ] Animation des tours (rotation vers cible, tir)
- [ ] Animation des joueurs (idle, marche)
- [ ] Animation coordonnée aux événements (mort, placement de tour…)
- [ ] Flot d'animation sans bugs majeurs

### 🎞️ Animation de l'interface `/20`
- [ ] Animation de l'interface (transitions HUD, pop-ups)
- [ ] Animation par code écrit par l'équipe (pas juste Animator)
- [ ] Easing sur les animations
- [ ] Plusieurs types d'animations

### ✨ Effets de particules `/15`
- [ ] Explosion à la mort d'un ennemi
- [ ] Effet de tir des tours
- [ ] Effet de destruction d'une tour
- [ ] Effet visuel sur la base quand elle prend des dégâts
- [ ] Pooling des particules (système codé par l'équipe)

### 🔊 Ambiance sonore `/15`
- [ ] Musique de fond (loop)
- [ ] Volume musique ajustable séparément

### 🔊 Effets sonores `/15`
- [ ] Son de tir des tours
- [ ] Son de mort des ennemis
- [ ] Son de placement d'une tour
- [ ] Son de dégâts sur la base
- [ ] Son de Game Over / Victoire
- [ ] Volume SFX ajustable séparément

### ⭐ Fonctionnalités individuelles TP3 `/20 chacun`
- [ ] **Membre 1** — Génération procédurale de la map
- [ ] **Membre 2** — Personnalisation des avatars
- [ ] **Membre 3** — (à définir selon choix TP2)

### 📄 Document TP3
- [ ] Méthodes d'animation des agents
- [ ] Méthodes d'animation de l'interface
- [ ] Description des effets de particules et contextes
- [ ] Description de l'ambiance sonore
- [ ] Liste des effets sonores et contextes
- [ ] Description de la fonctionnalité optionnelle

---

## Notes techniques

| Sujet | Décision |
|---|---|
| Input | Nouveau Input System Unity (traitement maison obligatoire) |
| Joueur 1 | Clavier (ZQSD + touches d'action) |
| Joueur 2 | Manette Switch (Joy-Con ou Pro Controller) |
| Pathfinding | A\* maison (NavMesh comme données seulement) |
| Culling | Brouillard de guerre basé sur position des agents |
| Multijoueur | Local, deux contrôleurs physiques séparés |
| Vue | Top-down 2D |
| Map | Divisée en deux moitiés symétriques — 40×20 unités, cellules 2×2 |
| Tours | Bloc 2×2 cellules, portée visible (cercle pointillé), layer Obstacles (détection grille) |
| Base | Centrale, partagée entre les deux joueurs — layer Default (pas Obstacles, sinon A* bloqué) |
| Ennemis | Layer Enemies ou Default — PAS Obstacles (évite de bloquer la grille) |
| Caméras | Culling Mask inclut Obstacles pour rendre les tours |
