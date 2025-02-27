using UnityEngine;
using System.Collections.Generic;

public class TrackerSystem : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TrackingArrow arrowPrefab;
    private List<TrackingArrow> arrows = new List<TrackingArrow>();

    public void AddTracker(Transform target)
    {
        for (int i = 0; i < arrows.Count; i++)
        {
            if (!arrows[i].gameObject.activeSelf)
            {
                arrows[i].SetTarget(target);
                arrows[i].gameObject.SetActive(true);
                return;
            }
        }

        TrackingArrow arrow = Instantiate(arrowPrefab, canvas.transform);
        arrow.SetTarget(target);
        arrows.Add(arrow);
    }

    public void RemoveTracker(Transform target)
    {
        for (int i = 0; i < arrows.Count; i++)
        {
            if (arrows[i].Target == target)
            {
                arrows[i].SetTarget(null);
                arrows[i].gameObject.SetActive(false);
            }
        }
    }
}
