# IFT-2103 H26 — TP3 : Rétroaction audiovisuelle
## Équipe 03

| Membre | Fonctionnalité individuelle |
|---|---|
| Antonin LEPREST | Audio SFX (mixer, library, settings, ambient) |
| Marion KAUFFMANN | Animations & assets visuels (avatar customization, particules) |
| Léandre CACARIE | Génération procédurale de l'environnement |

---

## 1. Animation des agents

### 1.1 Architecture

Chaque type d'agent (ennemi, joueur, tour) dispose d'un composant `Animator` Unity. La logique de gameplay met à jour les paramètres de l'`Animator` depuis le code, ce qui déclenche les transitions entre clips. Aucun comportement n'est piloté par l'`Animator` — la machine à états reste dans le code C#, l'`Animator` ne sert qu'à choisir le clip à jouer.

```
Code de gameplay              Animator (paramètres)         Clip joué
──────────────────            ──────────────────────        ──────────
EnemyAI.UpdateAnimator()  ──► SetInteger("stateAnim")  ──► Idle/Run/Attack/Death
PlayerController.Update() ──► SetBool("isMoving")
                               SetFloat("speed")        ──► Idle/Walk
PlayerController.OnPlace  ──► SetTrigger("place")       ──► Place oneshot
PlayerController.OnInter. ──► SetTrigger("interact")    ──► Interact oneshot
Tower.ShootAtEnemy()      ──► SetTrigger("fire")        ──► Fire oneshot
```

### 1.2 Animations par agent

| Agent | Paramètres Animator | Animations |
|---|---|---|
| **Ennemi** (`EnemyAI`) | `stateAnim` (int) | 0 = Idle (WAITING/BLOCKED), 1 = Run (MOVING), 2 = Attack (ARRIVED), 3 = Death (DEAD) |
| **Joueur** (`PlayerController`) | `isMoving` (bool), `speed` (float), `place` (trigger), `interact` (trigger) | Idle, Walk (vitesse blendée par `speed`), Place, Interact |
| **Tour** (`Tower`) | `fire` (trigger) | Idle, Fire |

**Flip horizontal :** les sprites des ennemis et du joueur sont inversés horizontalement selon la direction du déplacement (`transform.localScale.x *= Mathf.Sign(direction.x)`), ce qui donne un agent qui « regarde » dans la direction où il avance.

**Synchronisation aux événements de simulation :** les transitions sont déclenchées par les événements de gameplay et non par un timer interne à l'`Animator`. Quand un ennemi atteint la base, `EnemyAI.OnTriggerEnter2D` met l'état à `ARRIVED` et l'`Animator` bascule immédiatement sur l'animation d'attaque. Quand le joueur appuie sur le bouton de placement, `PlayerController.OnPlaceTower` déclenche le trigger `place` au même moment que `TowerPlacer.TryPlace`.

### 1.3 Coordination aux événements et feedback

**`HitFlash` (`Scripts/FX/HitFlash.cs`)** est un composant générique attaché aux ennemis. Quand l'ennemi prend des dégâts (`EnemyAI.TakeDamage`), tous ses `SpriteRenderer` enfants passent en rouge pendant 0,1 s puis reprennent leur teinte. Cela crée un retour visuel instantané sur l'impact, indépendant de l'`Animator`.

**`FootstepEmitter` (`Scripts/Audio/FootstepEmitter.cs`)** est attaché automatiquement au joueur et aux ennemis. Il émet un son de pas en mode auto basé sur le mouvement : si la position a changé de plus de `movementEpsilon` (0,0005 unité²) sur l'intervalle `autoInterval` (0,35 s), il joue le `SFXType` correspondant (`PlayerFootstep` ou `EnemyFootstep`) à la position de l'agent. Le son est donc directement coordonné au déplacement réel sans nécessiter d'`AnimationEvent`.

---

## 2. Animation de l'interface

### 2.1 Architecture — code maison

