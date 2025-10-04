# ServerPrioritizer

A BepInEx plugin for **ATLYSS** that automatically moves your favorite server to the top of the in-game server browser.

--- 

## ⚠️ Dependency 

This mod **requires [EasySettings](https://thunderstore.io/c/atlyss/p/Nessie/EasySettings/)** to function. 
Make sure you have it installed, or the plugin will not load.

---


## 📦 Features

- Prioritizes a specific server in the lobby list based on its name  
- Makes it easier to find and join your preferred server  
- Removes Steam’s 60-result limit on lobby queries

---

## 📥 Installation

1. Download the compiled `ServerPrioritizer.dll`.  
2. Place it in your `BepInEx/plugins` folder.  
3. Launch the game once to generate a config file.  
4. Edit the generated config at: `BepInEx/config/com.fleetime.serverprioritizer.cfg`

---

## ⚙️ How It Works

- Scans the server list for a lobby matching your configured ServerName.
- If found, it reorders the list so your server appears first.
- Patches out Steam’s result count filter (AddRequestLobbyListResultCountFilter(60)) to show more than 60 lobbies.

---

## 📋 Config Options

| Key        | Default       | Description                                 |
| ---------- | ------------- | ------------------------------------------- |
| ServerName | `Eu 24/7 SFW` | The exact name of the server to prioritize. |

---

## 🆘 Troubleshooting

- Server not moving to the top? Double-check that the server name in ServerPrioritizer.cfg matches exactly with how it appears in-game.
    - It’s case-sensitive (`Eu 24/7 SFW` ≠ `eu 24/7 sfw`).
    - Check for trailing spaces or special characters.
- Make sure you’ve launched the game once after installing the plugin to generate the config file.
- Watch the BepInEx console/log for messages like: `Moved 'Eu 24/7 SFW' to the top of the server list.` If you don’t see this, the name isn’t matching.