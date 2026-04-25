# TODO — Tower Defense IFT-2103

Suivi des travaux TP2 (rendu) et TP3 (rétroaction audiovisuelle, deadline **30 avril 2026**).

Équipe : Léandre CACARIE · Marion KAUFFMANN · Antonin LEPREST.

---

## TP2 — Interactivité (rendu, note 66/80 + individuel)

### Note reçue
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

### Leçons à retenir pour le TP3
- [x] **Inclure les sources dans le zip** — cause #1 de points perdus, à ne pas reproduire
- [x] Documenter clairement chaque fonctionnalité dans le PDF
- [x] Vérifier que le projet compile et se lance depuis le zip extrait

---

## TP3 — Rétroaction audiovisuelle (en cours)

### Pilier 1 — Animation des agents /15

- [ ] Branchement de l'`Animator` du Player (`PlayerController`) — `isMoving`, `speed`, triggers `place`/`interact`, flip horizontal selon direction
- [x] `EnemyAI` — flash rouge sur dégâts (`HitFlash`), états `HURT`/`DYING` via `_dying` gate
- [x] Auto-add `FootstepEmitter` sur Player et ennemis (mode auto basé sur le mouvement, pas besoin d'AnimationEvent)
- [~] `Tower.cs` câble `_animator?.SetTrigger("fire")` dans `ShootAtEnemy` — **inerte tant qu'il n'y a pas de `Tower.controller` avec ce paramètre**
- [ ] **Créer `Tower.controller`** (states Idle / Fire, trigger `fire`) + clip `Tower_Fire.anim` (scale punch). L'attacher au prefab Tower
- [ ] (Bonus) Dupliquer les clips `.anim` Tiny Swords et ajouter des `AnimationEvent` `OnStep` pour les pas (critère "coordonné aux events")
- [ ] (Bonus) `AnimationEvent` `OnAttackHitFrame` sur le clip d'attaque ennemi (remplace le timer)

### Pilier 2 — Animation de l'interface /20

- [x] `Easing.cs` — fonctions d'easing maison (Linear, EaseOutBack, EaseOutElastic, EaseInOutSine, etc.)
- [x] `UITween.cs` — API statique : `FadeTo`, `ScaleTo`, `MoveTo`, `ColorTo`, `FillTo`, `Punch`, `CountTo`
- [x] `UITweenRunner.cs` — singleton DontDestroyOnLoad qui héberge les coroutines en `unscaledDeltaTime` (marche en pause)
- [x] `UIButtonFeedback.cs` — hover punch + click feedback + SFX, attaché aux boutons générés
- [x] `HUDManager` — fill HP tween + couleur, count ressources, slide-in du phase text, punch wave
- [x] `MainMenuController` — fade + scale elastic du panel difficulté, button feedback
- [x] `PauseMenuController` — scale-in du panel sur ouverture, button feedback
- [x] `GameOverController` — cascade in (titre → subtitle → waves)
- [x] `TowerInteractUI` — scale-in du prompt et du menu (cache la scale d'origine)

### Pilier 3 — Effets visuels /15

- [x] `VFXPool.cs` — pool générique maison (Queue<GameObject>, Get/Release/Prewarm)
- [x] `VFXManager.cs` — singleton `Dictionary<VFXType, VFXPool>`, API `Play(type, pos)`
- [x] `AutoReleaseParticle.cs` — retour automatique au pool quand la particle system est terminée
- [x] `HitFlash.cs` — composant flash rouge réutilisable
- [x] `CameraShake.cs` — shake amplitude/duration avec damping, attaché à la Main Camera
- [x] `Projectile.cs` — projectile pooled, vol linéaire avec rotation auto vers la cible
- [x] `VFXManager.LaunchProjectile(from, to)` + `ReleaseProjectile`
- [x] 8 prefabs VFX créés : TowerMuzzle, TowerImpact, TowerPlace, TowerUpgrade, EnemyHit, EnemyDeath, BaseHit, ResourceGain
- [x] Prefab Projectile (sprite circle élongé, jaune)
- [x] Material `particle_additive.mat` + texture `particle_circle.png` générés
- [x] `VFXManager` installé dans `Game.unity` avec les 8 entries câblées
- [x] `CameraShake` ajouté sur Camera_P2 de `Game.unity` (singleton, suffit pour que `BaseController` puisse l'appeler)
- [x] Sorting layer `UI` + order 500 pour rendre par-dessus tout
- [ ] (Bonus) Effets lumineux URP `Light2D` pulse sur muzzle / torches ambiantes
- [ ] (Bonus) Tracer un script simple `Light2DPulse.cs` pour le critère « effets lumineux »

### Pilier 4 — Ambiance sonore /15

- [x] `AudioMixer GameAudioMixer.mixer` créé avec 4 groupes (Master / Music / SFX / Ambient)
- [x] Paramètres exposés : `MasterVolume`, `MusicVolume`, `SFXVolume`, `AmbientVolume`
- [x] `AudioManager` singleton DontDestroyOnLoad avec crossfade musique
- [x] `AudioManager.PlayMusic(track, fadeTime)` — crossfade entre 2 AudioSources
- [x] AudioListener fallback auto sur l'AudioManager (gère le manque de listener dans Game.unity)
- [x] Subscribe à `OnPhaseChanged` pour switcher la musique automatiquement (Menu → Prep → Defense)
- [x] 5 musiques importées + câblées : `music_menu`, `music_prep`, `music_defense_light`, `music_defense_intense`, `music_victory`, `music_defeat`
- [x] 1 ambient importé : `ambient_wind` (à utiliser dans Game scene)
- [x] `CREDITS.md` rédigé avec sources CC-BY 3.0 / CC0
- [x] Compresser les .wav en Vorbis dans l'Inspector Unity (Streaming, Quality 60-65, ~260 MB → ~25 MB)
- [x] Jouer `ambient_wind` en boucle pendant le Game — `AudioManager` étendu avec un slot `ambientClip` + sub-AudioSource routée sur le groupe `Ambient`. Auto-play crossfade quand la scène active s'appelle `Game`, fade-out quand on en sort. Câblé dans `MainMenu.unity` (AudioManager component).
- [ ] (Bonus) Importer un `ambient_torch` 3D spatialisé sur les torches pour le critère « spatialisation »

### Pilier 5 — Effets sonores /15

- [x] `SFXLibrary.cs` ScriptableObject — mapping `SFXType → AudioClip[]` avec variantes random
- [x] `AudioSourcePool.cs` — pool d'AudioSources pour les SFX poolés
- [x] `MainSFXLibrary.asset` créé et rempli avec 25 SFX (Tower, Enemy, Base, Player, UI)
- [x] `AudioManager.PlaySFX(type, worldPos)` — spatialisé 3D si `worldPos` fourni (`spatialBlend = 1f`)
- [x] Wiring complet dans `Tower`, `EnemyAI`, `BaseController`, `TowerPlacer`, `ResourceManager`
- [x] SFX UI sur les boutons (`UIButtonFeedback`)
- [x] `FootstepEmitter` auto-attaché sur Player (`PlayerFootstep`) et ennemis (`EnemyFootstep`)

### Pilier 6 — Volumes séparés (sliders Settings)

- [x] `SettingsPanelController.cs` refait : 4 sliders (Master/Music/SFX/Ambient) + back button. Hookup direct aux 4 méthodes `AudioManager.SetXxxVolume()`. Persistance via `PlayerPrefs` (`vol_master/music/sfx/ambient`). Mode "panneau enfant" via `Populate(panel, onBack)` — ne crée plus son propre overlay.
- [x] `AudioManager.Start()` charge les `PlayerPrefs` et applique les volumes sauvegardés (les valeurs persistent entre les runs).
- [x] Bouton "Settings" ajouté **sous Controls** dans le menu Pause in-game (`PauseMenuController.BuildMainPanel`), même pattern que Controls : panel enfant du canvas Pause, navigation Back qui revient au panneau principal.

---

## Fonctionnalités individuelles /20 × 3

### Léandre — Génération procédurale de l'environnement /20

- [x] `MapBlueprint.cs` ScriptableObject (largeur, hauteur, densités, seed)
- [x] `BlockVariantSet.cs` ScriptableObject (liste de variantes par type)
- [x] `MapGenerator.cs` — pose des blocs selon blueprint + seed déterministe + animation spawn `EaseOutBack`
- [ ] **Créer 3 ScriptableObjects `BlockVariantSet`** : `TreeVariants`, `BushVariants`, `RockVariants` (drag les prefabs Tiny Swords) — actuellement seul `MainSFXLibrary.asset` existe dans `Assets/ScriptableObjects/`
- [ ] **Créer un `MapBlueprint` `DefaultMap.asset`** avec les 3 sets liés
- [ ] **Installer le GameObject `MapGenerator` dans `Game.unity`** et lui assigner `DefaultMap` (actuellement absent de la scène)
- [ ] **Configurer `Project Settings → Script Execution Order → MapGenerator → -100`** (avant GridManager) — le fichier `ProjectSettings/ScriptExecutionOrder.asset` n'existe pas encore
- [ ] Vérifier que les obstacles sont bien sur le sorting layer `Obstacles` (sinon GridManager les ignore)
- [ ] Tester avec différents seeds → toujours jouable + variation visuelle

### Marion — Personnalisation de l'avatar /20

- [x] `AvatarProfile.cs` — sérialisation PlayerPrefs (classe, couleur primaire, tint secondaire, scale, flip)
- [x] `AvatarCustomizer.cs` — application du profil sur un avatar (swap controller, MaterialPropertyBlock tint)
- [x] `AvatarCustomizationPanel.cs` — UI de personnalisation (le script existe dans `Scripts/UI/`)
- [x] `SpriteMaskedTint.shader` — shader maskée pour la teinte secondaire
- [ ] **Créer un material `AvatarAccessoryTint.mat`** dans `Assets/Materials/Avatar/` (le dossier est vide)
- [ ] **Créer un prefab `AvatarPreview`** (basé sur un Pawn Tiny Swords) avec `AvatarCustomizer` câblé — n'existe pas encore
- [ ] Remplir les `ClassSet[]` (Archer/Lancer/Warrior/Monk × 5 couleurs Tiny Swords)
- [ ] Créer une texture de masque par classe (zone accessoire à teinter)
- [ ] **Construire le panel UI dans MainMenu** : 4 boutons classe, 5 boutons couleur, 3 sliders RGB, slider scale, toggle flip — aucun `AvatarCustomizationPanel` GameObject dans `MainMenu.unity` actuellement
- [ ] Brancher chaque widget aux champs sérialisés du `AvatarCustomizationPanel`
- [ ] Bouton "Confirm" qui sauve via `AvatarProfile.SaveForPlayer` et applique au player en jeu

### Antonin — Musique dynamique /20

- [x] `DynamicMusicDirector.cs` — singleton, écoute `OnPhaseChanged`, `OnWaveChanged`, `OnHPChanged`
- [x] États : Menu, PrepCalm, DefenseLight, DefenseIntense, Victory, Defeat
- [x] Crossfade entre 2×3 AudioSources (drums/melody/strings) avec courbe `Easing`
- [x] Synchronisation au beat via `AudioSettings.dspTime` + `bpm` + `beatsPerBar`
- [x] API `PlayJingle(Jingle id)` superposé au fond
- [x] Auto-cession à l'AudioManager si Director absent (pas de conflit)
- [ ] **Trouver/produire 3 stems pour `DefenseLight`** (drums + melody + strings) en CC-BY — `Audio/Stems/` est vide
- [ ] Trouver/produire 2-3 stems pour `Preparation`
- [ ] Trouver 5 jingles courts (WaveStart, WaveComplete, BaseLowHP, Victory, Defeat)
- [ ] **Installer le GameObject `DynamicMusicDirector` dans `Game.unity`** — actuellement absent de la scène
- [ ] Câbler les `MusicState` entries dans l'Inspector (drums/melody/strings + volumes par état)
- [ ] Tester la transition au beat (HP base < 40% → DefenseIntense)

---

## Remise — Préparation finale

### Documentation
- [ ] Rédiger le PDF principal `IFT2103H26_TP3_EquipeXX.pdf` :
  - [ ] Section animation des agents (méthodes employées)
  - [ ] Section animation de l'interface (UITween + easing maison)
  - [ ] Section VFX + contexte d'utilisation + screenshots
  - [ ] Section ambiance sonore (musiques, foleys, spatialisation)
  - [ ] Section SFX (liste + contexte)
  - [ ] Description des 3 fonctionnalités individuelles (qui a fait quoi)
  - [ ] Crédits assets (CC-BY 3.0 / CC0) — repompable depuis `Audio/CREDITS.md`
- [ ] Captures d'écran in-game pour illustrer

### Build
- [ ] Build Windows et/ou Mac dans `Build/`
- [ ] Tester le build sur une machine sans Unity
- [ ] Vérifier qu'aucune exception ne ressort en console

### Packaging zip
- [ ] Nom de l'archive : `IFT2103H26_TP3_EquipeXX.zip`
- [ ] **Inclure les sources** : `Assets/`, `Packages/`, `ProjectSettings/` (cause #1 du TP2)
- [ ] **Exclure** : `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/` intermédiaire, `obj/`, `.vs/`, `.idea/`, fichiers `*.blob`
- [ ] PDF à la racine du zip avec le même nom que l'archive
- [ ] `README.md` à la racine : instructions de lancement, contrôles, crédits équipe
- [ ] Vérifier que le projet ouvre proprement depuis le zip extrait (pas de réfs manquantes)
- [ ] Test final par un membre qui ne l'a pas compilé