L'animation de l'interface est entièrement codée par l'équipe : aucune dépendance à DOTween, LeanTween ou autre bibliothèque. Trois fichiers constituent la base.

```
┌─────────────────────────────────────────────┐
│              UITweenRunner                  │
│  (singleton DontDestroyOnLoad, héberge      │
│   les coroutines en unscaledDeltaTime)      │
└────────────────────┬────────────────────────┘
                     │
         ┌───────────┴───────────┐
         ▼                       ▼
   ┌──────────┐            ┌──────────────┐
   │ Easing   │ ◄────used──│   UITween    │
   │ (11 fns) │            │ (8 APIs)     │
   └──────────┘            └──────────────┘
                                  │
                                  ▼
              FadeTo / ScaleTo / MoveTo / ColorTo
              FillTo / Punch / CountTo
```

**`Easing.cs`** définit 11 fonctions d'easing pures : `Linear`, `EaseInQuad`, `EaseOutQuad`, `EaseInOutQuad`, `EaseInCubic`, `EaseOutCubic`, `EaseInOutCubic`, `EaseInBack`, `EaseOutBack`, `EaseOutElastic`, `EaseInOutSine`. La méthode `Easing.Evaluate(Ease type, float t)` est appelée à chaque frame du tween.

**`UITween.cs`** expose 8 APIs publiques retournant des `Coroutine` :

| API | Cible | Usage |
|---|---|---|
| `FadeTo(CanvasGroup, target, dur, ease)` | `CanvasGroup.alpha` | Apparition/disparition de panels |
| `FadeTo(Graphic, alpha, dur, ease)` | Toute `Graphic` (Image, Text) | Fade individuel |
| `ScaleTo(Transform, target, dur, ease)` | `localScale` | Pop-in elastic des panels et boutons |
| `MoveTo(RectTransform, anchoredPos, dur, ease)` | `anchoredPosition` | Slide-in du texte de phase |
| `ColorTo(Graphic, target, dur, ease)` | `Graphic.color` | Flash couleur HP basse |
| `FillTo(Image, target, dur, ease)` | `Image.fillAmount` | Tween de la barre de vie |
| `Punch(Transform, strength, dur)` | `localScale` | Effet de « coup » sur les éléments importants |
| `CountTo(TMP_Text, from, to, dur, format, ease)` | Texte numérique | Compteur de ressources animé |

**`UITweenRunner.cs`** est un singleton créé à la demande, marqué `DontDestroyOnLoad`. Toutes les coroutines de tween utilisent `Time.unscaledDeltaTime` afin de continuer à fonctionner quand le jeu est en pause (`Time.timeScale = 0`). C'est essentiel pour les animations du menu de pause.

### 2.2 Catalogue des tweens utilisés

| Contrôleur | Évènement | Animation |
|---|---|---|
| `HUDManager` | Phase change | `MoveTo` du texte de phase 80 px vers le haut puis retour, `EaseOutBack` 0,5 s |
| `HUDManager` | Wave start | `Punch` sur le texte de vague |
| `HUDManager` | HP change | `FillTo` de la barre de PV avec `EaseOutCubic` 0,35 s |
| `HUDManager` | Resources change | `CountTo` du compteur P1/P2 sur 0,4 s + `Punch` |
| `MainMenuController` | Click Play | `FadeTo` du panel difficulté (0→1) 0,3 s + `ScaleTo` (0,6→1) `EaseOutBack` 0,45 s |
| `PauseMenuController` | Ouverture | `ScaleTo` (0,7→1) `EaseOutBack` 0,35 s sur le panel |
| `GameOverController` | Affichage | `ScaleTo` cascade titre → sous-titre → vagues survécues, stagger 0,15 s |
| `TowerInteractUI` | Approche tour | `ScaleTo` du prompt (0→base) `EaseOutBack` 0,25 s |
| `TowerInteractUI` | Ouverture menu | `ScaleTo` du menu (0,7→1) `EaseOutBack` 0,3 s |
| `SettingsPanelController` | Ouverture | `ScaleTo` cascade (panel maître `PauseMenuController.AnimatePanelIn`) |

