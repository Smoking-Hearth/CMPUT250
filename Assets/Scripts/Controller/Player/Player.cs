using UnityEngine;

public enum GroundState
{
    None, Grounded
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
        if (playerObject == null)
        {
            return;
        }
        player = playerObject;
        Sounds = playerObject.GetComponentInChildren<PlayerSounds>();
        Health = playerObject.GetComponent<PlayerHealth>();
        Movement = playerObject.GetComponent<PlayerMovement>();
    }
}
