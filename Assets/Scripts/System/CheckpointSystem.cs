using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CheckpointSystem: MonoBehaviour
{
    private int current = -1;
    public int Current 
    {
        get { return current; }
    }
    
    [Tooltip("This should be in order")]
    [SerializeField] private List<Transform> checkpoints;
    public List<Transform> Checkpoints
    {
        get { return checkpoints; }
    }

    [SerializeField] private float triggerDist;

    void FixedUpdate()
    {
        Vector2 playerPos = gameObject.MyLevelManager().Player.Position;
        for (int i = checkpoints.Count - 1; i > current; --i)
        {
            if (Vector2.Distance(checkpoints[i].position, playerPos) <= triggerDist)
            {
                current = i;
                // Debug.Log("New checkpoint appointed: "+ checkpoints[current].position);
                // Debug.Log("Current: " + current);
            }
        }
    }

    public void ReturnToCurrent(Rigidbody2D player)
    {
        if (current < 0)
        {
            return;
        }
        player.position = checkpoints[current].position;
    }
}