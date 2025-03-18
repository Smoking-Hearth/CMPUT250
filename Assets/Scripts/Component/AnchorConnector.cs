using UnityEngine;

public class AnchorConnector : MonoBehaviour
{
    [SerializeField] private Transform anchorPoint;
    [SerializeField] private SpriteRenderer connectorGraphics;
    [SerializeField] private float tension;
    [SerializeField] private Rigidbody2D myRigidbody;
    private Combustible anchorFire;

    private void OnEnable()
    {
        anchorPoint.TryGetComponent(out anchorFire);

        if (myRigidbody == null)
        {
            TryGetComponent(out myRigidbody);
        }
    }

    void FixedUpdate()
    {
        if (anchorFire != null && !anchorFire.Burning)
        {
            gameObject.SetActive(false);
            return;
        }
        if (anchorPoint == null || anchorPoint.gameObject.activeSelf == false)
        {
            gameObject.SetActive(false);
            return;
        }
        Vector2 direction = (Vector2)anchorPoint.position - myRigidbody.position;
        float angle = Mathf.Atan2(direction.y, direction.x);
        myRigidbody.AddForce(direction.sqrMagnitude * tension * direction.normalized);
        connectorGraphics.transform.position = transform.position;
        connectorGraphics.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        connectorGraphics.size = new Vector2(direction.magnitude, 1);
    }
}
