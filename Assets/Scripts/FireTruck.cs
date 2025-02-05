using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireTruck : MonoBehaviour
{
    public void PlayDialogue()
    {
        List<string> lines = new List<string>
        {
            "Am firetruck"
        };
        GameManager.dialogSystem.Play(new GameDialog(lines, "Firetruck"));
    }
}
