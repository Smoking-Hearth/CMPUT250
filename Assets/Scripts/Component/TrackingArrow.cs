using UnityEngine;
using UnityEngine.UI;

public class TrackingArrow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Image arrowImage;
    [SerializeField] private Image icon;
    public Transform Target { get { return target; } }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 targetScreenPoint = Camera.main.WorldToScreenPoint(target.transform.position);
        Bounds screenBounds = new Bounds(Camera.main.WorldToScreenPoint(Camera.main.transform.position), new Vector2(Screen.width, Screen.height));

        if (icon.gameObject.activeSelf)
        {
            if (screenBounds.Contains(targetScreenPoint))
            {
                arrowImage.gameObject.SetActive(false);
                icon.gameObject.SetActive(false);
                return;
            }
        }
        else if (!screenBounds.Contains(targetScreenPoint))
        {
            arrowImage.gameObject.SetActive(true);
            icon.gameObject.SetActive(true);
        }

        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(gameObject.MyLevelManager().Player.Position);
        Vector2 direction = playerScreenPoint - targetScreenPoint;
        float angle = Mathf.Atan2(direction.y, direction.x);
        Ray ray = new Ray(targetScreenPoint, direction);
        float intersectDistance;
        screenBounds.IntersectRay(ray, out intersectDistance);

        icon.rectTransform.position = targetScreenPoint + direction.normalized * (intersectDistance + 170);
        arrowImage.rectTransform.position = targetScreenPoint + direction.normalized * (intersectDistance + 100);
        arrowImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + 90);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    public void SetTarget(Transform newTarget, Sprite iconSprite)
    {
        target = newTarget;
        icon.sprite = iconSprite;
    }
}
