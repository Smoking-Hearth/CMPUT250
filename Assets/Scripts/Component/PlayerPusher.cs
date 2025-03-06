using UnityEngine;

public class PlayerPusher : MonoBehaviour
{
    [SerializeField] private Bounds bounds;
    [SerializeField] private Vector2 force;

    private void Start()
    {
        bounds.center += transform.position;
    }

    private void FixedUpdate()
    {
        Player player = gameObject.MyLevelManager().Player;
        if (bounds.Contains(player.Position))
        {
            player.Movement.PushPlayer(force, (force.y > 0 ? true : false));
        }
    }
}
