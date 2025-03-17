using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

//CREDIT/HELP: https://gamedevbeginner.com/how-to-move-an-object-with-the-mouse-in-unity-in-2d/#dynamic
// public class CreditTranslation : MonoBehaviour
// {
public class ClickAndDrag : MonoBehaviour
{
    public GameObject selectedObject;
    Vector3 offset;

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);

            if (targetObject)
            {
                selectedObject = targetObject.transform.gameObject;
                offset = selectedObject.transform.position - mousePosition;
            }
        }

        if (selectedObject) // X == 11, Y == 5
        {
            selectedObject.transform.position = mousePosition + offset;
              
            
        }
        // if (selectedObject.transform.position.x >= 18f || selectedObject.transform.position.x <= 1f){

        //     selectedObject = null;
        //     Debug.Log("out of bounds: x");

        // }
        // if (selectedObject.transform.position.y >= 17f || selectedObject.transform.position.y <= 6f){

        //     selectedObject = null;
        //     Debug.Log("out of bounds: y");

        //}  
        if (Input.GetMouseButtonUp(0) && selectedObject)
        {
            selectedObject = null;
        }
    }
}
//     public Rigidbody2D selectedObject;
//     Vector2 offset;
//     Vector2 mousePointer;

//     public float maxSpeed=10;
//     Vector2 mouseForce;
//     Vector2 lastPosition;
    
//     void Update()
//     {
//         Vector2 mousePointer = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//         Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

//         if (selectedObject){

//             mouseForce = (mousePosition - lastPosition) / Time.deltaTime;
//             mouseForce = Vector2.ClampMagnitude(mouseForce, maxSpeed);
//             lastPosition = Input.mousePosition;
//         }

//         if (Input.GetMouseButtonDown(0)){

//             Collider2D targetObject = Physics2D.OverlapPoint(Input.mousePosition);

//             if (targetObject){

//                 selectedObject = targetObject.transform.gameObject.GetComponent<Rigidbody2D>();
//                 offset = selectedObject.transform.position - Input.mousePosition;

//             }
//         }
//         if (Input.GetMouseButtonDown(0)){

//             selectedObject.linearVelocity = Vector2.zero;
//             selectedObject.AddForce(mouseForce, ForceMode2D.Impulse);
//             selectedObject = null;



//         }

//     }

//     void FixedUpdate()
//     {   

//         Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
//         if (selectedObject){

//             selectedObject.MovePosition(mousePosition + offset);

//         }
//     }


// }
