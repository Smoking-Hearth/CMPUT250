using UnityEngine;
using UnityEngine.U2D;

public class HighPressureSpecial : SpecialAttack
{
    [SerializeField] private int segments;
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private float streamLength;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float streamDelay;
    [SerializeField] private ParticleSystem nozzleParticles;
    private float targetAngle;
    private Spline spline;
    private float activateNext;
    private bool activating;
    private float currentLength;

    [SerializeField] private float splashRadius;
    [SerializeField] private LayerMask collideLayers;
    [SerializeField] private LayerMask fireLayers;
    private float extinguishRayTimer;

    [SerializeField] private AnimationCurve splashCurve;
    [SerializeField] private ParticleSystem splashParticles;
    [SerializeField] private Transform spriteMask;

    public override int MaintainCost
    {
        get
        {
            return maintainCost;
        }
    }
    public override int InitialCost
    {
        get
        {
            return initialCost;
        }
    }

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
        else if (!activating && spriteShape.spriteShapeRenderer.enabled && Time.fixedTime >= activateNext)
        {
            activateNext = Time.fixedTime + 0.02f;
            if(!Retract())
            {
                spriteShape.spriteShapeRenderer.enabled = false;
            }
        }

        if (extinguishRayTimer > 0)
        {
            extinguishRayTimer -= Time.fixedDeltaTime;
        }
    }

    private bool Retract()
    {
        Vector2 targetDirection = spline.GetPosition(segments - 1);

        if (Vector2.Distance(targetDirection, spline.GetPosition(segments - 2)) < 0.2f)
        {
            return false;
        }

        for (int i = 0; i < segments - 1; i++)
        {
            spline.SetPosition(i, Vector2.Lerp(spline.GetPosition(i), spline.GetPosition(i + 1), 0.4f));
        }
        return true;
    }

    public override void Activate(Vector2 startPosition, bool set, Transform parent)
    {
        if (set)
        {
            nozzleParticles.Play();
            splashParticles.Play();
            transform.parent = parent;
        }
        else
        {
            nozzleParticles.Stop();
            splashParticles.Stop();
            transform.parent = null;
        }

        initialPushTime = initialPushDuration;
        transform.position = startPosition;
        activating = set;
        currentLength = 2;
        spline.SetPosition(0, Vector2.zero);
    }

    public override void ResetAttack(float aimAngle)
    {
        targetAngle = aimAngle;
        for (int i = 1; i < segments; i++)
        {
            float segmentDistance = streamLength / segments;
            Vector2 newPosition = Vector2.right * i * segmentDistance;
            spline.SetPosition(i, newPosition);
        }
    }

    public override void AimAttack(Vector2 startPosition, float aimAngle)
    {
        transform.position = startPosition;
        Vector2 targetDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;
        targetAngle = Mathf.LerpAngle(targetAngle, Mathf.Rad2Deg * Mathf.Atan2(targetDirection.y, targetDirection.x), turnSpeed);
        Vector2 delayedDirection = Quaternion.Euler(0, 0, targetAngle) * Vector2.right;

        nozzleParticles.transform.rotation = Quaternion.Euler(0, 0, aimAngle);

        int finalSegment = segments - 1;

        for (int i = segments - 1; i > 0; i--)
        {
            float segmentDistance = currentLength / segments;
            Vector2 segmentPosition = spline.GetPosition(i);
            Vector2 newPosition = Vector2.Lerp(delayedDirection * i * segmentDistance, segmentPosition, Mathf.Log(1 + i * streamDelay));
            float distance = Vector2.Distance(segmentPosition, newPosition);
            float height = 0.4f + i * 0.4f;

            if (currentLength != streamLength)
            {
                newPosition = delayedDirection * i * segmentDistance;
            }
            else
            {
                height += distance * 2;
            }

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
                RaycastHit2D collisionCheck = Physics2D.Raycast(startPosition + (Vector2)spline.GetPosition(i), exit.normalized, exit.magnitude, collideLayers);

                if (collisionCheck && finalSegment > i)
                {
                    finalSegment = i;
                    float width = Mathf.Lerp(10, spline.GetHeight(i), splashCurve.Evaluate(-Vector2.Dot(collisionCheck.normal, delayedDirection)));
                    MaskStream(collisionCheck.point, -collisionCheck.normal, width);
                }
            }
            else
            {
                exit = enter;

                if (finalSegment == i)
                {
                    MaskStream(startPosition + (Vector2)spline.GetPosition(i), exit.normalized, spline.GetHeight(i));
                }
            }

            Vector2 tangentIn = -enter - exit;
            Vector2 tangentOut = enter + exit;

            spline.SetLeftTangent(i, tangentIn.normalized * 0.75f);
            spline.SetRightTangent(i, tangentOut.normalized * 0.75f);
        }

        PushBack(targetDirection);

        if (extinguishRayTimer <= 0)
        {
            finalSegment += finalSegment == segments - 1 ? 0 : 1;
            CastExtinguishRay(startPosition, (Vector2)spline.GetPosition(finalSegment), spline.GetHeight(finalSegment));
        }
    }

    private void MaskStream(Vector2 position, Vector2 direction, float width)
    {
        float angle = Mathf.Atan2(direction.y, direction.x);

        splashParticles.transform.localScale = new Vector2(1, width * 0.5f);
        splashParticles.transform.position = position;
        splashParticles.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        spriteMask.position = position + direction * streamLength * 0.5f;
        spriteMask.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
    }

    private void CastExtinguishRay(Vector2 startPosition, Vector2 direction, float width)
    {
        Vector2 perpendicular = Vector2.Perpendicular(direction.normalized) * width * 0.25f;
        Vector2 leadingDirection = direction + perpendicular;
        Vector2 trailingDirection = direction - perpendicular;

        RaycastHit2D leadingRay = Physics2D.Raycast(startPosition, leadingDirection, leadingDirection.magnitude, collideLayers);
        RaycastHit2D middleRay = Physics2D.Raycast(startPosition, direction, direction.magnitude, collideLayers);
        RaycastHit2D trailingRay = Physics2D.Raycast(startPosition, trailingDirection, trailingDirection.magnitude, collideLayers);
        Debug.DrawRay(startPosition, leadingDirection, Color.red, 2);
        Debug.DrawRay(startPosition, direction, Color.red, 2);
        Debug.DrawRay(startPosition, trailingDirection, Color.red, 2);

        if (leadingRay)
        {
            OnSplash(leadingRay.point);
            Debug.DrawRay(leadingRay.point, Vector2.up, Color.green, 2);
        }
        if (middleRay)
        {
            OnSplash(middleRay.point);
            Debug.DrawRay(middleRay.point, Vector2.up, Color.green, 2);
        }
        if (trailingRay)
        {
            OnSplash(trailingRay.point);
            Debug.DrawRay(trailingRay.point, Vector2.up, Color.green, 2);
        }
        extinguishRayTimer = streamDelay;
    }

    private void OnSplash(Vector2 splashPosition)
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(splashPosition, splashRadius, fireLayers);
        for (int i = 0; i < hitTargets.Length; i++)
        {
            hitTargets[i].GetComponent<IExtinguishable>().Extinguish(extinguishClass, effectiveness);
        }
    }
}
