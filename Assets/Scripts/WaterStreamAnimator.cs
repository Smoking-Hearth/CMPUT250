using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;
using UnityEngine.InputSystem;

public class WaterStreamAnimator : MonoBehaviour
{
    [SerializeField] private int segments;
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private float streamLength;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float streamDelay;
    [SerializeField] private ParticleSystem nozzleParticles;
    private float targetAngle;
    private UnityEngine.U2D.Spline spline;
    private float activateNext;
    private bool activating;
    private float currentLength;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLength = streamLength;
        activating = false;
        spline = spriteShape.spline;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (activating && currentLength < streamLength && Time.fixedTime >= activateNext)
        {
            spriteShape.spriteShapeRenderer.enabled = true;

            activateNext = Time.fixedTime + 0.02f;
            currentLength += 2;

            if (currentLength > streamLength)
            {
                currentLength = streamLength;
            }
        }
        else if (!activating && currentLength > 2 && Time.fixedTime >= activateNext)
        {
            activateNext = Time.fixedTime + 0.02f;
            currentLength -= 2;

            if (currentLength <= 2)
            {
                currentLength = 2;
                spriteShape.spriteShapeRenderer.enabled = false;
            }
        }
    }

    public void SetSpecialActive(bool set)
    {
        activating = set;

        if (set)
        {
            nozzleParticles.Play();
        }
        else
        {
            nozzleParticles.Stop();
        }
    }

    public void ResetStream(float aimAngle)
    {
        Vector2 targetDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;

        for (int i = 1; i < segments; i++)
        {
            float segmentDistance = streamLength / segments;
            Vector2 newPosition = Vector2.right * i * segmentDistance;
            spline.SetPosition(i, newPosition);
        }
    }

    public void Aim(float aimAngle)
    {
        Vector2 targetDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;
        targetAngle = Mathf.LerpAngle(targetAngle, Mathf.Rad2Deg * Mathf.Atan2(targetDirection.y, targetDirection.x), turnSpeed);
        Vector2 delayedDirection = Quaternion.Euler(0, 0, targetAngle) * Vector2.right;

        nozzleParticles.transform.rotation = Quaternion.Euler(0, 0, aimAngle);

        for (int i = segments - 1; i > 0; i--)
        {
            float segmentDistance = currentLength / segments;
            Vector2 segmentPosition = spline.GetPosition(i);
            Vector2 newPosition = Vector2.Lerp(delayedDirection * i * segmentDistance, segmentPosition, Mathf.Log(1 + i * streamDelay));

            if (currentLength != streamLength)
            {
                newPosition = delayedDirection * i * segmentDistance;
            }

            float distance = Vector2.Distance(segmentPosition, newPosition);
            float height = 0.4f + distance * 2 + i * 0.4f;

            spline.SetHeight(i, height);

            spline.SetPosition(i, newPosition);
        }

        for (int i = 0; i < segments; i++)
        {
            //spline.SetPosition(i, new Vector2(i * 2, i * Mathf.Cos(Time.fixedTime * 2 + 0.5f* i)));
            Vector2 enter = delayedDirection;
            Vector2 exit;

            if (i > 0)
            {
                enter = spline.GetPosition(i) - spline.GetPosition(i - 1);
            }
            if (i < segments - 1)
            {
                exit = spline.GetPosition(i + 1) - spline.GetPosition(i);
            }
            else
            {
                exit = enter;
            }

            Vector2 tangentIn = -enter - exit;
            Vector2 tangentOut = enter + exit;

            spline.SetLeftTangent(i, tangentIn.normalized * 0.75f);
            spline.SetRightTangent(i, tangentOut.normalized * 0.75f);
        }
    }
}
