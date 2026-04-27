# TowerDefenseCoop — Suivi du projet

> Cours : IFT-2103 H26 — Programmation de jeu vidéo
> Équipe : 3 membres
> Moteur : Unity 6 (Universal 2D)

---

## TP2 — Interactivité (29 mars 2026)

### 📊 Note reçue (66/80 + individuel)
- Flot d'application : **15/15**
- Contrôle des agents : 11/15
- Multijoueur — Gestion des entrées : **5/5**
- Multijoueur — Affichage : **5/5**
- Agent autonome : **10/10**
- Recherche de chemin : 3/10 *(sources non remises)*
- Interface graphique : 17/20
- Fonctionnalités individuelles :
  - Léandre — Rendu interactif avancé : **17/20**
  - Marion — Personnalisation des entrées : 11/20
  - Antonin — IA : 9/20 *(sources non remises)*

### 📌 Leçons à retenir pour le TP3
- [x] **Inclure les sources dans le zip** — cause #1 de points perdus, à ne pas reproduire
- [x] Documenter clairement chaque fonctionnalité dans le PDF
- [x] Vérifier que le projet compile et se lance depuis le zip extrait

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
- [x] **Brouillard de guerre** (culling logique)
  > Les ennemis sont cachés tant qu'ils sont hors de portée des tours ou des joueurs
  > ⚠️ Doit être basé sur la position des agents, PAS sur la caméra
  > ✅ `FogOfWarManager.cs` — tuiles sombres par cellule, révélées par joueurs + tours + base
- [x] Le culling logique est présenté visuellement
  > ✅ Tuiles animées (fondu alpha progressif), ennemis cachés via SpriteRenderer.enabled
