using UnityEngine;

public class HelicopterDescend : MonoBehaviour
{
    private Vector2 startPosition;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float duration;
    private static float descendTimer;
    private bool boarded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        startPosition = transform.position;
    }

    public static void Descend()
    {
        descendTimer = 10;
    }

    public void Enter()
    {
        gameObject.MyLevelManager().Player.Movement.SetAttached(transform);
        gameObject.MyLevelManager().Player.Movement.transform.position = attachPoint.position;
        boarded = true;
        descendTimer = 10;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (descendTimer > 0)
        {
            descendTimer -= Time.fixedDeltaTime;

            if (boarded)
            {
                transform.position = Vector2.Lerp(startPosition, endPoint.position, descendTimer / duration);
            }
            else
            {
                transform.position = Vector2.Lerp(endPoint.position, startPosition, descendTimer / duration);
            }
        }
    }
}
