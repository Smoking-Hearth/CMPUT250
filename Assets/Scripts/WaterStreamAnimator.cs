using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;
using UnityEngine.InputSystem;

public class WaterStreamAnimator : MonoBehaviour
{
    [SerializeField] private int segments;
    [SerializeField] private SpriteShapeController spriteShape;
    private UnityEngine.U2D.Spline spline;
    private Vector2 targetVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spline = spriteShape.spline;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        targetVector = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 targetDirection = targetVector - (Vector2)transform.position;

        for (int i = 0; i < segments; i++)
        {
            float segmentDistance = Vector2.Distance(transform.position, targetVector) / segments;
            spline.SetPosition(i, Vector2.Lerp((Vector2)transform.position + targetDirection * i * segmentDistance, spline.GetPosition(i), i / (float)segments));
        }

        for (int i = 0; i < segments; i++)
        {
            //spline.SetPosition(i, new Vector2(i * 2, i * Mathf.Cos(Time.fixedTime * 2 + 0.5f* i)));
            Vector2 enter = targetDirection;
            Vector2 exit = targetDirection;

            if (i > 0)
            {
                enter = spline.GetPosition(i) - spline.GetPosition(i - 1);
            }
            if (i < segments - 1)
            {
                exit = spline.GetPosition(i + 1) - spline.GetPosition(i);
            }

            Vector2 tangentIn = -enter - exit;
            Vector2 tangentOut = enter + exit;
            
            spline.SetLeftTangent(i, tangentIn.normalized * 0.75f);
            spline.SetRightTangent(i, tangentOut.normalized * 0.75f);
        }

    }
}
