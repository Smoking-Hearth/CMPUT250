using UnityEngine;

public class HelicopterDescend : MonoBehaviour
{
    private Vector2 startPosition;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float duration;
    private float descendTimer;
    private bool descend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        startPosition = transform.position;
    }

    public void Descend()
    {
        descend = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!descend)
        {
            return;
        }

        if (descendTimer > 0)
        {
            descendTimer -= Time.fixedDeltaTime;
            transform.position = Vector2.Lerp(endPoint.position, startPosition, descendTimer / duration);
        }

    }
}
