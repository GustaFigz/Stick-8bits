```markdown
# 1. Project Overview

**Project Type:** 2D Platformer Prototype/Game

**Target Platforms:** Standalone Windows (x86_64)

**Rendering Pipeline:** Universal Render Pipeline (2D)

**Unity Version:** 6000.3.2f1

**Core Features / Pillars:**
- 2D side-scrolling environment utilizing URP 2D lighting.
- Player movement with jumping and attacking mechanics.
- Hand-animated sprite animations (attack, idle, run).
- Input driven entirely by the new Unity Input System.
- Lightweight structure for quick iteration and extensibility.

# 2. Gameplay Flow / User Loop

**Major States:**
- **Initialization:** The project starts at `SampleScene.unity`, initializing the main camera, lighting, and loading the Player prefab.
- **Gameplay:** The player can move, jump, and attack within a 2D environment composed of ground, background, and lighting.

**Core Loop:**
1. Player movement and ground collision detection via `Ground Check`.
2. Triggered animations (`run`, `idle`, `attack`) based on player input.
3. Loop persists until player exits the scene or quits.

**State Transitions:**
- Scene start → Player instantiated → Input actions enabled → Continuous gameplay loop.

# 3. Architecture (Runtime + Editor)

**Runtime Systems:**
- **PlayerMovement.cs:** Handles player input mapping, movement physics, and animation state transitions.
- **Animator Controller (_playerAnim.controller):** Defines animation blend tree and state machine for player sprite.
- **InputActionAssets:** `Player.inputactions` and `InputSystem_Actions.inputactions` configure and map gameplay and UI input.

**Editor Tooling:**
- Uses Unity AI Assistant and Generators packages for potential procedural asset generation.
- Scene Templates under `Settings/Scenes/` streamline the creation of URP-2D scenes.

**Entry Points:**
- Initial scene — `Assets/Scenes/SampleScene.unity`.
- Primary logic within `PlayerMovement` script executed at runtime.

**Patterns & Communication:**
- MonoBehaviour event loop (`Update`, `FixedUpdate`).
- Input actions trigger animation events through Animator parameters.

# 4. Scene Overview & Responsibilities

**Scene List:**
1. `SampleScene.unity`: Main gameplay scene.
   - Contains player, camera, ground, background, and lighting.
2. `URP2DSceneTemplate.unity`: Template baseline scene for URP setups.

**Loading Strategy:**
- Static scene loading (no additive loading implemented yet).

**Responsibilities:**
- **SampleScene:** Controls game runtime; hosts player logic, environment, lighting.
- **URP2DSceneTemplate:** Reference setup for new scene creation with proper URP 2D Renderer.

**Constraints:**
- Only open `SampleScene` for testing gameplay.
- All URP pipeline assets must be assigned in Project Settings or Universal Render Pipeline asset files.

# 5. UI System

**Framework:** UGUI (package `com.unity.ugui: 2.0.0`)

**Navigation:** No dedicated UI scenes present. Input system includes UI action maps for future extensibility.

**Binding Logic:** Input Action bindings for `UI/Click`, `Submit`, `Cancel`, etc., reserved.

**UI Style:** Not yet defined. All visuals currently 2D pixel-based sprites.

# 6. Asset & Data Model

**Asset Style:**
- Pixel-art style.
- Low-resolution, 8-bit aesthetic.
- Color palette minimalistic; stylized 2D lighting via Global Light 2D.

**Data Formats:**
- Sprites (Texture2D imported frames).
- AnimationClips (`.anim`)
- AnimatorController (`.controller`)
- InputActionAssets (`.inputactions` JSON-based binding data)
- Rendering Pipeline assets (`.asset`)

**Asset Organization:**
- Player-related assets all under `Assets/Player/` for isolation.
- Scene templates and renderer configuration under `Assets/Settings/`.
- Shared environmental sprites in root directory (`ground.png`, `back.png`).

**Naming & Versioning Rules:**
- Consistent frame naming: `frame_XX_delay-0.XXs` for animation frame textures.
- Scene and project templates use PascalCase naming.

# 7. Project Structure (Repo & Folder Taxonomy)

**Folder Layout:**
```
Assets/
 ├── Player/ (Character-related assets: animations, scripts, inputs)
 ├── Scenes/ (Gameplay scenes)
 ├── Settings/ (URP and Scene Template assets)
 ├── back.png, ground.png (Environment textures)
 ├── UniversalRenderPipelineGlobalSettings.asset (Global URP asset)
```

**Conventions:**
- Each gameplay asset folder (e.g., Player) contains visuals, code, and Input bindings.
- Animation frames stored per action (idle, attack, run_right) maintaining identical frame structure.

# 8. Technical Dependencies

**Unity & Pipeline:**
- Unity 6000.3.2f1
- Universal Render Pipeline (URP 17.3.0)

**Core SDKs / Packages:**
- Input System (`com.unity.inputsystem` 1.17.0)
- 2D Animation (`com.unity.2d.animation` 13.0.2)
- 2D SpriteShape and Tilemap integration.
- AI Assistant & AI Toolkit for in-editor assistance.

**Third-Party / Experimental Packages:**
- com.unity.ai.assistant (AI-driven guidance)
- com.unity.ai.generators (Procedural content generation)

# 9. Build & Deployment

**Build Steps:**
1. Open `SampleScene.unity`.
2. Ensure URP asset linked under Graphics Settings.
3. Select *File → Build Settings → Build*.

**Supported Platforms:**
- Windows 64-bit (Standalone)

**CI/CD:**
- No configured automation yet.

**Environment Requirements:**
- Requires .NET Standard 2.0 environment.
- Compatible with UniversalRP 2D Renderer.

# 10. Style, Quality & Testing

**Code Style:**
- Unity C# convention, PascalCase for class names, camelCase for fields.

**Performance Guidelines:**
- Minimize per-frame sprite swapping.
- Optimize texture resolution to preserve pixel fidelity.
- URP 2D renderer keeps scene lighting efficient.

**Testing Strategy:**
- Manual playtesting via SampleScene.
- Input events validation through Input Debugger panel.

# 11. Notes, Caveats & Gotchas

**Known Issues:**
- Limited scene transitions.
- No UI or menus implemented yet.
- Possible overlap between Player.inputactions and InputSystem_Actions.inputactions.

**Dependency Rules:**
- If modifying the Player animation controller, ensure associated clips maintain correct naming and Animator parameter bindings.

**Deprecated Systems:** None.

**Platform Caveats:**
- URP 2D lighting requires correct Renderer2D asset assignment per scene.
```