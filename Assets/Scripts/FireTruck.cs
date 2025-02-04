using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireTruck : MonoBehaviour, IInteractable
{
    public Vector2 Position 
    {
         get { return gameObject.transform.position; }
    }

    public bool Available
    {
        get { return true; }
    }

    private float interactDistance; 
    public float InteractDistance 
    {
        get { return interactDistance; }
    }

    [SerializeField] private GameObject interactText;

    void Awake()
    {
        interactDistance = gameObject.GetComponent<BoxCollider2D>().bounds.extents.x;
    }

    public void StartInteract()
    {
        List<String> lines = new List<string>
        {
            "Am firetruck"
        };
        GameManager.dialogSystem.Play(new GameDialog(lines, "Firetruck"));
    }

    public void StopInteract() {}

    public void Target() 
    {
        interactText.SetActive(true);
    }

    public void Untarget() 
    {
        interactText.SetActive(false);
    }

}
