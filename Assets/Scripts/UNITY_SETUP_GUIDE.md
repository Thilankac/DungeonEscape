# Unity Setup Guide — 2D Dungeon Escape

**Status:** `Assets/Scenes/SampleScene.unity` includes Player, walls, key, enemy, exit, bonus coins, Canvas UI, GameManager, and AudioManager. Press **Play** to test.

## Beginner enhancements

The player starts with **three lives** and **60 seconds** to find the key and escape.
Five small orange diamonds are optional coins worth **5 points each**. The key is
still worth 10 points and is required to open the exit; coins are optional.

Touching an enemy removes one life. With lives remaining, the player returns to
the starting position and keeps collected points and the key. A 1.5-second grace
period prevents repeated damage immediately after a hit. Losing all lives or
running out of time ends the game. Restart resets the whole round.

Select **GameManager** in the Inspector to change Starting Lives, Time Limit, or
Hit Protection Seconds. Select a coin to change its Score Value.
Use **Tools → Dungeon Escape → Add Beginner Enhancements** to add coins and labels
to an existing scene while keeping its artwork. The full setup tools also include
these enhancements.

### Student exercises (in order)

1. **Coins: triggers and variables.** Read `CoinCollect.cs`, change a coin to 10
   points, then duplicate a coin and place it elsewhere. Predict the final score
   before collecting everything (default maximum: 35).
2. **Timer: Update and delta time.** Read `GameManager.Update`, set Time Limit to
   90, and change the red warning threshold from 10 seconds to 15 seconds.
3. **Lives: conditions and methods.** Read `DamagePlayer` and `Respawn`, change
   Starting Lives to 5, and explain why only the final hit shows Game Over.

### Beginner feature checks

- Start: Score 0, Lives 3, Time 60s. Each orange coin adds 5 points once.
- Coins do not unlock the door; the key does and adds 10 points.
- Enemy contact: Lives 2, respawn at the start, score and key retained.
- Contact again after the grace period: Lives 1. Final hit: Game Over.
- Set Time Limit to 5: the timer reaches 0, shows "Time's up!", and ends the game.
- Win or lose: movement and countdown stop; Restart restores coins, key, lives,
  time and score. Restore Time Limit to 60 after testing.

Scripts are in `Assets/Scripts/`. To rebuild the scene later: Unity menu **Tools → Dungeon Escape → Setup Scene Now**, or run `AgentScripts/BuildDungeonScene.cs` via Pipeline `script run`.

Your project already includes Unity 6, URP 2D, the Input System package, and `Assets/Settings/InputSystem_Actions.inputactions`.

---

## 1. Create / open the 2D project

1. Open **Unity Hub** → open this project (`DungeonEscape`).
2. Confirm template style is **2D (URP)** (already set for this folder).
3. Open `Assets/Scenes/SampleScene.unity` (or create a new scene: **File → New Scene → Basic 2D (URP)** and save it as `DungeonScene`).
4. **Edit → Project Settings → Player → Active Input Handling** should be **Input System Package** (or **Both**). This project is already set to the new Input System.
5. If Unity asks to enable TextMeshPro essentials, click **Import TMP Essentials**.

---

## 2. Create the Player

1. Hierarchy → right-click → **2D Object → Sprites → Square**.
2. Rename it to **Player**.
3. In the Inspector:
   - **Tag** → **Player** (create the tag if needed: **Add Tag… → + → Player**).
   - **Transform Position**: `(0, 0, 0)`.
   - **Transform Scale**: `(0.8, 0.8, 1)`.
   - **Sprite Renderer → Color**: blue (e.g. R 0.2, G 0.5, B 1).

---

## 3. Add Rigidbody2D and BoxCollider2D (Player)

1. Select **Player**.
2. **Add Component → Rigidbody 2D**:
   - **Body Type**: Dynamic
   - **Gravity Scale**: `0`
   - **Constraints → Freeze Rotation Z**: enabled
   - **Collision Detection**: Continuous (recommended)
