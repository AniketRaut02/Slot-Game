# 3-Reel Slot Machine Prototype

A high-quality 3-reel slot machine prototype built in Unity, developed with a strict focus on software architecture — a data-driven, event-based approach designed for decoupled, scalable systems. The game features four standard symbols (Cherry, Bar, Bell, Seven), mathematically weighted reel stripping, dynamic win evaluation, and polished "game feel" mechanics.

**🎮 Live Demo:** [aniketraut02.github.io/Slot-Game](https://aniketraut02.github.io/Slot-Game/)

---

## Instructions to Run

**Play instantly:** [aniketraut02.github.io/Slot-Game](https://aniketraut02.github.io/Slot-Game/) — no download required.

**Running the local WebGL build from this repo:**

1. Extract the `.zip` file containing the WebGL build (or use the `/Build/WebGL` folder directly).
2. Due to standard browser CORS restrictions, WebGL builds generally can't be run by double-clicking `index.html` directly.
3. Start a local web server from the build folder:
   ```bash
   python -m http.server 8000
   # or: python3 -m http.server 8000
   ```
   Then open `http://localhost:8000` in your browser.
4. **Alternative:** Zip the build folder contents and upload as an HTML5 project on a platform like itch.io.

---

## Bonus Features

- **Free Spins (Scatter Mechanics)** — Landing 3 Scatter symbols seamlessly hijacks the game loop, locking the UI and triggering an automated Free Spins bonus round that does not deduct from the player's wallet.
- **Wild Symbols** — Substitute for standard symbols to complete payout lines, fully decoupled from the Scatter logic.
- **"Rigged Spin" Debug Tool** — A custom inspector tool built into the orchestrator that bypasses the RNG, forcing specific reel indices to instantly test edge cases, win evaluations, and bonus triggers without relying on luck.
- **Polished "Juice" & Game Feel** — Custom mathematical Back-Ease-Out reel snapping for physical weight, symbol squash/stretch on impact, a synchronized animated lever, tactile UI buttons, and staggered reel tension timing.

---

## Thought Process & Approach

The primary objective was to adhere strictly to Clean Code principles and modular architecture (composition over inheritance) to produce a maintainable codebase.

- **Data-Driven Architecture** — All core game data (Symbols, Reel Strips with RNG weights, Paytables, and machine configuration) is authored as `ScriptableObjects`, cleanly separating the data layer from the logic layer.
- **Separation of Concerns (SRP)** — The presentation layer (UI/visual reels) is completely blind to the mathematical core. Economy management (`PlayerWallet`, `PayoutCalculator`) and the `WinEvaluator` are written as pure C# classes, fully decoupled from the Unity `GameObject` lifecycle.
- **Event-Driven Communication** — Systems communicate exclusively through `ScriptableObject`-based Event Channels (e.g. `SpinRequestedEvent`, `WinEvaluatedEvent`). This made it possible to add entirely new systems — the Audio Manager and Lever Animator — by simply listening to the broadcast, with zero modifications to the core orchestrator.
- **Robust State Management** — The slot machine flow is governed by a strict state machine (`Idle`, `Spinning`, `Evaluating`, `BonusPlay`), preventing race conditions and logic bugs such as input being processed mid-spin or visual/audio overlap during reel deceleration.
