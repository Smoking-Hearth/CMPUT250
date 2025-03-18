using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// CREDIT/HELP: https://gamedevbeginner.com/how-to-move-an-object-with-the-mouse-in-unity-in-2d/#dynamic
// public class CreditTranslation : MonoBehaviour
// {
public class CreditTranslation : MonoBehaviour
{
    public GameObject selectedObject;
    Vector3 offset;

    void Update()
    {
        // Get mouse position in world
        Vector3 mousePointer = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // If player holds down mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Find object that mouse is over
            Collider2D targetObject = Physics2D.OverlapPoint(mousePointer);
            
            // If it exists, get object
            if (targetObject)
            {
                selectedObject = targetObject.transform.gameObject;
                offset = selectedObject.transform.position - mousePointer;
            }
        }
        // If the object exists, move it to where the mouse pointer is
        if (selectedObject)
        {
            selectedObject.transform.position = mousePointer + offset;
            
        }
        // If the player releases the mouse, deselect it
        if (Input.GetMouseButtonUp(0) && selectedObject)
        {
            selectedObject = null;
        }
    }
}
