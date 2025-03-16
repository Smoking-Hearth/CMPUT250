using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Path : MonoBehaviour
{
    [SerializeField] private List<Vector2> nodes = new List<Vector2>();
    [SerializeField] private List<PathFollower> followers = new List<PathFollower>();
    private List<int> currentFollowerNode;

    public bool Empty
    {
        get
        {
            return followers.Count == 0;
        }
    }

    private void OnEnable()
    {
        currentFollowerNode = new List<int>();
        for (int i = 0; i < followers.Count; i++)
        {
            currentFollowerNode.Add(followers[i].startNode);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            PathFollower current = followers[i];
            if (currentFollowerNode[i] >= nodes.Count || currentFollowerNode[i] > current.destinationNode)
            {
                current.EndEvent.Invoke();
                followers.RemoveAt(i);
                currentFollowerNode.RemoveAt(i);
                i--;
                continue;
            }
            Vector2 toNextNode = nodes[currentFollowerNode[i]] + (Vector2)transform.position - (Vector2)current.transform.position;
            if (toNextNode.magnitude < 0.5f)
            {
                currentFollowerNode[i]++;
            }
            else
            {
                current.transform.position = (Vector2)current.transform.position + toNextNode.normalized * current.speed * Time.fixedDeltaTime;

                if (current.flipTransform == null)
                {
                    continue;
                }

                if (toNextNode.x < 0)
                {
                    current.flipTransform.localScale = new Vector2(-1, 1);
                }
                else
                {
                    current.flipTransform.localScale = new Vector2(1, 1);
                }
            }
        }

    }

    public Vector2 GetNodePosition(int index)
    {
        return (Vector2)transform.position + nodes[index];
    }

    public void AddNode(Vector2 node)
    {
        nodes.Add(node);
    }

    public void AddFollower(PathFollower follower)
    {
        followers.Add(follower);
        currentFollowerNode.Add(follower.startNode);
        follower.StartEvent.Invoke();
    }
}

[System.Serializable]
public struct PathFollower
{
    public Transform transform;
    public Transform flipTransform;
    public float speed;
    public int startNode;
    public int destinationNode;
    public UnityEvent StartEvent;
    public UnityEvent EndEvent;
}
