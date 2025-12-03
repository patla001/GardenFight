# [Progress Video](https://www.youtube.com/watch?v=eiFQcHKocGE)


# GARDEN FIGHT!
<p align="center">
  <img width="300" height="300" src="Demo/icon.png"><img width="300" height="300" alt="image" src="https://github.com/user-attachments/assets/652e65d9-2f5b-4a42-88bc-ce1f9f0629ab" />

</p>

# Introduction
The game is Garden Fight. This game is built upon the Unity3d game engine, built upon a 3d multiplayer game called Fight. The objective of this game is to fight the boss and win. You are defending your garden from the boss. Protect it at all costs!

# Introduction
- **Group Members**: Ethan Kent, Evan Tardiff, Ezer Patlan, Saqwon Williams

## Updated to Unity 6
This project has been updated from Unity 2019.1.8f1 to **Unity 6000.2.2f1** (Unity 6.2) with modern networking using **Mirror Networking**.

### Requirements
- **Unity Version**: 6000.2.2f1 or later (Unity 6.2 recommended)
- **Mirror Networking**: Version 96.8.5 or later
- **TextMeshPro**: Version 3.0.9 or later

### 📥 Installation
For detailed installation instructions, please see **[INSTALLATION.md](INSTALLATION.md)**

### Quick Start
1. Install Unity 6000.2.2f1 (Unity 6.2)
2. Download [Mirror v96.8.5](https://github.com/MirrorNetworking/Mirror/releases/download/v96.8.5/Mirror-96.8.5.unitypackage)
3. Open the project in Unity
4. Import Mirror package: `Assets → Import Package → Custom Package`
5. Import TextMeshPro essentials: `Window → TextMeshPro → Import TMP Essential Resources`
6. Open `Assets/Scenes/FightScene.unity`
7. Press Play and click "Host" to start

For complete setup instructions, troubleshooting, and build guides, see **[INSTALLATION.md](INSTALLATION.md)**

### What's New in Unity 6 Update
- **Unity Engine**: Updated from 2019.1.8f1 to Unity 6000.2.2f1 (Unity 6.2)
- **Networking**: Migrated from deprecated Unity UNET to Mirror Networking v96.8.5
- **Packages**: Updated all packages to Unity 6 compatible versions
- **Scripts**: Updated all networking scripts to use Mirror namespace and APIs
- **Discovery**: Implemented custom `SimpleNetworkDiscovery` using Mirror's discovery system
- **Transport**: Using KCP Transport for reliable networking
- **Prefabs**: Updated player prefabs with `NetworkIdentity` components
- **API Updates**: Replaced deprecated Unity APIs (e.g., `FindObjectOfType` → `FindFirstObjectByType`)
- **Build Support**: Tested and working on Windows, macOS, and Android platforms
- **Garden Arena**: Added beautiful garden-themed fighting arena with custom materials, animated water effects, and optimized collision system

# Platforms: Android, Windows
The currently supported platforms for this game are both Android and Windows. Cross play between Android and Windows is also available.

# Game Overview
Fight is a 3d, third person, multiplayer, one vs one fighting game. Garden Fight is a sub game of Fight, adding elements from Fight and adding a dark souls style boss that you can fight. In game, you must protect your garden by fighting the boss AI without dying!

# Game Description
Previously, Fight focused on the Multiplayer System so that the player can fight another real player and not AI. Garden Fight focuses on battling a boss AI similarly to dark souls. You, the player, are defending their garden from the attacking creatues. Blue has invaded your garden, and threatens to wreak irreparable damage if left alive. Your job is to defeat him and bring peace to your garden. The boss has certain mechanics that you must work through in order to live and defeat the boss. The boss has several unique and challening mechanics such as projectiles you must dodge, rings you must dodge and find safe spots for, and physical attacks when in close proximity to the boss. Garden Fight builds upon Fight's core mechanics and changes the focus from other players to the threat attacking your garden.

- **Multiplayer**: We use Mirror Networking (formerly UNet) to maintain the multiplayer functionality. Mirror is the community-supported continuation of Unity's deprecated UNET system, providing reliable and feature-rich networking for multiplayer games.
- **Arena**:  
<img src="Demo/arena.png" width="400"> <img src="Demo/2_player.png" width="400"> 
- **Garden Arena**: The game now features a beautiful garden-themed arena with:
  - Lush green grass ground
  - Stone pathways for strategic movement
  - Colorful flower beds as decorative obstacles
  - Animated water pond with shimmer effects
  - All surfaces are fully walkable and jumpable
  - Optimized for combat with open space and visual appeal
-	**Character**:  
![](Demo/player.png)
-	**Animations**: The character is fully animated for all kinds of situation. Animations make a game feels more alive and dynamic.

# Game Features
The game features a variety of things. Here are some of the most noticeable features-
-	**Hotspot multiplayer**: After connecting two devices via Wi-Fi hotspot, players can fight each other in real time **(DEPRECATED)**.
-	**Health bar**: The player and boss has its own health, after the health reaches to 0, the player loses.
-	**Ratio**: The number of wins and loses so far are shown on the home screen. The ratio between wins and loses is shown during the fight under the Health Bar **(DEPRECATED)**.
-	**Movement**: Player can move around freely in any direction. But the looking direction is restricted due to multiplayer complexity. During a fight player will always look at the opponent.
-	**Jump**: Player can jump in any direction.
-	**Shield**: Player can protect themselves with a wooden shield.
-	**Weapons**: Player can decide whether use fist or sword to attack the opponent.
-	**Magic spell**: A powerful magic spell attack fireball can be casted which deals heavy damage to the opponent. But there is a certain amount of time is need for the player to cast the magic spell again.
-	**Sounds**: There are all kinds of sounds present in the game. These sounds make the game more natural. Sounds can be switched on or off.
-	**Particle effects**: Particle effects make a game more stunning and appealing to the player. Blood splash, explosion and some other particle effects are present in this game.
-	**Screen shake**: The screen shakes while being attacked by the opponent. It makes the game punchier and alive.
-	**Cross play**: Android user and windows user can also fight each other by connecting via Wi-Fi hotspot **(DEPRECATED)**.
-	**NEW: Boss Fight**: Boss fight, player can fight a dark souls style boss.

# User Manual (TAKEN FROM FIGHT)
## Getting Started
For two players to fight each other, both users must be in the same Wi-Fi network. If there is no Wi-Fi in the area, then any one of the users must turn his device’s hotspot on and let the other user connect to that hotspot. It is required to have a stable connection between the two devices. Now the users can both open the game in their corresponding devices.  
After the game opens, in the home screen there will be two buttons. One is HOST and the other is JOIN.  
Now one user will have to click the HOST button. After clicking the HOST button, the character will appear in an arena.  
<img src="Demo/home.png" width="400"> <img src="Demo/ui.png" width="400">  
Now the other user has to click the JOIN button. Then a new button CLICK TO ENTER will appear. CLICK TO ENTER button only appears if the other user has already clicked HOST button and entered into the arena.  
<img src="Demo/waiting.png" width="200"> <img src="Demo/join.png" width="245"> <img src="Demo/2_player_ui.png" width="400">  
After clicking CLICK TO ENTER the other player will also join the arena.
Now both of the players are in the arena and can fight each other.
User can also click the CANCEL button and go back to the home screen.
## Control: 
-	**Android**: The joystick on the left side on screen is used for moving the character up-down, left-right. And the icons on the right side perform the corresponding actions shown by the icons. Shield icon to bring in/out the shield, switch icon to switch the weapon, sword, or fist icon to attack the opponent, jump icon for jumping and magic icon to shoot fireball.
-	**Windows**: WASD is used for moving the character. F for shooting magic fireball, up-arrow for weapon switching, left arrow to bring in/out shield, down arrow for jumping and right arrow is for attacking.

---

## 📚 Documentation

### Installation & Setup
- **[INSTALLATION.md](INSTALLATION.md)** - Complete installation guide with step-by-step instructions
  - System requirements
  - Dependency installation
  - Project setup
  - Building for different platforms
  - Troubleshooting guide

### Project Information
- **Unity Version**: 6000.2.2f1 (Unity 6.2)
- **Networking**: Mirror v96.8.5
- **Main Scene**: `Assets/Scenes/FightScene.unity`
- **Player Prefab**: `Assets/Prefabs/RPG-Character Variant.prefab`
- **Arena Materials**: `Assets/Materials/Garden_*.mat` (6 custom garden-themed materials)
- **Arena Scripts**: `Assets/Scripts/GardenWaterShimmer.cs` (water animation effects)

---

## 🛠️ Developer Tools

The project includes several helpful Unity Editor tools:

- **Tools → Setup NetworkManager in Scene** - Automatically configures NetworkManager
- **Tools → Fix Player Prefabs Now** - Fixes prefabs and adds NetworkIdentity
- **Tools → Cleanup Missing Scripts in Scene** - Removes broken script references
- **Tools → Quick Fix - Add Transport to NetworkManager** - Adds transport component

---

## 🤝 Contributing

Contributions are welcome! If you find bugs or have suggestions:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## 📄 License

This project uses:
- **Unity Engine** - Personal/Plus/Pro License
- **Mirror Networking** - MIT License
- **RPG Character Animation Pack FREE** - Unity Asset Store License

---

## 🙏 Credits

- **Original UNET Version**: [Previous contributors]
- **Unity 6 Migration**: Updated to Unity 6000.2.2f1 with Mirror Networking
- **Assets**: RPG Character Animation Pack FREE from Unity Asset Store
- **Networking**: Mirror Networking by vis2k and contributors

## Current Target for the Game
Garden Fight is a game built upon Fight, a 3d multiplayer combat arena game, but instead focuses on you, the player, fighting a boss. We are aiming toward a bullet hell dark souls style boss that you can fight user the human controlled player. We are aiming to add interactive mechanics and bullet hell to the boss to make the boss fight more engaging and challenging. As we add more elements to the boss, we can separate the boss into different difficulties with each difficulty having different mechanics, the easier difficulty have less than the hardest. Garden Fight will keep the core attacking and defending mechanics found and used from Fight.

## General Goals for Each Member
* note that these goals are tentative and different roles can be swapped around
  
**Ethan Kent**
- Create a new Boss AI
- Implement different styles of bullet hell
- Separate difficulties from the boss
- Implement boss phases
- Update README and "story"/theme

**Evan Tardiff**
- Manage human controlled player mechanics
- Manage simple boss attack mechanics
- Handle boss animation

**Ezer Patlan**
- Port Fight from Unity 2019.1.8f1 to Unity 6000.2.2f1 (Unity 6.2) with modern networking
- Create a new arena to fit the garden theme
- Habdle new art assets
- Handle animations

**Saqwon Williams**
- Manage and implement health system for player and boss
- Manage damage system for player and boss
- Manage damage animations for player and boss

