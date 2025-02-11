# Architecture
At a high level we have two key classes: `GameManager` and `LevelManager`. These
divide our global and local state.

In each file there is an `LevelManager` `GameObject` (which should be at the first
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

# Manager vs System vs Controller
The difference between these things is the scope they operate on. 

A `Manager` works and should "think" within the scope of the whole game. It should think on the level of scenes. We only
have two `Manager` classes: `GameManager` (scope: across all scenes) and `LevelManager` (scope: scene by scene basis).

A `System` is somewhat a different kind of thing than a manager or controller, it's kindof like the glue between the two. `System`s typically have state that "belongs" the the whole of a scope.
`TimeSystem` has state that "belongs" to the whole scene/level and is responsible for updating it,
whereas `SceneSystem` has state that belongs to the whole game. Thus respectivly `TimeSystem` would
be thrown onto the `LevelManager` `GameObject` in a single scene. And `SceneSystem` would be stuck
on the `GameManager` singleton.

A `Controller` works on the level of an individual things, a general rule of thumb is that it could concievably make sense to have multiple instances of a `Controller` script in the same scene. `PlayerController` controls the player, which though a collection of `GameObject` it's still one distinct thing. `EnemyController` controls a single enemy. etc.


## Fire

## Water

## Enemy

## Player