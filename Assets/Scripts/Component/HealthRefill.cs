using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthRefill : MonoBehaviour
{
    [SerializeField] private int HealthPerPickup; // HealthPerPickup 
    private bool interacting;
    public delegate void OnHealed(int amount); // OnHealed
    public static event OnHealed onHealed; // OnHealed, onHealed

    public LevelManager levelManager;

    public void StartPickUp()
    {
        interacting = true;
    }
    public void HealAndDestroy()
    {
        // If not interacting, get out of function
        if (!interacting)
        {
            return;
        }

        // If player interacts with item, give desired effect
        float playerMax = levelManager.Player.Health.Max;

        if (levelManager.Player.Health.Current >= playerMax - HealthPerPickup)
        {

        levelManager.Player.Health.Current = playerMax;

        }
        else{

        levelManager.Player.Health.Current += HealthPerPickup;
        
        }
        interacting = false;
        Destroy(gameObject);

    }
}

