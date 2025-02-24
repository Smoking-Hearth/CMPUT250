using UnityEngine;

public class RigidbodyEnabler : MonoBehaviour
{
    private Rigidbody2D myRigidbody;
    private void OnEnable()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetEnable(bool enabled)
    {
        if (enabled)
        {
            myRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            myRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
