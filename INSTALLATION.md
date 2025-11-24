# Installation Guide - FIGHT! 3D Multiplayer Fighting Game

This guide will help you set up and run the FIGHT! game on your machine.

---

## 📋 System Requirements

### Minimum Requirements:
- **Operating System**: Windows 10/11 or macOS 10.15+
- **RAM**: 8 GB
- **Graphics**: DirectX 11 or Metal capable GPU
- **Storage**: 5 GB available space
- **Network**: Internet connection for multiplayer

### Recommended:
- **RAM**: 16 GB
- **Graphics**: Dedicated GPU with 2GB+ VRAM
- **Network**: Broadband internet or LAN connection

---

## 🛠️ Dependencies & Requirements

### 1. Unity Engine
- **Version**: Unity 6000.2.2f1 (Unity 6.2)
- **Download**: [Unity Hub Download](https://unity.com/download)

### 2. Mirror Networking
- **Version**: v96.8.5 or later
- **Download**: [Mirror v96.8.5 Release](https://github.com/MirrorNetworking/Mirror/releases/download/v96.8.5/Mirror-96.8.5.unitypackage)
- **Note**: Mirror is a networking library for Unity that replaced the deprecated UNET system

### 3. Unity Packages (Auto-installed)
These packages are automatically installed when you open the project:
- **TextMeshPro**: v3.0.9 or later (for UI text)
- **Visual Studio Editor**: Latest version
- **UI Toolkit**: Latest version

---

## 📥 Installation Steps

### Step 1: Install Unity Hub & Unity Editor

1. **Download Unity Hub**:
   - Visit: https://unity.com/download
   - Download and install Unity Hub for your platform

2. **Install Unity 6000.2.2f1**:
   - Open Unity Hub
   - Go to **Installs** tab
   - Click **Install Editor**
   - Select **Unity 6000.2.2f1** (or Unity 6.2.x)
   - Choose the following modules during installation:
     - ✅ Android Build Support (if targeting Android)
     - ✅ Windows Build Support (if on Mac targeting Windows)
     - ✅ Mac Build Support (if on Windows targeting Mac)

---

### Step 2: Clone or Download the Project

#### Option A: Clone from Git
```bash
git clone https://github.com/patla001/GardenFight.git
cd GardenFight
```

#### Option B: Download ZIP
1. Download the project ZIP file
2. Extract it to your desired location
3. Note the project path for the next step

---

### Step 3: Download Mirror Networking

**IMPORTANT**: You must manually download Mirror before opening the project.

1. **Download Mirror v96.8.5**:
   - Visit: https://github.com/MirrorNetworking/Mirror/releases
   - Download: `Mirror-96.8.5.unitypackage` (or latest version)
   - Save it somewhere accessible (Desktop, Downloads, etc.)

2. **DO NOT install yet** - you'll import it after opening the project

---

### Step 4: Open the Project in Unity

1. **Open Unity Hub**
2. Click **Open** or **Add** button
3. Navigate to the project folder
4. Select the `Fighting-Game-3d-Multiplayer-Unity3d` folder
5. Click **Open**

**⏳ First-time setup will take 5-10 minutes:**
- Unity will import all assets
- Packages will be downloaded
- Scripts will be compiled
- **Wait for this process to complete!**

---

### Step 5: Import Mirror Networking

1. **Wait for Unity to finish initial setup** (check bottom-right corner)
2. In Unity, go to: **Assets → Import Package → Custom Package...**
3. Navigate to where you downloaded `Mirror-96.8.5.unitypackage`
4. Select it and click **Open**
5. In the import dialog, click **Import** (import all items)
6. Wait for import to complete (1-2 minutes)

---

### Step 6: Import TextMeshPro Essentials

1. In Unity menu: **Window → TextMeshPro → Import TMP Essential Resources**
2. Click **Import** in the popup dialog
3. Wait for import to complete

---

### Step 7: Setup the Scene

1. **Open the main scene**:
   - In Project window: `Assets/Scenes/FightScene.unity`
   - Double-click to open it

2. **Setup NetworkManager** (if not already present):
   - Go to: **Tools → Setup NetworkManager in Scene**
   - A popup will confirm "Setup Complete"
   - If you don't see this menu, the NetworkManager is already configured

3. **Cleanup any issues** (if needed):
   - Go to: **Tools → Cleanup Missing Scripts in Scene**
   - Go to: **Tools → Fix Player Prefabs Now**

4. **Save the scene**:
   - Press `Ctrl+S` (Windows) or `Cmd+S` (Mac)
   - Or go to: **File → Save**

---

### Step 8: Verify Setup

1. **Check Console for errors**:
   - Open: **Window → General → Console**
   - Yellow warnings are OK, red errors are not
   - Clear old messages by clicking the **Clear** button

2. **Test Play Mode**:
   - Click the **Play** button (▶️) at the top
   - You should see the game UI with Host/Join buttons
   - Click **Stop** to exit Play Mode

---

## 🎮 Running the Game

### Testing in Unity Editor

1. **Open FightScene.unity**
2. Click **Play** button
3. Click **"Host"** to start a local server
4. Your character should spawn in the arena
5. Use controls to test gameplay

### Building the Game

1. **Open Build Settings**:
   - Go to: **File → Build Settings**

2. **Add Scene to Build**:
   - Click **Add Open Scenes** (if FightScene isn't listed)
   - Ensure `FightScene` is checked

3. **Select Platform**:
   - Choose **Windows**, **Mac**, or **Android**
   - Click **Switch Platform** if needed (one-time process)

4. **Configure Player Settings** (optional):
   - Click **Player Settings**
   - Set **Company Name**, **Product Name**, **Version**
   - Set **Icon** and **Splash Screen** if desired

5. **Build the Game**:
   - Click **Build** or **Build And Run**
   - Choose a location to save the build
   - Wait for build to complete (2-5 minutes)

---

## 🌐 Multiplayer Setup

### Local Network (LAN) Multiplayer

1. **Build the game** (see above)
2. **Run 2 instances**:
   - Method 1: Run the built game + Unity Editor
   - Method 2: Run 2 copies of the built game
3. **On first instance**: Click **"Host"**
4. **On second instance**: Click **"Join"**
5. The client should automatically discover and connect to the host

### Internet Multiplayer (Advanced)

For internet play, you'll need:
- Port forwarding on router (default port: 7777)
- Or use a relay service like Unity Relay or Photon
- Or deploy on a cloud server

**Note**: LAN discovery only works on local network. For internet play, clients need the host's public IP address.

---

## 🎯 Controls

### PC Controls:
- **WASD** / **Arrow Keys**: Move character
- **Spacebar**: Jump
- **Mouse**: Camera control (if enabled)
- **UI Buttons**: Attack, Guard, Special Moves

### Mobile Controls (if building for Android):
- **On-screen joystick**: Movement
- **UI Buttons**: All actions

---

## 🐛 Troubleshooting

### Issue: "Mirror namespace not found" errors

**Solution**:
- Ensure Mirror v96.8.5 is imported
- Go to: **Assets → Reimport All**
- Wait for Unity to finish reimporting

---

### Issue: Black screen when playing

**Solution**:
1. Open **FightScene.unity**
2. Run: **Tools → Setup NetworkManager in Scene**
3. Save the scene
4. Try playing again

---

### Issue: "NetworkIdentity required" errors

**Solution**:
1. Run: **Tools → Fix Player Prefabs Now**
2. Wait for success message
3. Clear Console and test

---

### Issue: Missing scripts in Inspector

**Solution**:
1. Run: **Tools → Cleanup Missing Scripts in Scene**
2. Save the scene
3. Try again

---

### Issue: Cannot connect to host

**Solution**:
- Ensure both devices are on the same network
- Check firewall settings (allow Unity/game through firewall)
- Verify NetworkManager has KcpTransport component
- Try manually entering host IP address

---

### Issue: Compilation errors on first open

**Solution**:
- Wait for all packages to finish downloading
- Close and reopen Unity
- Go to: **Assets → Reimport All**

---

## 📦 Build Output

### Windows Build:
- **Executable**: `[GameName].exe`
- **Data Folder**: `[GameName]_Data/`
- **Both are required** to run the game

### Mac Build:
- **App Bundle**: `[GameName].app`
- Right-click → Show Package Contents to see internal files

### Android Build:
- **APK File**: `[GameName].apk`
- Install on Android device (enable "Unknown Sources" in settings)

---

## 📱 Android-Specific Setup

### Additional Requirements:
- **Android SDK** (installed with Unity)
- **JDK** (Java Development Kit)
- **Android Device**: Android 5.0 (API 21) or higher

### Build Steps:
1. **Switch to Android Platform**:
   - File → Build Settings → Android → Switch Platform
2. **Configure Player Settings**:
   - Set Package Name (e.g., `com.yourcompany.fight`)
   - Set Minimum API Level: Android 5.0 (API 21)
   - Set Target API Level: Automatic (Highest Installed)
3. **Build APK**:
   - Click **Build** or **Build And Run**
4. **Install on Device**:
   - Transfer APK to device
   - Enable "Install from Unknown Sources"
   - Install the APK

---

## 🔧 Advanced Configuration

### Changing Network Port

1. Open **FightScene.unity**
2. Select **NetworkManager** in Hierarchy
3. Find **Kcp Transport** component
4. Change **Port** value (default: 7777)
5. Save the scene

### Adjusting Player Settings

1. Select player prefab: `Assets/Prefabs/RPG-Character Variant.prefab`
2. Adjust health, damage, speed values in Inspector
3. Save the prefab

---

## 📚 Additional Resources

- **Mirror Documentation**: https://mirror-networking.gitbook.io/
- **Unity Manual**: https://docs.unity3d.com/Manual/
- **Project Repository**: [Your GitHub URL]
- **Issue Tracker**: [Your Issues URL]

---

## 📄 License

This project uses:
- **Unity Engine** (Unity Personal/Plus/Pro License)
- **Mirror Networking** (MIT License)
- **RPG Character Animation Pack FREE** (Unity Asset Store License)

---

## ✅ Installation Checklist

Use this checklist to ensure proper installation:

- [ ] Unity Hub installed
- [ ] Unity 6000.2.2f1 (Unity 6.2) installed
- [ ] Project downloaded/cloned
- [ ] Mirror v96.8.5 downloaded
- [ ] Project opened in Unity
- [ ] Mirror package imported
- [ ] TextMeshPro essentials imported
- [ ] FightScene.unity opened
- [ ] NetworkManager setup complete
- [ ] No red errors in Console
- [ ] Test play mode successful
- [ ] Build completed (optional)
- [ ] Multiplayer tested (optional)

---

## 🎉 Ready to Play!

If you've completed all steps above, you're ready to fight! 

**Start the game, click Host, and enjoy!**

For support or questions, please open an issue on the GitHub repository.

**Have fun fighting! 🥊**

