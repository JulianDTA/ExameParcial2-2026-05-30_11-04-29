# Sistema de Juiciness + Flujo de Juego

## Estructura

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs       # Máquina de estados: Hub → Playing → LevelComplete / Defeat
│   └── LevelController.cs    # Win() / Fail() / GameOver() de ejemplo
├── Juice/
│   ├── JuiceManager.cs       # Camera shake (trauma), hit stop, post-FX URP
│   ├── AudioManager.cs       # SFX con pitch/volumen aleatorio + pool
│   ├── UIJuice.cs            # Punch scale (Bézier), color flash TMP, shake
│   └── ButtonJuice.cs        # New Input System + lerp de color de material
└── UI/
    ├── UIScreen.cs           # Base de pantalla (fade + punch)
    ├── ScreenManager.cs      # Conmuta pantallas según GameState
    ├── HubScreen.cs          # Lobby con selección de niveles desbloqueados
    ├── LevelCompleteScreen.cs# Pantalla de progreso (victoria)
    └── DefeatScreen.cs       # Pantalla de derrota
```

## Flujo de estados

```
        StartLevel()                 CompleteLevel()
  HUB ───────────────► PLAYING ──────────────────► LEVEL_COMPLETE
   ▲                     │                              │
   │                     │ RegisterFailure() (vidas=0)  │ GoToNextLevel()
   │                     ▼                              │
   └──── GoToHub() ──── DEFEAT ◄───────────────────────┘
              ▲           │
              └─ RetryLevel()
```

## Generación automática (recomendado)

En Unity, menú superior:

- **Tools ▸ Juice ▸ Build Demo Scene** → crea y abre `Assets/Scenes/JuiceDemo.unity`
  con managers, EventSystem, Canvas y las 3 pantallas (Hub, Victoria, Derrota) +
  un HUD con botones de prueba `WIN` / `FAIL` ya cableados. Todas las referencias
  `[SerializeField]` quedan conectadas automáticamente.
- **Tools ▸ Juice ▸ Build Base Prefabs** → genera `Assets/Prefabs/JuicyButton.prefab`
  y `Assets/Prefabs/Managers.prefab`.

Tras generar la escena solo queda (manual):
1. **AudioManager** → arrastra `tone*.wav` (id `"tone"`) y `prueba.wav` (id `"prueba"`).
2. **JuiceManager** → asigna el `Volume` de post-proceso (opcional).
3. Pulsa **Play** y prueba el flujo Hub → WIN → pantalla de progreso → FAIL → derrota.

> El builder vive en `Assets/Scripts/Editor/DemoSceneBuilder.cs` (solo editor).
> Las escenas/prefabs se generan así porque escribir el YAML a mano es frágil;
> este método cablea las referencias de forma garantizada.

## Setup manual en el editor

### 1. GameObject `_Managers` (persistente)
- `GameManager` → set `totalLevels = 9`, `startingLives = 3`.
- `JuiceManager` → arrastra la `Main Camera` y el `Volume` de post-proceso.
- `AudioManager` → crea `SoundEntry`:
  - id `"tone"` → arrastra `tone.wav` y `tone (1..10).wav` al array `variants`.
  - id `"prueba"` → arrastra `prueba.wav`.

### 2. Canvas de UI
- `ScreenManager` en el Canvas raíz. Asigna `hubScreen`, `levelCompleteScreen`, `defeatScreen`, `gameplayHud`.
- Cada pantalla = Panel con `CanvasGroup` + el script correspondiente (`HubScreen`, `LevelCompleteScreen`, `DefeatScreen`).
- En `HubScreen.levelButtons` arrastra 9 botones (usan BtnColor1..9.mat).

### 3. Post-processing
En `DefaultVolumeProfile.asset` añade con override habilitado:
`Chromatic Aberration`, `Vignette`, `Lens Distortion`.

### 4. Input
En `ButtonJuice.interactAction` asigna una acción de `InputSystem_Actions.inputactions`.

## Disparar eventos desde tu jugabilidad

```csharp
levelController.Win();         // → pantalla de progreso/victoria
levelController.Fail();        // → resta vida, derrota si llega a 0
levelController.GameOver();    // → derrota inmediata
JuiceManager.Instance.TriggerHeavyImpact(0.7f); // golpe completo de juice
```