3. **Add Component → Box Collider 2D** (leave **Is Trigger** unchecked).
4. **Add Component → Player Input**:
   - **Actions**: drag `Assets/Settings/InputSystem_Actions`
   - **Default Map**: `Player`
   - **Behavior**: **Send Messages**
5. **Add Component → Player Movement** (script).
6. Confirm **Move Speed** is `5`.

---

## 4. Create the boundary walls

Create four grey squares. Each needs a **Box Collider 2D** with **Is Trigger = false** (default).

| Name         | Position (approx) | Scale (approx)   | Color |
|--------------|-------------------|------------------|-------|
| WallTop      | `(0, 5, 0)`       | `(12, 0.5, 1)`   | Grey  |
| WallBottom   | `(0, -5, 0)`      | `(12, 0.5, 1)`   | Grey  |
| WallLeft     | `(-6, 0, 0)`      | `(0.5, 10, 1)`   | Grey  |
| WallRight    | `(6, 0, 0)`       | `(0.5, 10, 1)`   | Grey  |

Steps for each wall:

1. **2D Object → Sprites → Square** → rename.
2. Set Position / Scale as above.
3. **Sprite Renderer → Color**: grey.
4. Ensure **Box Collider 2D** exists and **Is Trigger** is off.
5. Optional: put all walls under an empty parent named **Walls**.

Adjust sizes to fit your camera. Select **Main Camera** and set **Size** (Orthographic) to about `5` if needed.

---

## 5. Create the key

1. **2D Object → Sprites → Square** → rename to **Key**.
2. Position somewhere open, e.g. `(-3, 2, 0)`.
3. Scale: `(0.5, 0.5, 1)`.
4. **Sprite Renderer → Color**: yellow.
5. **Box Collider 2D → Is Trigger**: **enabled**.
6. **Add Component → Key Collect**.
7. Confirm **Score Value** is `10`.

---

## 6. Create the enemy

1. Create empty GameObject named **EnemyPath**.
2. As children of **EnemyPath**, create two empties:
   - **PointA** at e.g. `(2, -2, 0)`
   - **PointB** at e.g. `(2, 2, 0)`
3. **2D Object → Sprites → Square** → rename to **Enemy**.
4. Position at PointA.
5. Scale: `(0.7, 0.7, 1)`.
6. **Sprite Renderer → Color**: red.
7. **Add Component → Rigidbody 2D**:
   - **Body Type**: Kinematic
   - **Gravity Scale**: `0`
   - **Freeze Rotation Z**: enabled
8. **Add Component → Box Collider 2D** (**Is Trigger** off for solid collision).
9. **Add Component → Enemy Movement**.
10. Drag **PointA** and **PointB** into the script fields.
11. Set **Move Speed** (e.g. `2`).

---

## 7. Create the exit door

1. **2D Object → Sprites → Square** → rename to **ExitDoor**.
2. Position e.g. `(4, 3, 0)`.
3. Scale: `(0.8, 1.2, 1)`.
4. **Sprite Renderer → Color**: green.
5. **Box Collider 2D → Is Trigger**: **enabled**.
6. **Add Component → Exit Door**.

---

## 8. Create the Canvas and TextMeshPro UI

1. Hierarchy → right-click → **UI → Canvas**.
2. With Canvas selected, set **UI Scale Mode** to **Scale With Screen Size** (Canvas Scaler).
3. Create UI text (TextMeshPro):
   - Right-click Canvas → **UI → Text - TextMeshPro** → rename **ScoreText**
     - Text: `Score: 0`
     - Anchor: top-left
   - Right-click Canvas → **UI → Text - TextMeshPro** → rename **InformationText**
     - Text: `Find the key and escape!`
     - Anchor: top-center
