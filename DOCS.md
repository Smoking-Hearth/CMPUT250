# Manager vs Controller and Composing behaviour
If we're going to use different words we should have a shared understanding of what the difference is (otherwise we ought to just use the shoter one for being easier to write). Based on the previous way the code was organized I have inferred that the the difference is in scope.

1. `Manager` works and should "think" within the scope of the game. Only two kinds: `LevelManager` (scope: game within a scene) and `GameManager` (scope: game across scenes/whole game)


2. `Controller` should "think" within the scope of a single thing (`PlayerController`, `FiretruckController`, `EnemyController`, `BossController`, `CutsceneJumpOffACliffController`, etc).

As per standard practice when writing code, we want to keep these things as small as possible and reuse as much as possible. Not every level may need the same functionality, and we need to balance making things as generic as possible with not abstracting so much that we lose all the useful properties of the objects we're working with.

Making a controller/manager for something gives alot of control over that feature, and the tradeoff that control comes with is a loss of speed. For the purpose of sharing behaviour between things and customizing them we have `System`s for `GameManager` and `LevelManager`, and `Component`s for `Controller`s and random other objects that need behaviour but may not need specifics.

Again the difference between a `System` and `Component` is the scope in which they are expected to operate. The `LevelManager` may not have a `Health` component but will likely have a `InteractableSystem`. Likewise a collider which should trigger an action won't have a `DialogSystem` (this could be a component if you think about it differently, but just roll with it) but will have a `TriggerArea` component. 

**IMPORTANT**: When should we use a `Component`/`System` rather than a `Controller`/`Manager`? `Controller`/`Manager` will require less code short term since it doesn't need to think about all the ways it will be used in since it will only be used in one way. If you start needing that behaviour in a bunch of places though it's going to be pricy (time-wise) to implement a bunch of different kinds of `Controller`s. `Component`s will be more expensive up front which since there are alot of use cases to consider, but once its done you can just drag and drop spam it onto everything. Decide based on how applicable to feature is to other things in the game.

# Architecture
At a high level we have two key classes: `GameManager` and `LevelManager`. These
divide our global and local state.

In each scene there is an `LevelManager` `GameObject` (which should be at the first
entity in the scene, see `SceneSystem`). It serves as the dump spot for the various
systems that operate on data on the scale of the individual `Scene` but aren't closely
associated with any particular `GameObject` which can include
any number of the following:
- `CheckpointSystem`
- `DialogSystem`
- `TimeSystem`
- `EventSystem` (The Unity Input System package one)
- `MusicSystem`
- etc

The `GameManager` is a `GameObject` situated in `DontDestroyOnLoad` and is a singleton. 
This stores scene independent data and runs scene independent systems.

> Such an object needs setup. That can happen either when the
> `GameManager` is accessed for the first time, or if you add a `GameObject` to the scene and attach a `GameManager` to it. On load
> if the `GameManager` is not yet initialized that object will be moved to 
> `DontDestroyOnLoad` and will become the `GameManager`. In practice this 
> only works best for specifying settings we want when a certain scene is run
> by itself.


## Fire

## Water

## Enemy

## Player

# A Note On Logging
Logs are helpful but slow. DO NOT USE `Debug.Log`. Instead, use one of: `DevLog.Info(...);`, `DevLog.Warn(...);`, or `DevLog.Error(...);`. They automatically get stripped out in the production builds and only get run in the editor. Leave them in to give people information when things go wrong.

This should make a couple troubleshoots faster but will by consequence slow down our editor build. So don't profile in the editor (we should be profiling the web and native anyways since there is other editor stuff that makes the editor profiles less efficient).