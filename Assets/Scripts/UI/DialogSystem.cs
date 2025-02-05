using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameDialog : IEnumerator<String> {
    public String Title;

    public IList<String> lines;
    private int idx = 0;

    public String Current
    {
        get 
        { 
            if (lines != null && lines.Count > 0 && idx < lines.Count)
                return lines[idx];
            else
                return null; 
        }
    }

    object IEnumerator.Current
    {
        get { return Current; }
    }


    public GameDialog(IList<String> inner, String title = null)
    {
        Title = title;
        lines = inner;
    }

    public bool MoveNext() 
    {
        idx += 1;
        return idx < lines.Count;
    }

    public void Reset()
    {
        idx = 0;
    }

    public void Dispose() {}
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

    private GameDialog currentDialog;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;

    int currentPosition = 0;

    void Update()
    {
        if (dialogSystemState == State.Inactive) return;

        if (dialogSystemState == State.DisplayingLine)
        {
            if (currentPosition <= currentDialog.Current.Length)
            {
                contentText.text = currentDialog.Current.Substring(0, currentPosition);
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
            currentPosition = 0;
            if (currentDialog.MoveNext())
            {
                dialogSystemState = State.DisplayingLine;
            }
            else
            {
                dialogSystemState = State.Inactive;
                gameObject.SetActive(false);
            }
        }  
    }

    public bool Play(GameDialog gameDialog)
    {
        if (dialogSystemState == State.Inactive && gameDialog.lines.Count > 0)
        {
            dialogSystemState = State.DisplayingLine;

            currentDialog = gameDialog;

            titleText.text = currentDialog.Title;
            contentText.text = "";

            gameObject.SetActive(true);
            return true;
        }
        return false;
    }
}