### 2.3 Feedback boutons — `UIButtonFeedback`

Composant attaché à tous les boutons générés par code. Au survol (`OnPointerEnter`), le bouton passe à `localScale × 1,08` en 0,12 s avec `EaseOutBack` ; au clic (`OnPointerDown`), il descend à `× 0,95` en 0,06 s puis remonte au scale de base avec rebond élastique. Chaque interaction joue le `SFXType` correspondant : `UIHover` au survol, `UIClick` au clic. Cette cohérence visuelle et sonore est appliquée uniformément sur le menu principal, le menu pause, le panel des contrôles, et le panel de personnalisation d'avatar.

---

## 3. Effets visuels

### 3.1 Projectile visible — `Projectile.cs`

Lorsqu'une tour tire (`Tower.ShootAtEnemy`), un prefab `Projectile_Arrow` est instancié à la position de la tour. Le composant `Projectile` calcule la trajectoire en interpolation linéaire entre la tour (origine) et l'ennemi (cible). À chaque frame, `transform.position` est mis à jour le long du segment et `transform.rotation` est ajustée pour que la flèche pointe dans la direction du déplacement (`Mathf.Atan2`). À l'arrivée, le projectile applique les dégâts à l'ennemi via le callback `onArrive` et joue le `SFXType.TowerImpact` à la position d'impact, puis se détruit.

### 3.2 Particules par évènement

| Évènement | Prefab | Déclencheur |
|---|---|---|
| Amélioration de tour | particules dorées | `Tower.upgradeParticles.Play()` dans `TryUpgradeRange` |
| Base à mi-vie | fumée d'alerte | `BaseController.damageParticles.Play()` une seule fois quand HP ≤ 50 % |
| Effets ambiants Tiny Swords | `dust_1`, `dust_2`, `explosion_1`, `explosion_2`, `Heal_Effect`, `Magic_Effect` | utilisés sur les déplacements et certaines tours |
| Tir d'arc | `Projectile_Arrow` (sprite + trail) | `Tower.ShootAtEnemy` → `Instantiate` |

### 3.3 HitFlash — feedback dégâts

Le composant `HitFlash` (déjà décrit en section 1.3) constitue également un effet visuel : chaque ennemi qui prend des dégâts vire au rouge pendant 0,1 s, ce qui rend les impacts lisibles immédiatement même quand plusieurs ennemis sont à l'écran.

---

## 4. Ambiance sonore

### 4.1 Architecture du mixer

Un `AudioMixer` `GameAudioMixer.mixer` regroupe quatre groupes, tous enfants du `Master` :

```
Master ──┬── Music   (param exposé "MusicVolume")
         ├── SFX     (param exposé "SFXVolume")
         └── Ambient (param exposé "AmbientVolume")

Master lui-même : param exposé "MasterVolume"
```

Les quatre paramètres sont contrôlés à l'exécution par `AudioManager.SetMasterVolume / SetMusicVolume / SetSFXVolume / SetAmbientVolume`. Chaque setter convertit une valeur linéaire ∈ [0, 1] en décibels via `dB = Mathf.Log10(linear) × 20`, puis appelle `mixer.SetFloat(param, dB)`.

### 4.2 Musiques adaptatives

Six pistes sont câblées dans `AudioManager.musicEntries` (toutes en CC-BY 3.0, voir crédits en section 8) :

| Piste | Contexte de lecture |
|---|---|
| `music_menu` | Menu principal et écran de difficulté |
| `music_prep` | Phase de préparation (placement de tours) |
| `music_defense_light` | Phase de défense (vague active) |
| `music_defense_intense` | (réservé pour escalade dynamique) |
| `music_victory` | Écran de victoire |
| `music_defeat` | Écran de défaite |

Le changement de piste est automatique : `AudioManager.OnPhaseChanged` est appelé sur l'évènement `GameManager.OnPhaseChanged` et déclenche un `PlayMusic(track, fadeTime = 1.5s)`.

