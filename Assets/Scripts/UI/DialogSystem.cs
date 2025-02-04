using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class DialogLine 
{

} 

public interface IGameDialog {
    String Title();
    String NextLine();
}


public class DialogSystem : MonoBehaviour
{

    public enum State
    {
        Inactive,
        DisplayingLine,
        WaitingForContinue,
    }

    private State dialogSystemState = State.Inactive;
    public State DialogSystemState 
    {
        get { return dialogSystemState; }
    }

    private IGameDialog currentDialog;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;

    String currentLine;
    int currentPosition = 0;

    void Awake()
    {
        dialogSystemState = State.Inactive;
    }

    void Update()
    {
        if (dialogSystemState == State.Inactive) return;

        if (dialogSystemState == State.DisplayingLine)
        {
            if (currentPosition < currentLine.Length)
            {
                contentText.text.Append(currentLine[currentPosition]);
                ++currentPosition;
            }
            else
            {
                dialogSystemState = State.WaitingForContinue;
            }
        }

        // FIXME: Update this to use Input System
        if (dialogSystemState == State.WaitingForContinue && Input.anyKeyDown)
        {
            currentLine = currentDialog.NextLine();
            if (currentLine == "")
            {
                dialogSystemState = State.Inactive;
                gameObject.SetActive(false);
            }
            else
            {
                dialogSystemState = State.DisplayingLine;
                currentPosition = 0;
            }
        }  
    }

    public bool Play(IGameDialog gameDialog)
    {
        if (dialogSystemState == State.Inactive)
        {
            dialogSystemState = State.DisplayingLine;

            currentDialog = gameDialog;

            titleText.text = currentDialog.Title();
            contentText.text = "";
            currentLine = currentDialog.NextLine();

            gameObject.SetActive(true);
            return true;
        }
        return false;
    }
}
