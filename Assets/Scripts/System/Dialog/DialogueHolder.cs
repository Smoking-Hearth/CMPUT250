using TMPro;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct DialogSegment
{
    public string title;
    public AudioClip scrollSound;
    public AudioClip startSound;
    public TMP_FontAsset font; 
    public Sprite image;

    [Header("Background")]
    public Sprite background;
    public Color backgroundMultiplier;
    public bool isTiled;


    [TextArea(0, 5)]
    public string[] lines;

    public bool autoContinue;
    public UnityEvent DoEvent;
}

public class DialogueHolder : MonoBehaviour
{
    [SerializeField] protected DialogSystem dialogSystem;
    [SerializeField] protected DialogSegment[] segments;

    public void PlayDialogue()
    {
        if (dialogSystem != null)
        {
            dialogSystem.Play(new GameDialog(segments));
        }
        else
        {
            gameObject.MyLevelManager().DialogSystem.Play(new GameDialog(segments));
        }
    }
}