### 4.3 Crossfade entre pistes

`AudioManager` gère deux `AudioSource` (`MusicA`, `MusicB`) routées sur le groupe `Music`. À chaque appel de `PlayMusic`, le rôle « in/out » est inversé : la nouvelle source charge la nouvelle piste et est jouée à volume 0, puis une coroutine `FadeMusic` interpole linéairement les volumes des deux sources avec `Easing.EaseInOutSine` sur la durée de fade. La coroutine utilise `Time.unscaledDeltaTime`, ce qui permet au crossfade de continuer même quand le jeu est en pause. Quand la source sortante atteint 0,001, elle est arrêtée pour libérer le stream Vorbis.

### 4.4 Ambient — vent en boucle

Une `AudioSource` interne `AmbientLoop` est créée dynamiquement dans `AudioManager.Awake`, parentée à l'`AudioManager` et routée sur le groupe `Ambient`. Le clip `ambient_wind.wav` (chargé via `Resources.Load<AudioClip>("Audio/ambient_wind")`) est joué uniquement quand la scène active s'appelle `Game` :
- À l'entrée dans la scène Game, fondu en 1,5 s (volume 0 → 0,5).
- À la sortie (retour menu, défaite, victoire), fondu en 1,5 s vers 0 puis arrêt.

Cela garantit que le menu n'est pas pollué par le bruit ambiant du jeu et que la transition est fluide.

### 4.5 Volumes séparés — Settings panel

Le menu de pause expose un panel **Settings** contenant quatre sliders construits dynamiquement par `SettingsPanelController.Populate(panel, onBack)` :

| Slider | API appelée | Clé PlayerPrefs | Valeur par défaut |
|---|---|---|---|
| Master | `AudioManager.SetMasterVolume` | `vol_master` | 0,8 |
| Music | `AudioManager.SetMusicVolume` | `vol_music` | 0,7 |
| SFX | `AudioManager.SetSFXVolume` | `vol_sfx` | 0,8 |
| Ambient | `AudioManager.SetAmbientVolume` | `vol_ambient` | 0,6 |

À chaque mouvement de slider, la valeur est sauvée dans `PlayerPrefs` et propagée immédiatement au mixer. Au démarrage du jeu, `AudioManager.Start` recharge les quatre valeurs et les applique au mixer, garantissant la persistance entre les sessions. Le bouton Back déclenche `PlayerPrefs.Save()` et joue `SFXType.UIBack`.

---

## 5. Effets sonores

### 5.1 Architecture — `SFXLibrary` + `AudioSourcePool`

```
┌─────────────────────────────────────────┐
│      MainSFXLibrary.asset               │
│  (ScriptableObject SFXLibrary)          │
│                                         │
│  enum SFXType { TowerShoot, ... }       │
│  Entry[] entries :                      │
│    - type, AudioClip[] variants,        │
│      volume, pitchJitter                │
└──────────────┬──────────────────────────┘
               │ GetRandomClip(type)
               ▼
┌─────────────────────────────────────────┐
│     AudioManager.PlaySFX(type, pos)     │
│                                         │
│  var src = _sfxPool.Get();              │
│  src.clip = clip;                       │
│  src.pitch = 1 + Random(±jitter);       │
│  src.spatialBlend = pos.HasValue ? 1:0; │
│  src.transform.position = pos;          │
│  src.Play();                            │
└──────────────┬──────────────────────────┘
               │ Update() : Tick()
               ▼
┌─────────────────────────────────────────┐
│         AudioSourcePool                 │
│  (pool maison, prewarm 12)              │
│                                         │
│  Get() → dequeue or Instantiate         │
│  Tick() → auto-Release sources finished │
└─────────────────────────────────────────┘
```

