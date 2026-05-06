# GOALS.md

# FPV Drone Racing Game — Unity Project Spec

## Project Objective

Build a polished Unity-based 3D drone racing game inspired by Geometry Dash pacing, but with FPV drone-style movement and ring-based checkpoint progression.

The player controls a drone that flies forward automatically at a constant speed through a linear course. The player only controls vertical and horizontal movement. The goal is to fly through rings/checkpoints in order. Missing a required ring, crashing, or leaving the playable course resets the level.

The game should feel fast, smooth, skill-based, and replayable. The most important priority is that flying the drone feels genuinely fun.

This should be a complete playable Unity project, not a small prototype.

---

## High-Level Game Concept

The game combines:

- Geometry Dash-style fixed-speed level progression
- FPV drone racing movement feel
- Ring/checkpoint-based course navigation
- Multiple handcrafted levels
- Clean neon futuristic visuals
- Smooth controls and polished feedback

The player should not control speed. The course always advances at a constant forward pace. Challenge comes from positioning, precision, anticipation, and flow.


## Autonomous Execution Rules

- Do not pause for confirmation
- Make reasonable design decisions independently
- Continuously iterate and refine gameplay feel
- Fix bugs as they appear
- Prefer shipping over asking questions
- Run Unity batch validation after major changes
- Continue improving visuals/game feel after core systems work


---

## Core Gameplay Loop

1. Player starts a level.
2. Drone automatically flies forward at a constant speed.
3. Player moves the drone up/down/left/right to line up with rings.
4. The next required ring is highlighted.
5. Player must pass through rings in order.
6. Missing the required ring resets the level.
7. Crashing into obstacles resets the level.
8. Passing through all rings completes the level.
9. Completing a level unlocks the next level.

---

## Controls

The player only controls lateral movement.

### Keyboard Controls

- W / Up Arrow: move up
- S / Down Arrow: move down
- A / Left Arrow: move left
- D / Right Arrow: move right
- R: restart current level
- Esc: pause or return to menu

There should be no throttle control, no braking, and no manual forward movement. Forward motion is automatic and constant.

---

## Drone Movement Requirements

Movement is one of the most important parts of the project.

The drone must not feel like a cube sliding on rails.

### Required Feel

- Smooth acceleration
- Smooth damping when input is released
- Momentum/inertia
- Responsive but not twitchy
- Natural banking and tilting
- Clear sense of speed
- No excessive camera shake

### Movement Model

The drone should:

- Move forward automatically at constant velocity
- Use input to control local X/Y movement
- Smoothly accelerate toward target lateral velocity
- Smoothly decelerate when input is released
- Stay within playable course boundaries
- Reset if it exits the course boundary

### Visual Rotation

The drone model should tilt based on movement:

- Moving left rolls drone left
- Moving right rolls drone right
- Moving up pitches slightly upward
- Moving down pitches slightly downward
- Rotation should be visual-only and not destabilize movement

---

## Camera Requirements

Use a smooth chase camera or near-FPV camera.

The camera should:

- Follow behind the drone smoothly
- Slightly lag behind movement for physical feel
- Remain readable and playable
- Avoid aggressive shake
- Use subtle FOV changes for speed feedback
- Never make the game nauseating or visually chaotic

The camera should make the player feel like they are racing through the course, but clarity matters more than intensity.

---

## Ring / Checkpoint System

Each level contains a sequence of rings.

### Ring Rules

- Rings must be flown through in order
- Only the current required ring counts
- The current ring should be clearly highlighted
- Future rings can be visible but visually subdued
- Completed rings should deactivate, fade, or change color
- Missing the current ring should reset the level once the drone passes beyond it
- Passing through all rings completes the level

### Ring Visual States

Implement clear states:

1. Future ring
   - visible but not dominant
   - neutral color
   - low glow

2. Current ring
   - distinct highlight color
   - stronger glow
   - optional arrow/marker
   - unmistakably the next target

3. Completed ring
   - faded out
   - changed color
   - disabled collider
   - optional particle effect

---

## Failure Conditions

