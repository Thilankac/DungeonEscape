# Dungeon Escape

Beginner-friendly **Unity 6** 2D top-down dungeon escape game (C#, Input System, TextMeshPro).

## Features

- WASD / arrow movement with Rigidbody2D
- Collect the key, avoid the enemy, escape through the exit
- Score, lives, and countdown timer
- Win / Game Over UI with restart
- Optional coins for bonus score
- Pixel art sprites and SFX

## Requirements

- Unity 6 (6000.x)
- 2D URP template
- Input System package
- TextMeshPro

## Getting started

1. Clone this repository.
2. Open the folder in **Unity Hub** (Unity 6).
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play**.

Setup details: `Assets/Scripts/UNITY_SETUP_GUIDE.md`

## Controls

| Input | Action |
|---|---|
| WASD / Arrows | Move |
| Touch key | Unlock exit (+10 score) |
| Touch enemy | Lose a life / Game Over |
| Reach exit with key | Win |

## Scripts

- `PlayerMovement.cs`
- `KeyCollect.cs`
- `EnemyMovement.cs`
- `ExitDoor.cs`
- `GameManager.cs`
- `AudioManager.cs`
- `CoinCollect.cs`
- `SimplePulse.cs`
- `UiAnimator.cs`