`AudioSourcePool` est un pool maison construit par l'équipe. À chaque frame, `Tick()` parcourt les sources actives et libère celles dont `isPlaying == false`, ce qui évite tout `Destroy` ou allocation pendant le gameplay. La taille de pré-allocation est de 12 sources, ce qui couvre largement les pics simultanés observés (un tir + un footstep + un hit + un click UI).

### 5.2 Catalogue des SFX

Le `SFXType` énumère 16 types ; chaque type peut avoir plusieurs variantes choisies aléatoirement à chaque lecture (avec un pitch jitter pour éviter la fatigue auditive). Tous les clips sont en OGG Vorbis CC0 (packs Kenney).

| Type | Variantes | `volume` | `pitchJitter` | Contexte |
|---|---|---|---|---|
| TowerShoot | 3 | 0,8 | 0,08 | Tir d'une tour |
| TowerImpact | 2 | 0,8 | 0,08 | Impact du projectile sur l'ennemi |
| TowerPlace | 1 | 0,8 | 0,05 | Placement d'une nouvelle tour |
| TowerUpgrade | 1 | 0,8 | 0,05 | Amélioration d'une tour |
| EnemyHit | 2 | 0,55 | 0,1 | Ennemi touché par un projectile |
| EnemyDeath | 2 | 0,85 | 0,08 | Ennemi éliminé |
| EnemyFootstep | 4 | 0,4 | 0,1 | Pas d'ennemi (auto-déclenché) |
| EnemyAttack | 2 | 0,85 | 0,08 | Ennemi frappant la base |
| BaseHit | 1 | 0,9 | 0,05 | Base subissant des dégâts |
| PlayerFootstep | 4 | 0,4 | 0,1 | Pas du joueur (auto-déclenché) |
| ResourceGain | 1 | 0,6 | 0,05 | Joueur reçoit de l'or |
| UIHover | 1 | 0,4 | 0,05 | Survol d'un bouton |
| UIClick | 1 | 0,7 | 0,05 | Clic de validation |
| UIBack | 1 | 0,7 | 0,05 | Retour / fermeture |
| UIOpen | 1 | 0,7 | 0,05 | Ouverture d'un panel |
| UIWaveStart | 1 | 0,9 | 0,05 | Démarrage d'une vague |

Total : **25 fichiers OGG**, ~1 Mo compressé.

### 5.3 Spatialisation 3D

`AudioManager.PlaySFX(type, worldPos)` accepte un paramètre `Vector3?` optionnel. Quand `worldPos` est fourni, la source est configurée en 3D : `spatialBlend = 1`, `rolloffMode = AudioRolloffMode.Linear`, `minDistance = 2`, `maxDistance = 25`. La position de la source est déplacée à `worldPos`, ce qui permet à `AudioListener` (caméra) de calculer l'atténuation et le panoramique stéréo. Quand `worldPos` est `null`, la source reste en 2D (`spatialBlend = 0`), comportement utilisé pour tous les SFX d'UI.

### 5.4 Coordination aux événements

| SFXType | Call site | Spatialisé ? |
|---|---|---|
| TowerShoot | `Tower.ShootAtEnemy` | oui (position tour) |
| TowerImpact | `Projectile.OnArrive` | oui (position impact) |
| TowerPlace | `TowerPlacer.TryPlace` | oui (cellule placée) |
| TowerUpgrade | `Tower.TryUpgradeRange` | oui (position tour) |
| EnemyHit | `EnemyAI.TakeDamage` | oui (position ennemi) |
| EnemyDeath | `EnemyAI.Die` | oui (position ennemi) |
| EnemyFootstep | `FootstepEmitter.OnStep` (auto) | oui (position ennemi) |
| EnemyAttack | `EnemyAI.Update` (état ARRIVED, à chaque frappe) | oui (position ennemi) |
| BaseHit | `BaseController.TakeDamage` | oui (position base) |
| PlayerFootstep | `FootstepEmitter.OnStep` (auto) | oui (position joueur) |
| ResourceGain | `ResourceManager.Add` | non (UI 2D) |
| UIHover / UIClick | `UIButtonFeedback` (events EventSystem) | non |
| UIBack | `MainMenuController`, `PauseMenuController`, `SettingsPanelController` | non |
| UIOpen | `PauseMenuController.Pause`, `TowerInteractUI.OpenMenu` | non |
| UIWaveStart | `HUDManager.OnWaveStart` | non |

