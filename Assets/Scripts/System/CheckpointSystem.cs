using System;
using System.Collections.Generic;
using UnityEngine;

public struct SavedVariableAccess
{
    public Func<object> Get;
    public Action<object> Set;
}

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

    private List<SavedVariableAccess> savedState;
    private List<object> savedData;

    public void RegisterState(SavedVariableAccess savedVariableAccess)
    {
        if (savedState == null)
        {
            savedState = new() { savedVariableAccess };
            savedData = new() { null };
        }
        else
        {
            savedState.Add(savedVariableAccess);
            savedData.Add(null);
        }
    }

    public void SaveState()
    {
        for (int i = 0; i < savedState.Count; ++i)
        {
            savedData[i] = savedState[i].Get();
        }
    }

    public void LoadState()
    {
        for (int i = 0; i < savedState.Count; ++i)
        {
            savedState[i].Set(savedData[i]);
        }
    }

    void FixedUpdate()
    {
        Vector2 playerPos = gameObject.MyLevelManager().Player.Position;
        int old = current;
        for (int i = checkpoints.Count - 1; i > current; --i)
        {
            if (Vector2.Distance(checkpoints[i].position, playerPos) <= triggerDist)
            {
                current = i;
            }
        }
        if (old != current)
        {
            SaveState();
        }
    }
}