- [ ] Les éléments importants sont rendus au travers des obstacles (optionnel)
- [x] Minimap par rendu de caméra (optionnel, vaut jusqu'à 9 pts)
  > ✅ `MinimapController.cs` — camera orthographique → RenderTexture → RawImage UI, points P1 (bleu) / P2 (rouge), fog exclu via layer Ignore Raycast

### 🤖 IA avancée — Membre 1 `/20`
- [x] Comportements variés : rush, tank, contournement (au moins 2 niveaux de difficulté)
  > ✅ `EnemyAI.cs` — enum `EnemyType` (Rush / Tank / Flanker), stats configurées par type
  > Rush : rapide (3.5), fragile (2PV), 10or | Tank : lent (1), résistant (10PV), 25or | Flanker : moyen (2.5), 4PV, 15or
  > Récompense or distribuée aux deux joueurs à la mort via `ResourceManager`
- [x] Au moins deux niveaux de difficulté
  > ✅ `WaveManager.cs` — distribution progressive : Rush (v1-2), Rush+Tank (v3-4), Rush+Tank+Flanker (v5+)
- [x] Flanker : contournement actif des tours (A* pondéré)
  > ✅ `AStarPathfinder.FindPath(towerPenalty)` — Flanker passe penalty=50, cellules proches des tours coûtent plus cher (Chebyshev dist ≤1 : ×3, dist ≤2 : ×1)
- [ ] La planification ne présente pas de bugs majeurs
- [ ] Analyse dans le rapport

### ⭐ Personnalisation des entrées — Membre 2 `/20`
- [x] Le schéma de contrôle passe par un gestionnaire codé par l'équipe (`KeyBindingManager`)
- [x] Les touches peuvent être réassignées (clavier P1 — `ControlsRebindingUI`)
- [x] Les propriétés sont enregistrées et récupérées (`PlayerPrefs`)
- [x] Le comportement des axes peut être personnalisé (sensibilité, dead zone exposée au joueur)
  > ✅ Sliders − / + dans le menu Controls → Dead Zone [0–0.5] et Sensitivity [0.1–3], sauvegardés via PlayerPrefs
- [x] Chaque joueur peut choisir son contrôleur
  > ✅ Boutons Keyboard / Gamepad par joueur dans Controls, sauvegardés via PlayerPrefs
- [x] Rebind des boutons manette (P2)
  > ✅ Section "GAMEPAD — PLAYER 2" dans Controls, même flow que clavier (attente appui bouton, timeout 5s)

### 📄 Document TP2
- [x] Diagramme du flot d'application
- [x] Schéma de contrôle des agents (clavier + manette)
- [x] Description de l'IA de l'agent autonome (A\*)
- [x] Description des fonctionnalités supplémentaires individuelles

---

## TP3 — Rétroaction audiovisuelle (30 avril 2026)

### 🎬 Animation des agents `/15`
- [x] `EnemyAI` — flash rouge sur dégâts (`HitFlash`), états `HURT`/`DYING`
- [x] Animation des joueurs (idle, marche, flip horizontal selon direction)
- [x] Auto-add `FootstepEmitter` sur Player + ennemis (mode auto basé mouvement)
- [~] `Tower.cs` câble `_animator?.SetTrigger("fire")` dans `ShootAtEnemy` — **inerte tant qu'il n'y a pas de `Tower.controller` avec ce paramètre**
- [ ] **Créer `Tower.controller`** (states Idle / Fire, trigger `fire`) + clip `Tower_Fire.anim` (scale punch). L'attacher au prefab Tower
- [ ] (Bonus) Dupliquer les clips `.anim` Tiny Swords et ajouter des `AnimationEvent` `OnStep` pour les pas (critère "coordonné aux events")
- [ ] (Bonus) `AnimationEvent` `OnAttackHitFrame` sur le clip d'attaque ennemi (remplace le timer)

### 🎞️ Animation de l'interface `/20`
- [x] `Easing.cs` — fonctions d'easing maison (Linear, EaseOutBack, EaseOutElastic, EaseInOutSine, etc.)
- [x] `UITween.cs` — API statique : `FadeTo`, `ScaleTo`, `MoveTo`, `ColorTo`, `FillTo`, `Punch`, `CountTo`
- [x] `UITweenRunner.cs` — singleton DontDestroyOnLoad qui héberge les coroutines en `unscaledDeltaTime` (marche en pause)
- [x] `UIButtonFeedback.cs` — hover punch + click feedback + SFX, attaché aux boutons générés
- [x] `HUDManager` — fill HP tween + couleur, count ressources, slide-in du phase text, punch wave
- [x] `MainMenuController` — fade + scale elastic du panel difficulté, button feedback
- [x] `PauseMenuController` — scale-in du panel sur ouverture, button feedback
- [x] `GameOverController` — cascade in (titre → subtitle → waves)
- [x] `TowerInteractUI` — scale-in du prompt et du menu (cache la scale d'origine)

### ✨ Effets de particules (VFX) `/15`
- [x] `VFXPool.cs` — pool générique maison (Queue<GameObject>, Get/Release/Prewarm)
- [x] `VFXManager.cs` — singleton `Dictionary<VFXType, VFXPool>`, API `Play(type, pos)`
- [x] `AutoReleaseParticle.cs` — retour automatique au pool quand la particle system est terminée
- [x] `HitFlash.cs` — composant flash rouge réutilisable
- [x] `CameraShake.cs` — shake amplitude/duration avec damping, sur Camera_P2
- [x] `Projectile.cs` — projectile pooled, vol linéaire avec rotation auto vers la cible
- [x] `VFXManager.LaunchProjectile(from, to)` + `ReleaseProjectile`
- [x] 8 prefabs VFX : TowerMuzzle, TowerImpact, TowerPlace, TowerUpgrade, EnemyHit, EnemyDeath, BaseHit, ResourceGain
- [x] Prefab Projectile (sprite circle élongé, jaune)
- [x] `VFXManager` installé dans `Game.unity` avec les 8 entries câblées
- [x] Sorting layer `UI` + order 500 pour rendre par-dessus tout
- [ ] (Bonus) Effets lumineux URP `Light2D` pulse sur muzzle / torches ambiantes
- [ ] (Bonus) `Light2DPulse.cs` pour le critère « effets lumineux »

### 🔊 Ambiance sonore `/15`
- [x] `AudioMixer GameAudioMixer.mixer` créé avec 4 groupes (Master / Music / SFX / Ambient)
- [x] Paramètres exposés : `MasterVolume`, `MusicVolume`, `SFXVolume`, `AmbientVolume`
- [x] `AudioManager` singleton DontDestroyOnLoad avec crossfade musique
- [x] `AudioManager.PlayMusic(track, fadeTime)` — crossfade entre 2 AudioSources
- [x] AudioListener fallback auto sur l'AudioManager (gère le manque de listener dans Game.unity)
- [x] Subscribe à `OnPhaseChanged` pour switcher la musique automatiquement (Menu → Prep → Defense)
- [x] 6 musiques importées + câblées : `music_menu`, `music_prep`, `music_defense_light`, `music_defense_intense`, `music_victory`, `music_defeat`
- [x] 1 ambient `ambient_wind` joué en boucle pendant le Game (AudioManager étendu, sub-AudioSource sur groupe Ambient, auto-play à l'entrée Game, fade-out à la sortie)
- [x] `CREDITS.md` rédigé avec sources CC-BY 3.0 / CC0
- [x] Compresser les .wav en Vorbis dans l'Inspector Unity (Streaming, Quality 60-65, ~260 MB → ~25 MB)
- [ ] (Bonus) Importer un `ambient_torch` 3D spatialisé sur les torches pour le critère « spatialisation »

### 🔊 Effets sonores `/15`
- [x] `SFXLibrary.cs` ScriptableObject — mapping `SFXType → AudioClip[]` avec variantes random
- [x] `AudioSourcePool.cs` — pool d'AudioSources pour les SFX poolés
- [x] `MainSFXLibrary.asset` rempli avec 25 SFX (Tower, Enemy, Base, Player, UI)
- [x] `AudioManager.PlaySFX(type, worldPos)` — spatialisé 3D si `worldPos` fourni (`spatialBlend = 1f`)
- [x] Wiring complet dans `Tower`, `EnemyAI`, `BaseController`, `TowerPlacer`, `ResourceManager`
- [x] SFX UI sur les boutons (`UIButtonFeedback`)
- [x] `FootstepEmitter` auto-attaché sur Player (`PlayerFootstep`) et ennemis (`EnemyFootstep`)

### 🎚️ Volumes séparés (sliders Settings) `/bonus`
- [x] `SettingsPanelController.cs` : 4 sliders (Master/Music/SFX/Ambient) + back button. Hookup direct aux 4 méthodes `AudioManager.SetXxxVolume()`. Persistance via `PlayerPrefs` (`vol_master/music/sfx/ambient`). Mode "panneau enfant" via `Populate(panel, onBack)`.
- [x] `AudioManager.Start()` charge les `PlayerPrefs` et applique les volumes sauvegardés.
- [x] Bouton "Settings" sous Controls dans le menu Pause (`PauseMenuController.BuildMainPanel`).

### ⭐ Fonctionnalités individuelles TP3 `/20 chacun`

#### Léandre — Génération procédurale de l'environnement
- [x] `MapBlueprint.cs` ScriptableObject (largeur, hauteur, densités, seed)
- [x] `BlockVariantSet.cs` ScriptableObject (liste de variantes par type)
- [x] `MapGenerator.cs` — pose des blocs selon blueprint + seed déterministe + animation spawn `EaseOutBack`
- [ ] **Créer 3 ScriptableObjects `BlockVariantSet`** : `TreeVariants`, `BushVariants`, `RockVariants` (drag les prefabs Tiny Swords)
- [ ] **Créer un `MapBlueprint` `DefaultMap.asset`** avec les 3 sets liés
- [ ] **Installer le GameObject `MapGenerator` dans `Game.unity`** et lui assigner `DefaultMap`
- [ ] **Configurer `Project Settings → Script Execution Order → MapGenerator → -100`** (avant GridManager)
- [ ] Vérifier que les obstacles sont bien sur le sorting layer `Obstacles`
- [ ] Tester avec différents seeds → toujours jouable + variation visuelle

#### Marion — Personnalisation de l'avatar
- [x] `AvatarProfile.cs` — sérialisation PlayerPrefs (classe, couleur primaire, tint secondaire, scale, flip)
- [x] `AvatarCustomizer.cs` — application du profil (swap controller, MaterialPropertyBlock tint)
- [x] `AvatarCustomizationPanel.cs` — UI de personnalisation (le script existe dans `Scripts/UI/`)
- [x] `SpriteMaskedTint.shader` — shader maskée pour la teinte secondaire
- [ ] **Créer un material `AvatarAccessoryTint.mat`** dans `Assets/Materials/Avatar/`
- [ ] **Créer un prefab `AvatarPreview`** (basé sur un Pawn Tiny Swords) avec `AvatarCustomizer` câblé
- [ ] Remplir les `ClassSet[]` (Archer/Lancer/Warrior/Monk × 5 couleurs Tiny Swords)
- [ ] Créer une texture de masque par classe
- [ ] **Construire le panel UI dans MainMenu** : 4 boutons classe, 5 boutons couleur, 3 sliders RGB, slider scale, toggle flip
- [ ] Brancher chaque widget aux champs sérialisés du `AvatarCustomizationPanel`
- [ ] Bouton "Confirm" qui sauve via `AvatarProfile.SaveForPlayer` et applique au player en jeu

#### Antonin — Musique dynamique
- [x] `DynamicMusicDirector.cs` — singleton, écoute `OnPhaseChanged`, `OnWaveChanged`, `OnHPChanged`
- [x] États : Menu, PrepCalm, DefenseLight, DefenseIntense, Victory, Defeat
- [x] Crossfade entre 2×3 AudioSources (drums/melody/strings) avec courbe `Easing`
- [x] Synchronisation au beat via `AudioSettings.dspTime` + `bpm` + `beatsPerBar`
- [x] API `PlayJingle(Jingle id)` superposé au fond
- [x] Auto-cession à l'AudioManager si Director absent (pas de conflit)
- [ ] **Trouver/produire 3 stems pour `DefenseLight`** (drums + melody + strings) en CC-BY — `Audio/Stems/` est vide
- [ ] Trouver/produire 2-3 stems pour `Preparation`
- [ ] Trouver 5 jingles courts (WaveStart, WaveComplete, BaseLowHP, Victory, Defeat)
- [ ] **Installer le GameObject `DynamicMusicDirector` dans `Game.unity`**
- [ ] Câbler les `MusicState` entries dans l'Inspector
- [ ] Tester la transition au beat (HP base < 40% → DefenseIntense)

### 📄 Document TP3
- [ ] Section animation des agents (méthodes employées)
- [ ] Section animation de l'interface (UITween + easing maison)
- [ ] Section VFX + contexte d'utilisation + screenshots
- [ ] Section ambiance sonore (musiques, foleys, spatialisation)
- [ ] Section SFX (liste + contexte)
- [ ] Description des 3 fonctionnalités individuelles (qui a fait quoi)
- [ ] Crédits assets (CC-BY 3.0 / CC0) — repompable depuis `Audio/CREDITS.md`
- [ ] Captures d'écran in-game pour illustrer

### 📦 Remise finale
- [ ] Build Windows et/ou Mac dans `Build/`
- [ ] Tester le build sur une machine sans Unity
- [ ] Vérifier qu'aucune exception ne ressort en console
- [ ] Archive `IFT2103H26_TP3_EquipeXX.zip`
- [ ] **Inclure les sources** : `Assets/`, `Packages/`, `ProjectSettings/` (cause #1 du TP2)
- [ ] **Exclure** : `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/`, `obj/`, `.vs/`, `.idea/`, `*.blob`
- [ ] PDF à la racine du zip avec le même nom que l'archive
- [ ] `README.md` à la racine : instructions de lancement, contrôles, crédits équipe
- [ ] Vérifier que le projet ouvre proprement depuis le zip extrait
- [ ] Test final par un membre qui ne l'a pas compilé

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
| Map | Divisée en deux moitiés symétriques — 200×200 unités (−100 à 100), grille recommandée 50×50 cellules, cellSize=0.5 |
| Tours | Bloc 2×2 cellules, portée visible (cercle pointillé), layer Obstacles (détection grille) |
| Base | Centrale, partagée entre les deux joueurs — layer Default (pas Obstacles, sinon A* bloqué) |
| Ennemis | Layer Enemies ou Default — PAS Obstacles (évite de bloquer la grille) |
| Caméras | Culling Mask inclut Obstacles pour rendre les tours |