### 5.5 Footsteps automatiques

`FootstepEmitter` est attaché automatiquement au `PlayerController` et à chaque `EnemyAI` au moment de leur `Awake`. Il détecte le mouvement réel en comparant `transform.position` à la frame précédente : si la magnitude du déplacement dépasse `movementEpsilon` (0,0005), l'agent est considéré en marche et l'émetteur déclenche un pas toutes les `autoInterval` (0,35 s). Cette approche évite la dépendance aux `AnimationEvent` (qui auraient nécessité de dupliquer les clips Tiny Swords pour y ajouter des marqueurs) tout en garantissant que le son colle au déplacement effectif.

---

## 6. Fonctionnalités individuelles

### 6.1 Audio SFX — Antonin LEPREST

La fonctionnalité couvre l'ensemble de l'infrastructure audio du jeu : le mixer, le manager de lecture, la bibliothèque SFX, le pool de sources, les emitters automatiques, et le panel utilisateur de réglage.

**`AudioManager`** est un singleton `DontDestroyOnLoad` instancié une seule fois dans la scène `MainMenu`. Il survit aux changements de scène et conserve les sources de musique et d'ambient en lecture continue. Il s'abonne à `GameManager.OnPhaseChanged` pour faire les transitions musicales, à `SceneManager.sceneLoaded` pour gérer l'ambient et resynchroniser un `AudioListener` de secours si la scène n'en contient pas.

**`SFXLibrary`** est un `ScriptableObject` (`MainSFXLibrary.asset`) qui mappe chaque `SFXType` vers un tableau d'`AudioClip` variantes, un volume de base, et un pitch jitter. La méthode `GetRandomClip(SFXType)` est appelée à chaque `PlaySFX` pour piocher une variante au hasard, ce qui évite la répétition mécanique sur les actions fréquentes (footsteps, tir).