The level should reset when:

- The player misses the required ring
- The player crashes into an obstacle
- The player exits the playable boundary
- The player passes beyond the current ring without entering it

The reset should be quick and not frustrating.

Suggested failure feedback:

- Short “Missed Ring” or “Crashed” message
- Quick fade or flash
- Immediate restart after short delay
- R key should restart instantly

---

## Level Completion

A level is complete when all required rings are passed in order.

On completion, show:

- “Level Complete”
- Next Level button
- Replay Level button
- Back to Level Select button

Do not use timer/best-time as the primary mechanic because each course has a fixed forward speed and therefore a mostly fixed completion time.

Progression should be based on completing levels, not racing the clock.

---

## Levels

Include at least 5 playable levels.

Each level should be handcrafted or generated from clear level configuration data.

### Level 1 — Basic Flow

Purpose: teach basic control.

- Straight course
- Large rings
- Gentle vertical and horizontal movement
- No obstacles
- Clear spacing

### Level 2 — Horizontal/Vertical Patterns

Purpose: teach directional control.

- Rings alternate left/right
- Rings move up/down in simple waves
- Slightly tighter spacing
- Still no major obstacles

### Level 3 — Rhythm Patterns

Purpose: create Geometry Dash-style flow.

- Smooth arcs
- Alternating ring sequences
- More demanding positioning
- Faster-feeling visual layout
- May introduce decorative tunnel elements

### Level 4 — Obstacles

Purpose: introduce hazard avoidance.

- Rings placed near obstacles
- Simple walls or pillars
- Clear openings
- No unfair blind obstacles
- Current ring must remain readable

### Level 5 — Precision Course

Purpose: final challenge.

- Smaller rings
- Tighter path
- More vertical/horizontal variation
- Obstacles used carefully
- Requires smooth control and anticipation

---

## Level Design Principles

Levels should feel rhythmic and readable.

Use patterns such as:

- Smooth waves
- Rising arcs
- Falling arcs
- Alternating left/right paths
- Center-to-edge transitions
- Edge-to-center recovery
- Tight tunnel sections
- Wide recovery sections after difficult sequences

Avoid:

- Random ring placement
- Blind turns
- Cluttered visuals
- Impossible sequences
- Overly chaotic camera or effects

The player should usually be able to see the next ring before reaching it.

---

## Progression System

Implement a simple progression system.

- Level 1 unlocked by default
- Completing a level unlocks the next level
- Completed levels are marked in level select
- Locked levels are visible but disabled
- Progress is saved locally using PlayerPrefs

---

## UI Requirements

### Main Menu

Include:

- Play
- Level Select
- Quit

### Level Select

Include:

- Level buttons for Levels 1–5
- Locked level visual state
- Completed level visual state
- Back button

### In-Game HUD

Show:

- Current level name/number
- Ring progress, e.g. 4 / 18
- Restart hint: “R to Restart”
- Optional pause hint: “Esc to Pause”

Do not show timer or best time as the primary game objective.

### Pause Menu

Include:

- Resume
- Restart Level
- Level Select
- Main Menu

### Level Complete Screen

Include:

- Level Complete title
- Next Level
- Replay
- Level Select
- Main Menu

---

## Visual Style

The game should have a clean futuristic neon drone-racing aesthetic.

### Style Requirements

- Dark or simple background
- High-contrast glowing rings
- Minimal visual clutter
- Futuristic course elements
- Clear track readability
- Sleek drone model
- Polished but not cartoonish

### Ring Visuals

Rings should:

- Be visible from a distance
- Have emission/glow
- Clearly communicate current/future/completed state
- Use particle burst on successful pass

### Course Visuals

Use:

- Tunnels
- Floating track markers
- Neon guide rails
- Minimal obstacle geometry
- Subtle background elements
- Clean skybox or dark void environment

Avoid:

- messy textures
- cluttered scenery
- childish/cartoonish effects
- excessive bloom
- excessive screen shake

---

## Drone Visual Design

If no asset is available, create a drone using primitive shapes.

The drone should look like a small futuristic FPV racing drone.