4. Create panels:
   - Right-click Canvas → **UI → Panel** → rename **WinPanel**
     - Add child TextMeshPro: `YOU WIN!` (large, centered)
     - Disable the panel in the Inspector (uncheck the top-left checkbox)
   - Right-click Canvas → **UI → Panel** → rename **GameOverPanel**
     - Add child TextMeshPro: `GAME OVER!`
     - Disable the panel
5. Right-click Canvas → **UI → Button - TextMeshPro** → rename **RestartButton**
   - Button label: `Restart`
   - Place near the center (visible when game ends)
   - You can leave it enabled; `GameManager` hides it on Start and shows it on win/lose
6. Ensure an **EventSystem** exists (Unity usually creates one with the Canvas).

---

## 9. Attach manager scripts

1. Create empty GameObject named **GameManager**.
2. **Add Component → Game Manager**.
3. Create empty GameObject named **AudioManager**.
4. **Add Component → Audio Source** (if not auto-added).
5. **Add Component → Audio Manager**.

---

## 10. Assign Inspector references

### GameManager

| Field              | Assign                          |
|--------------------|---------------------------------|
| Score Text         | ScoreText                       |
| Information Text   | InformationText                 |
| Win Panel          | WinPanel                        |
| Game Over Panel    | GameOverPanel                   |
| Restart Button     | RestartButton                   |

### Player

| Component     | Setting                                      |
|---------------|----------------------------------------------|
| Player Input  | Actions = InputSystem_Actions, Map = Player, Behavior = Send Messages |
| Player Movement | Move Speed = 5                             |

### Enemy

| Field   | Assign  |
|---------|---------|
| Point A | PointA  |
| Point B | PointB  |

### AudioManager

Assign clips only if you have them (see next section). Leave empty if not — the game still runs.

---

## 11. Add audio clips (optional)

1. Import short `.wav` / `.mp3` files into e.g. `Assets/Audio/`.
2. Select **AudioManager**.
3. Drag clips into:
   - **Key Collect Clip**
   - **Win Clip**
   - **Game Over Clip**
4. On **Audio Source**, leave **Play On Awake** off.

If no clips are assigned, collection / win / lose still work silently.

---

## 12. How to test the game

1. Save the scene (**Ctrl+S**).
2. Press **Play**.
3. Checklist:
   - **WASD / arrows** move the blue player
   - Walls block the player
   - Touching the yellow key: score becomes `10`, key disappears, message updates
   - Touching the green door **without** the key shows `Collect the key first!`
   - Touching the door **with** the key shows the Win panel and pauses
   - Touching the red enemy removes one life; the third hit shows Game Over and pauses
   - **Restart** reloads the scene
4. If movement does nothing:
   - Confirm **Player Input** Actions asset and **Send Messages**
   - Confirm Active Input Handling uses the Input System package
5. If UI buttons do nothing after Game Over:
   - Confirm **EventSystem** exists
   - Confirm RestartButton is assigned on GameManager

---

## Suggested hierarchy

```
Main Camera
Player
Walls
  WallTop
  WallBottom
  WallLeft
  WallRight
Key
EnemyPath
  PointA
  PointB
Enemy
ExitDoor
GameManager
AudioManager
Canvas
  ScoreText
  InformationText
  WinPanel
  GameOverPanel
  RestartButton
EventSystem
```

---

## Script ↔ responsibility quick map

| Script            | Attach to   | Role                                      |
|-------------------|-------------|-------------------------------------------|
| PlayerMovement.cs | Player      | Input + Rigidbody2D movement              |
| KeyCollect.cs     | Key         | Score, unlock key flag, destroy key       |
| EnemyMovement.cs  | Enemy       | Patrol + remove a life on contact          |
| CoinCollect.cs    | Each coin   | Award optional bonus points once           |
| ExitDoor.cs       | ExitDoor    | Win if key collected, else hint message   |
| GameManager.cs    | GameManager | Score, state, UI, pause, restart          |
| AudioManager.cs   | AudioManager| Optional SFX                              |
