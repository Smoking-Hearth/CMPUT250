using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueHolder : MonoBehaviour
{
    [SerializeField] private string title;
    [TextArea(0, 5)]
    [SerializeField] private List<string> lines;

    public void PlayDialogue()
    {
        GameManager.dialogSystem.Play(new GameDialog(lines, title));
    }
}
