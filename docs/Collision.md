# Something to Stand on
We're using layers to save on some physics computation and because it makes the code nicer cause you don't need to think about all the other colliders a collsion could be happening with and filter them. 

Ground consists of a `GameObject` with one or more 2D colliders and a `Ground` component. If the `GameObject` is supposed to be a platform assign
it to the `Platform` layer, otherwise it should be in the `Ground` layer.

Additionally if the platform has a slope to it assign it to the `Stairs` layer. All this does is stop the player from sliding down the slope. 

![Player Movement Screenshot](img/PlayerMovement.png)

The various collision layers for ground have a kind of inheritance setup between them for behaviour:
- Ground: Allow player to walk on
- Platform is Ground: Allow player to drop through
- Stairs is Platform: Turn off gravity when player is in contact so they don't slide down the slope.

```C#
❯ : rg WalkableType
Assets/Scripts/Component/Ground.cs
3:public enum WalkableType
10:    [SerializeField] private WalkableType walkableType;
14:    public WalkableType WalkableType { get { return walkableType; } }
```

You'll notice that the `Ground` component has a field called `WalkableType` which seems to specify
similar information to the layer. This field is never used and the only reason I suspect its there because it could be information used later in the future. We can get away without setting it, but I guess best practice would be to set it just in case.