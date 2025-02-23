using UnityEngine;

[System.Serializable]
public struct DialogSegment
{
    public string title;

    [TextArea(0, 5)]
    public string[] lines;

    public bool autoContinue;
}

public class DialogueHolder : MonoBehaviour
{
    [SerializeField] protected DialogSegment[] lines;

    public void PlayDialogue()
    {
        // gameObject.MyLevelManager().DialogSystem.Play(new GameDialog(lines, title), autoContinue);
    }
}
