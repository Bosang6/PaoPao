# PaoPao - Mobile game for MasterCGD (UniVR)


## Overview

PaoPao is a Bomberman-inspired mobile game developed in Unity using C# as a group project for the Mobile Programming course of the Master in Computer Game Development at the University of Verona.

The game combines strategic bomb placement, enemy avoidance, and arena-based gameplay, offering both single-player matches against AI-controlled enemies and online multiplayer matches.

Sound integration is handled through FMOD Studio.

## Project Goals

The main goal of the project was to develop a complete mobile gaming experience, from menu navigation and match configuration to gameplay and end-game management.

The project initially focused on single-player gameplay, with enemy AI and different playable maps. It was later expanded to support online multiplayer using Photon PUN, introducing network synchronization and multiplayer session management.

## Gameplay

Players compete in Bomberman-style arenas, strategically placing bombs to defeat their opponents while avoiding explosions.

The game includes:

- **Single-player:** matches against AI-controlled enemies.
- **Online Multiplayer:** online matches with other players.
- **Match Settings:** character and map selection before starting a match.
- **Audio Settings:** management of the game's audio preferences.
- **End Game:** victory and defeat screens displaying the match result.

## Multiplayer & Technologies

The project was developed using **Unity 6000.3.6f1 and C#**, with **FMOD Studio** for audio integration and **Photon PUN** for online multiplayer functionality.

The multiplayer implementation includes online lobby and room management, player synchronization, and synchronization of networked gameplay objects, including bombs.

These systems allow players to interact within the same arena while maintaining a consistent multiplayer experience.

## Team
The team consists of:
- Conforti Riccardo (main focus: UI + Sound + Multiplayer)
- D'Ambrosio Lorenzo (main focus: Gameplay + Sound + AI)
- Huang Zhikang (main focus: Design and Animations + Sound)

## Version

### Version 1.0 (06/2026)
- Number of playable character: 2
- Number of enemy character: 3
- Number of map: 2

### Version 2.0 (10/2026)
- Added a new map with new gameplay areas and level content.
- Added new background music to improve the overall game atmosphere.
- Added multiplayer support, allowing players to play together online.

### Version 3.0 (10/2026)
Increase the AI of the bot
- Kamikaze: focuses on reach the nearest player (using BFS), places a bomb next to them, and then runs away
- Explorer: focuses on reaching the nearest destructible tile (using BFS) and placing a bomb next to it. If no destructible tiles are present, it use the Kamikaze strategy
- Pacifist: if the number of surviving players is greater than 2, the focus is exclusively on avoiding the bomb. When it comes down to a 1vs1 duel, it use the Kamikaze strategy

All the AI has as first priority to dodge the bomb and stay alive, then everyone has its own custom strategy

---

## Media Version 1.0

- link: https://youtu.be/7MZSeV5ZUPU?si=mRJhvmgd11EUn49Z

<p align="center">
  <img src="Docs/Images/MatchSetup.jpg" width="30%">
  <img src="Docs/Images/MapSpring.jpg" width="30%">
  <img src="Docs/Images/MapWinter.jpg" width="30%">
</p>

## Media Version 2.0 & 3.0

- link: https://youtu.be/vvPt_-8rNBo?si=mfArcvNCdfYzNGiP
<p align="center">
  <img src="Docs/Images/PaoPao_MultiServerLobby.jpg" width="30%">
  <img src="Docs/Images/PaoPao_MultiRoomLobby.jpg" width="30%">
  <img src="Docs/Images/PaoPao_MultiPostGame.jpg" width="30%">
</p>
