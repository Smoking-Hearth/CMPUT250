using System.Collections.Generic;
using UnityEngine;

public class DialogueHolder : MonoBehaviour
{
    [SerializeField] protected string title;
    [TextArea(0, 5)]
    [SerializeField] protected List<string> lines;
    [SerializeField] protected bool autoContinue;

    public void PlayDialogue()
    {
        gameObject.MyLevelManager().DialogSystem.Play(new GameDialog(lines, title), autoContinue);
    }
}
