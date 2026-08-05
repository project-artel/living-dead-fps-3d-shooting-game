<div align="center">


<a href="https://aayush21.itch.io/living-dead">

  <img src="https://capsule-render.vercel.app/api?type=transparent&fontColor=8B0000&text=🧟‍♂️%20LIVING%20DEAD&fontSize=60&height=100&animation=twinkle&fontAlignY=50" alt="LIVING DEAD Text"/>
  
</a>

**A 3D First-Person Shooter Game Built with Unity**

<br/>

[🎮 **Play / Download for Windows on itch.io**](https://aayush21.itch.io/living-dead) • [🎬 **Watch Gameplay Video**](https://www.youtube.com/watch?v=Xocj5toyDDg)

<br/>

### 📈 Community Reach
*(as of August 2026)*

<table align="center">
  <tr>
    <td align="center" width="220">
      <br/>
      <h1>👁️ 1.8k+</h1>
      <b>itch.io Views</b>
      <br/><br/>
    </td>
    <td align="center" width="220">
      <br/>
      <h1>📥 390+</h1>
      <b>itch.io Downloads</b>
      <br/><br/>
    </td>
  </tr>
</table>

</div>

---

## 📌 Overview

**LIVING DEAD** is an action-packed 3D First-Person Shooter (FPS) featuring both **Normal** and **Zombie Survival** game modes. Built on Unity Terrain, the game leverages precision **Raycasting** for hit detection, realistic item pick-up systems, dynamic audio feedback, and intelligent **Unity NavMesh AI** pathfinding.

### Key Highlights
- 🎯 **Raycast Weapon Mechanics:** Accurate bullet trajectories, range dynamics, and critical hit registers (headshots and eyeshots).
- 🧠 **Unity NavMesh AI:** Dynamic pathfinding, waypoint patrolling, line-of-sight target acquisition, chasing, and distance-based attacking.
- 💻 **Platform Support:** Available as a standalone **Windows PC Executable (`.exe`)**. The Unity source project can also be built and exported for other target platforms.

> **Development Specs:** Developed using Unity `2020.3.24f1`.

---

## 🎬 Gameplay

Click the preview below to watch full gameplay action:

<div align="center">

<a href="https://www.youtube.com/watch?v=Xocj5toyDDg">
  <img src="https://github.com/user-attachments/assets/05b6b238-cd27-4d89-9b71-78bd6ccfbe93" width="500" alt="Living Dead Gameplay Video">
  <br/>
  <img src="https://img.shields.io/badge/▶️%20Watch%20on-YouTube-red?style=for-the-badge&logo=youtube" alt="Play Video">
</a>

</div>

---

## ⚙️ Game Logic & Core Mechanics

- ⏱️ **Objective:** Search the terrain and eliminate all hostiles before the countdown timer expires.
- 🧟 **AI Patrol & Chase Behavior:** Hostile units patrol assigned waypoints until the player breaches their field-of-view (FOV), triggering active chase and engagement protocols.
- ⚔️ **Combat Range & Attacks:** Enemies initiate combat once within striking distance. Enemy health gradually regenerates if they are left unattacked after engagement.
- 🎯 **Critical Hit System:** Headshots and eyeshots deliver boosted critical damage to enemies.
- 📦 **Pickup System:** Ammo replenishment boxes and Medikit health pickups are strategically distributed across the map terrain.
- 🔫 **Arsenal & Loadouts:**
  - **Pistol:** Close-quarters combat weapon.
  - **AK-47:** High-rate assault rifle for medium-to-long-range engagements.
  - **Grenades:** Throwable explosive ordinance with area-of-effect (AoE) damage.
  - *Use the Mouse Scroll Wheel or `Q` key to switch active weapons seamlessly.*

---

## 🎨 Game Features

### 🖥️ User Interface & HUD

<div align="center">

**Main Menu UI**
<br/>
<img src="https://user-images.githubusercontent.com/101112022/176936728-722c1085-63b7-458e-b47d-6bc2da399cbf.png" width="500">

<br/><br/>

**In-Game Interface**
<br/>
<img src="https://user-images.githubusercontent.com/101112022/176938001-c9e42eff-36e1-4610-9cbf-07ea78ed54f9.png" width="500">

</div>

- **Player HP Bar:** Bottom right corner
- **Match Countdown Timer:** Top center HUD
- **Weapon & Ammo Display:** Bottom left corner (shows active weapon sprite, current ammo, and grenade inventory)
- **Target Reticle:** Dynamic white aim sight
- **Radar / Mini-Map:** Top right corner showing enemy positions relative to the player
- **Kill Counter:** Track total eliminations directly below the mini-map

---

### 👾 Enemy Models

| Zombie Model | Fly Robot Model | Soldier Model 1 | Soldier Model 2 |
| :---: | :---: | :---: | :---: |
| ![Zombie](https://user-images.githubusercontent.com/101112022/176939444-3f4f9ef7-5698-46fc-9a66-e313af5eb119.png) | ![Fly Robot](https://user-images.githubusercontent.com/101112022/176939463-33b91d21-81c7-4508-87c6-4703f6e7cab8.png) | ![Soldier 1](https://user-images.githubusercontent.com/101112022/176939490-bed2aef1-cfb2-44fc-a0ab-0e14634babd4.png) | ![Soldier 2](https://user-images.githubusercontent.com/101112022/176939475-e49c0115-2abf-472b-af12-224e6edabd94.png) |

---

### 🔫 Gun Models

| Pistol | AK-47 |
| :---: | :---: |
| ![Pistol](https://user-images.githubusercontent.com/101112022/176951039-8a4c2b97-1c1a-4e7b-8f99-8a36e29b213f.png) | ![AK-47](https://user-images.githubusercontent.com/101112022/176951024-fc97b7bf-1872-49f9-a79d-c04779ac171a.png) |

---

### 🎭 Animations & Particle Effects

<details>
<summary><b>Click to expand Animations Details</b></summary>

- **Directional Movement:** Full 4-way walk and run animation blend trees
- **Dual-Layer Body Masking:**
  - **Jump Animation:** Executes lower-body movement without interrupting upper-body aiming/shooting *(using Unity 3D Avatar Body Masking)*
  - **Shoot Animation:** Operates upper-body recoil and firing motion independently from locomotion
- **Death & Headshot Animations:** Unique reactions upon non-lethal vs. headshot eliminations

</details>

#### Visual Particle Effects

| Headshot Impact | Blood Effect |
| :---: | :---: |
| ![Headshot Impact](https://github.com/user-attachments/assets/a4809f71-7275-49f3-a584-393f02c56610) | ![Blood Effect](https://user-images.githubusercontent.com/101112022/176952175-d6c2178f-4a00-40d7-8b0e-636eaeebdc34.png) |

| Player Hurt | Explosions |
| :---: | :---: |
| ![Player Hurt](https://user-images.githubusercontent.com/101112022/176952086-94adef45-e5a8-4535-8caf-961e0aed3fe6.png) | ![Explosions](https://user-images.githubusercontent.com/101112022/176952062-1e7bbc77-7da1-4a58-a2ce-897fe4587133.png) |

| Bullet Effect | Enemy Finish (Red Smoke) |
| :---: | :---: |
| ![Bullet Effect](https://user-images.githubusercontent.com/101112022/176952027-20d5dad9-4c70-4e0d-9e07-ead7d8125c29.png) | ![Enemy Finish](https://user-images.githubusercontent.com/101112022/176953031-61b49467-4b7a-410b-8cb3-edc01624c7de.png) |

---

### 🔊 Audio System
Immersive 3D positional and dynamic audio for every gameplay event: gunshots, grenade explosions, damage/death grunts, enemy attacks, flying robot prop sounds, ambient background tracks, and pause menu sound effects.

---

## 🎮 Controls (Windows PC)

| Action | Control Key |
| :--- | :--- |
| **Move Forward** | `W` |
| **Move Left** | `A` |
| **Move Backward** | `S` |
| **Move Right** | `D` |
| **Jump** | `Space` |
| **Sprint** | `Left Shift` (Hold) |
| **Shoot** | `Left Mouse Button` |
| **Aim (ADS)** | `Right Mouse Button` |
| **Previous Weapon** | `Q` *or* `Mouse Scroll Up` |
| **Next Weapon** | `Q` *or* `Mouse Scroll Down` |
| **Reload Ammo** | `R` (Near Ammo Box) |
| **Recharge Health** | `Caps Lock` (Near Medikit) |
| **Throw Grenade** | `G` |

---

## 📥 Installation & Play

Download and launch the standalone **Windows executable (`.exe`)** directly from [Living Dead on itch.io](https://aayush21.itch.io/living-dead).

---

## <img src="https://github.com/user-attachments/assets/45853b5d-e055-4884-821e-efc9bcf12f6f" width="45" height="45" align="center" alt="🧟" /> What's Next on the Horizon?

*Living Dead* is constantly evolving behind the scenes as a sandbox for cutting-edge game tech—from **advanced FPS mechanics** and **reinforcement learning AI** to **multiplayer systems** and **next-gen engine architecture**. 

Tried the game? Got wild ideas, feedback, or feature concepts you'd love to see explored?
- 💬 **Drop a comment or review on [itch.io](https://aayush21.itch.io/living-dead)**