**`AudioSourcePool`** est un pool d'`AudioSource` codé par l'équipe (pas d'utilisation de `UnityEngine.Pool.ObjectPool`). Sa file `Queue<AudioSource>` est pré-remplie au démarrage avec 12 sources désactivées. Chaque `Get()` active et retourne une source ; chaque frame, `Tick()` recycle automatiquement les sources qui ont fini de jouer.

**Settings panel** (`SettingsPanelController.Populate`) construit dynamiquement à l'ouverture du sous-panneau quatre sliders Master/Music/SFX/Ambient, sauvegarde leurs valeurs dans `PlayerPrefs`, et applique en temps réel au mixer. Les valeurs persistent entre les sessions et sont rechargées par `AudioManager.Start`.

**Ambient** (`ambient_wind.wav`) est chargé depuis `Assets/Resources/Audio/ambient_wind.wav` via `Resources.Load`, ce qui rend l'asset robuste aux conflits de wiring de scène (fix appliqué après un incident de merge où le serialized field perdait la référence). Il joue uniquement dans la scène `Game` avec un fondu de 1,5 s en entrée et en sortie.

**Assets** : 25 SFX OGG en CC0 1.0 (packs Kenney), 6 musiques en CC-BY 3.0 (freesound.org), 1 ambient en CC-BY 3.0 (Szegvari, freesound.org). Crédits intégrés dans `Assets/Audio/CREDITS.md`.

---

### 6.2 Animations & assets visuels — Marion KAUFFMANN

La fonctionnalité couvre les state machines d'animation des agents, le composant de feedback visuel sur dégâts, le système de personnalisation d'avatar, et l'intégration des prefabs de particules.

**Animations des agents :** mise en place des paramètres d'`Animator` côté code (`stateAnim` pour ennemi, `isMoving`/`speed`/`place`/`interact` pour joueur, `fire` pour tour) et liaison aux clips via les transitions de l'`Animator Controller`. Le flip horizontal des sprites en fonction de la direction permet aux agents de toujours regarder dans le sens de leur déplacement.

**`HitFlash`** est un composant générique réutilisable. Au moment où un ennemi prend des dégâts, ses `SpriteRenderer` enfants passent en rouge pendant 0,1 s puis reprennent leur couleur d'origine. Le composant cache les couleurs initiales à l'éveil pour pouvoir les restaurer fidèlement.

**Personnalisation d'avatar :**

```
AvatarCustomizationPanel (UI)        AvatarSessionManager (DontDestroyOnLoad)
─────────────────────────────        ───────────────────────────────────────
5 boutons couleur          ──┐        SetPlayerAvatar(playerIndex, type)
3 sliders RGB tint            │       └── PlayerPrefs : avatar_p{N}_color
slider scale (0.6–1.4)        ├──►              avatar_p{N}_tintR/G/B
toggle flip horizontal       ─┘                 avatar_p{N}_scale
bouton Confirm                                  avatar_p{N}_flip
                                       │
                                       ▼ OnSceneLoaded
                              ┌───────────────────┐
                              │ AvatarCustomizer  │
                              │ - swap controller │
                              │ - tint mask       │
                              │ - scale + flip    │
                              └───────────────────┘
```

Cinq variantes de couleur primaire sont fournies sous forme de cinq `RuntimeAnimatorController` distincts (`Resources/AvatarControllers/{Black|Blue|Purple|Red|Yellow}.controller`). À l'application du profil, l'`AvatarCustomizer` swappe l'`Animator.runtimeAnimatorController` du joueur par celui de la couleur choisie. Une teinte secondaire (RGB) est appliquée par-dessus via un `MaterialPropertyBlock`, ce qui n'instancie pas de nouveau matériau et reste léger côté GPU.

La personnalisation est par joueur (P1 et P2 indépendants), persistée via `PlayerPrefs` avec un préfixe `avatar_p{1|2}_*`, et rechargée à chaque chargement de scène.

**Prefabs de particules** : intégration des effets visuels Tiny Swords (`Heal_Effect`, `Magic_Effect`, `dust_1/2`, `explosion_1/2`) sur les évènements de gameplay, plus le prefab `Projectile_Arrow` utilisé par le système de projectile visible.

---

### 6.3 Génération procédurale de l'environnement — Léandre CACARIE

La fonctionnalité génère la décoration de la map (arbres, buissons, rochers) à chaque chargement de la scène `Game`, garantissant un layout différent à chaque partie tout en restant jouable.

**`MapGenerator`** est un singleton lancé en `Awake` qui pose les obstacles en trois passes (une par couche). Pour chaque cellule de la map :

1. Une valeur de bruit de Perlin est calculée à partir de `(x, y, seed)` avec une fréquence configurable (`noiseScale`).
2. Si la valeur dépasse le seuil `density`, un bonus de cluster est ajouté en fonction du nombre de voisins déjà placés (`clusterBonus`), ce qui crée des regroupements naturels (forêts, tas de rochers).
3. Une vérification d'espacement minimum (`minSpacing`) interdit deux obstacles du même type adjacents s'ils ne respectent pas la distance.
4. Un prefab variant est choisi par tirage pondéré dans le `BlockVariantSet` correspondant (chaque variant a un `weight`).
5. Une variation visuelle aléatoire est appliquée : scale ±15 %, flip horizontal optionnel.

**Validation de connectivité** : après les trois passes, un floodfill 8-connexe est lancé depuis chaque `spawnPoint` vers la `baseTransform`. Si un point de spawn ne peut pas atteindre la base (chemin bloqué par les obstacles générés), le `MapGenerator` retire des obstacles bloquants jusqu'à ce que le chemin soit libre. Cela garantit qu'aucun seed ne produit une partie injouable.

**Animation de spawn** : une fois les obstacles instanciés, chacun apparaît avec un `ScaleTo` (0 → 1) en `EaseOutBack` réutilisant la lib `UITween` de l'équipe (collaboration directe avec le pilier 2). Le stagger entre obstacles est `animStagger` (0,01 s par défaut) ce qui donne un effet de « pousse » progressive de l'environnement. En production, `skipAnimOnAwake = true` saute cette animation pour ne pas retarder le démarrage.

**Configuration via ScriptableObjects :**

| Asset | Rôle |
|---|---|
| `DefaultMap.asset` | `MapBlueprint` principal — seed 42, `useRandomSeed = true`, 3 obstacles layers |
| `TreeVariantSet.asset` | Liste pondérée des arbres Tiny Swords |
| `BushVariantSet.asset` | Liste pondérée des buissons |
| `RockVariantSet.asset` | Liste pondérée des rochers |

Chaque `ObstacleLayer` du `DefaultMap` règle indépendamment sa densité, sa fréquence Perlin, son bonus de cluster et son espacement, ce qui permet d'équilibrer la map sans toucher au code.

**Intégration scène** : le GameObject `MapGenerator` est installé dans `Game.unity` avec ses références (`spawnRoot`, `spawnPoints[2]`, `baseTransform`) câblées en Inspector, et son `MapBlueprint` réglé sur `DefaultMap.asset`.

---

## 7. Compatibilité et persistance multi-sessions

Plusieurs systèmes du jeu utilisent `PlayerPrefs` pour la persistance entre sessions :

| Préférence | Clé(s) | Source |
|---|---|---|
| Volumes audio | `vol_master`, `vol_music`, `vol_sfx`, `vol_ambient` | Settings panel (Pilier 6, Antonin) |
| Profil avatar P1 | `avatar_p1_color`, `avatar_p1_tintR/G/B`, `avatar_p1_scale`, `avatar_p1_flip` | Customization panel (Marion) |
| Profil avatar P2 | `avatar_p2_*` | Customization panel (Marion) |
| Bindings clavier | `keybinding_<Action>` | TP2 — KeyBindingManager |
| Bindings manette | `gamepad_<Action>` | TP2 — KeyBindingManager |
| Sensibilité / dead zone | `gamepad_sensitivity`, `gamepad_deadzone` | TP2 — KeyBindingManager |

Toutes les valeurs sont rechargées au démarrage et appliquées avant le premier rendu, ce qui garantit qu'un joueur retrouve exactement sa configuration précédente.

---

## 8. Crédits assets

### Musiques et ambient — CC-BY 3.0 (freesound.org)

| Fichier | Source | Auteur |
|---|---|---|
| `music_menu.wav` | *Music from the Middle Ages — Douce Dame Jolie* | Kyster |
| `music_prep.ogg` | *Afternoon Stroll* | HenryOfSkalitz |
| `music_defense_light.wav` | *Anime Fight Music Loop 1* | Sirkoto51 |
| `music_defense_intense.wav` | *Boss Battle Loop 3* | Sirkoto51 |
| `music_victory.wav` | *Victory Fanfare 8-bit Thunder 4* | Silverillusionist |
| `music_defeat.ogg` | *Time Draft Hiss and Crackle* | Sonically Sound |
| `ambient_wind.wav` | *Winter Fantasy Atmosphere Ambient* | Szegvari |

### Effets sonores — CC0 1.0 (packs Kenney)

- **Kenney Impact Sounds** — https://kenney.nl/assets/impact-sounds
- **Kenney RPG Audio** — https://kenney.nl/assets/rpg-audio
- **Kenney UI Audio** — https://kenney.nl/assets/ui-audio

Détails des renommages dans `Assets/Audio/CREDITS.md`.

### Assets visuels

- **Tiny Swords** (sprites des unités, environnement, particules) — pmaresca, licence libre pour usage non-commercial éducatif.

---

*Document généré pour la remise du TP3 — IFT-2103 H26, Équipe 03*
*Deadline : 30 avril 2026*
