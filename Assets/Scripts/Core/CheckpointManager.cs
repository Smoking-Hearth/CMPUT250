using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CheckpointManager
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

    void UpdateCheckpoint(Vector3 playerPos)
    {
        for (int i = checkpoints.Count - 1; i > current; --i)
        {
            if (Vector2.Distance(checkpoints[i].position, playerPos) <= triggerDist)
            {
                current = i;
            }
        }
    }
}