using UnityEngine;

public enum GroundState
{
    None, Grass, Concrete, Wood
}

public class Player
{
    private Rigidbody2D player;
    public GroundState GroundState { get; set; }
    public PlayerSounds Sounds { get; private set; }
    public Vector2 Position
    {
        get
        {
            return player.position;
        }
    }
    public Vector2 Velocity
    {
        get
        {
            return player.linearVelocity;
        }
    }
    public PlayerHealth Health { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public Player(Rigidbody2D playerObject)
    {
        player = playerObject;
        Sounds = playerObject.GetComponent<PlayerSounds>();
        Health = playerObject.GetComponent<PlayerHealth>();
        Movement = playerObject.GetComponent<PlayerMovement>();
    }
}
