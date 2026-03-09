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

### 🕹️ Contrôle des agents (multijoueur local) `/15`
- [x] Mouvement top-down Joueur 1 (clavier — ZQSD)
- [x] Mouvement Joueur 2 (manette Switch — Joy-Con ou Pro Controller)
- [x] Migrer vers le nouveau **Input System** (package Unity) pour les deux joueurs
- [x] Coder le **gestionnaire de contrôles maison** (traitement des inputs, pas juste les lire)
- [x] Mouvement libre sur toute la map (les deux joueurs)
- [x] Placement des tours limité à la demi-map respective de chaque joueur
- [x] Action : poser une tour (bouton dédié par joueur) — bloc **2×2 cellules**
- [ ] Action : améliorer / vendre une tour

### 🗺️ Flot d'application `/15`
- [x] Scène **Menu principal** (bouton Jouer, Quitter)
- [x] Scène **Jeu** (boucle principale)
- [x] Scène **Game Over** (victoire / défaite + retour menu)
- [x] Transitions entre scènes
- [x] Phase de **préparation** (placement des tours, timer)
- [x] Phase de **défense** (vagues d'ennemis actives, joueurs se baladent)
- [x] **Écran scindé** (split-screen local — caméra P1 gauche, caméra P2 droite)

### 🤖 Agent autonome `/10`
- [x] Créer le prefab `Enemy` (dans Unity Editor)
- [x] Implémenter le **pathfinding A\*** maison (grille + algo)
  > ⚠️ NavMesh autorisé comme source de données seulement — l'algo doit être codé par l'équipe
- [x] `GridManager.cs` — grille de navigation
- [x] `AStarPathfinder.cs` — algorithme A\*
- [x] `EnemyAI.cs` — suivi de chemin + comportement
- [x] Les ennemis recalculent leur chemin quand une tour est posée
- [x] `WaveManager.cs` — spawn des vagues, difficulté croissante
- [x] `BaseController.cs` — PV de la base, déclenchement Game Over

### 🔍 Recherche de chemin `/10`
- [x] Grille de walkability mise à jour dynamiquement (tours = obstacles)
- [x] Chemin recalculé en temps réel
- [x] Gestion des cas bloqués (chemin impossible)

### 🖥️ Interface graphique `/20`
- [x] HUD Joueur 1 (ressources, zone gauche)
- [x] HUD Joueur 2 (ressources, zone droite)
- [x] HP de la base centrale (barre de vie partagée)
- [x] Indicateur de vague actuelle et vague suivante
- [x] Timer de la phase de préparation
- [x] Écran de Game Over (score, vagues survécues)
- [x] Feedback visuel au survol d'une case (placement de tour valide/invalide)
- [x] Affichage de la portée des tours (cercle en pointillés — `TowerRangeDisplay`)
- [x] Quadrillage de la map (lignes transparentes — `GridOverlay`)

### 🌫️ Culling logique (bonus / critère qualité)
- [ ] Implémenter un **brouillard de guerre**
  > Les ennemis sont cachés tant qu'ils sont hors de portée des tours ou des joueurs  
  > ⚠️ Doit être basé sur la position des agents, PAS sur la caméra

### ⭐ Fonctionnalités individuelles `/20 chacun`
- [ ] **Membre 1** — IA avancée (comportements variés : rush, tank, contournement)
- [ ] **Membre 2** — Personnalisation des entrées (rebind des touches/boutons)

### 📄 Document TP2
- [ ] Diagramme du flot d'application
- [ ] Schéma de contrôle des agents (clavier + manette)
- [ ] Description de l'IA de l'agent autonome (A\*)
- [ ] Description des fonctionnalités supplémentaires individuelles

---

## TP3 — Rétroaction audiovisuelle (30 avril 2026)

### 🎬 Animation `/` 
- [ ] Animation des ennemis (marche, mort)
- [ ] Animation des tours (rotation vers cible, tir)
- [ ] Animation des joueurs (idle, marche)
- [ ] Animation de l'interface (transitions HUD, pop-ups)

### ✨ Effets de particules
- [ ] Explosion à la mort d'un ennemi
- [ ] Effet de tir des tours
- [ ] Effet de destruction d'une tour
- [ ] Effet visuel sur la base quand elle prend des dégâts

### 🔊 Audio
- [ ] Musique de fond (loop)
- [ ] Son de tir des tours
- [ ] Son de mort des ennemis
- [ ] Son de placement d'une tour
- [ ] Son de dégâts sur la base
- [ ] Son de Game Over / Victoire

### ⭐ Fonctionnalités individuelles TP3 `/20 chacun`
- [ ] **Membre 1** — Génération procédurale de la map
- [ ] **Membre 2** — Personnalisation des avatars

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
| Tours | Bloc 2×2 cellules, portée visible (cercle pointillé) |
| Base | Centrale, partagée entre les deux joueurs |
