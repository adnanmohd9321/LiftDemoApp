# 🚀 Unity 2D Elevator Simulation (Multiple Lifts)

## 📌 Project Overview

This project is a **2D Elevator Simulation built using Unity Engine**.
The system simulates multiple elevators responding to floor requests intelligently. The goal of this project is to demonstrate **Unity UI implementation, programming logic, and structured code design**.

The simulation contains **multiple elevators, multiple floors, and an intelligent system that selects the nearest available elevator**.

---

# 🎮 Gameplay Preview

![Gameplay Demo](gameplay.gif)

*(Upload a GIF or screenshot named `gameplay.gif` to your GitHub repository to display the gameplay here.)*

Example:

* Record gameplay using **OBS / Xbox Game Bar**
* Convert to GIF or upload screenshot
* Place it in the project root

---

# 🎮 Features

* 🏢 **3 Elevators (Lifts)** operating simultaneously
* 🪜 **4 Floors** (Ground, Floor 1, Floor 2, Floor 3)
* 🔘 **Floor Call Buttons** for each floor
* 🚀 **Nearest Elevator Selection System**
* 🔄 **Elevator Request Queue System**
* ↕️ **Smooth Elevator Movement between floors**
* 📟 **Current Floor Display for each elevator**
* 🧠 **Smart elevator response logic**

---

# ⚙️ System Logic

1. User presses a **floor call button**.
2. The system checks **which elevator is closest and available**.
3. The nearest elevator receives the request.
4. The elevator moves **smoothly to the requested floor**.
5. Each elevator maintains **its own request queue**.
6. Other elevators ignore the request to avoid duplication.

---

# 🛠️ Technologies Used

* **Unity Engine 6**
* **C#**
* **Unity UI System**
* **2D Game Development**

---

# 📂 Project Structure

```
Assets
 ├── Scripts
 │    ├── ElevatorController.cs
 │    ├── ElevatorManager.cs
 │    └── FloorButton.cs
 │
 ├── Prefabs
 │    └── Elevator
 │
 ├── Scenes
 │    └── MainScene
 │
 └── UI
      └── Floor Buttons & Displays
```

---

# ▶️ How to Run

1. Clone the repository

```
git clone https://github.com/adnanmohd9321/Unity-project.git
```

2. Open the project in **Unity Hub**
3. Open the **Main Scene**
4. Click **Play ▶️** to run the simulation

---

# 🎯 Assignment Objective

This project demonstrates:

* Unity **UI design**
* **Game logic programming**
* **Handling multiple elevators simultaneously**
* **Clean and structured code**

---

# 👨‍💻 Author

**Adnan Mohd**

GitHub:
https://github.com/adnanmohd9321