Suggested structure:

- central body
- four arms
- four small rotors
- glowing engine lights
- subtle trail particles

The drone should not be a plain cube.

---

## Audio Requirements

Add simple but polished audio if possible.

Suggested sounds:

- Drone engine hum
- Ring pass sound
- Crash/fail sound
- Level complete sound
- UI click sounds

Audio should enhance the game without becoming annoying.

---

## Game Feel / Polish

Prioritize game feel.

Add:

- subtle engine glow
- subtle drone trail
- ring pass particles
- smooth camera follow
- clean UI transitions
- small FOV response to movement/speed
- responsive restart
- readable failure feedback

Avoid:

- aggressive camera shake
- excessive motion blur
- overdone speed effects
- overly bright clutter
- physics instability

---

## Technical Requirements

Use Unity 3D and C#.

The project should be modular and easy to extend.

### Required Scripts

Create scripts similar to:

- DroneController.cs
- CameraFollow.cs
- RingCheckpoint.cs
- RingManager.cs
- LevelManager.cs
- GameManager.cs
- UIManager.cs
- LevelSelectManager.cs
- BoundaryReset.cs
- ObstacleReset.cs

### Suggested Architecture

Use clear separation:

- DroneController handles movement/input
- CameraFollow handles camera behavior
- RingCheckpoint handles individual ring triggers
- RingManager tracks ring order/progress
- LevelManager loads/resets levels
- GameManager tracks game state
- UIManager updates menus/HUD
- LevelSelectManager handles unlocks/completion

### Prefabs

Create prefabs for:

- Drone
- Ring
- Obstacle
- Level container
- Particle burst
- UI panels if useful

### Data

Use either:

- ScriptableObjects for level definitions
- serialized arrays of ring transforms
- prefab-based level scenes

The game should be easy to expand with new levels.

---

## Collision and Detection

### Ring Detection

Use trigger colliders on rings.

When drone enters the current ring trigger:

- mark ring completed
- increment ring progress
- activate next ring
- play pass effect
- update HUD

If drone enters a future ring before it is current, ignore it or treat it as invalid depending on what feels best. Prefer ignoring future rings unless it creates confusion.

### Miss Detection

A ring is missed if:

- the drone’s forward position passes beyond the current ring by a threshold
- and the ring was not completed

Then reset the level.

### Obstacle Detection

Colliding with obstacle geometry resets the level.

### Boundary Detection

Leaving the playable volume resets the level.

---

## Constant Speed Requirement

Do not make speed the main variable.

The drone should move forward automatically at a fixed speed per level.

Optional:

- Later levels may use slightly higher fixed speed
- Speed can vary by level, but not by player input
- No timer/best-time focus

The challenge should come from course design and precision, not speedrunning.

---

## Local Save System

Use PlayerPrefs to save:

- highest unlocked level
- completed levels
- optional settings

Do not require online accounts, backend, or database.

---

## README Requirements

Include a README.md with:

- Project overview
- Unity version used
- How to open the project
- How to play
- Controls
- How to add levels
- File/script architecture summary

---

## Definition of Done

The project is done when:

- Unity project opens without errors
- Main menu works
- Level select works
- At least 5 levels exist
- Level progression/unlocking works
- Player controls drone with up/down/left/right only
- Drone moves forward automatically at constant speed
- Movement feels smooth and fun
- Drone visually tilts/banks while moving
- Camera follows smoothly
- Rings must be passed in order
- Current ring is clearly highlighted
- Completed rings visually deactivate
- Missing a ring resets the level
- Crashing resets the level
- Completing all rings completes the level
- UI screens are functional
- Progress saves locally
- README exists
- Code is modular and understandable

---

## Final Quality Bar

This should feel like a polished playable vertical slice, not a graybox prototype.

The final result should be fun enough that someone can immediately understand the game, play multiple levels, and feel the difference between sloppy movement and clean flying.

The project should demonstrate:

- Unity gameplay programming
- Movement/controller design
- Level progression
- Collision systems
- UI systems
- Game feel and polish
- Clean project architecture