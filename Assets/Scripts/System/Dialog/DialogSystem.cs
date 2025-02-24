using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameDialog : IEnumerator<DialogSystem.Command> {
    public DialogSegment[] segments;
    private int segmentIndex = 0;
    private int lineIndex = 0;

    public DialogSystem.Command Current
    {
        get 
        { 
            // We have segments and the cursor is over one
            if (segments != null && segments.Length > 0 && 0 <= segmentIndex && segmentIndex < segments.Length)
            {
                var lines = segments[segmentIndex].lines;
                // We have lines and the cursor is over one
                if (lines != null && lines.Length > 0 && 0 <= lineIndex && lineIndex < lines.Length)
                {
                    return new DialogSystem.Command(lines[lineIndex], segments[segmentIndex].title, segments[segmentIndex].autoContinue, segments[segmentIndex].scrollSound);
                }
            }
            return new DialogSystem.Command(null); 
        }
    }

    object IEnumerator.Current
    {
        get { return Current; }
    }


    public GameDialog(DialogSegment[] segments)
    {
        this.segments = segments;
    }

    public bool MoveNext() 
    {
        lineIndex += 1;
        if (lineIndex >= segments[segmentIndex].lines.Length)
        {
            segmentIndex += 1;
            lineIndex = 0;
        }

        return segmentIndex < segments.Length && lineIndex < segments[segmentIndex].lines.Length;
    }

    public void Reset()
    {
        segmentIndex = 0;
        lineIndex = 0;
    }

    public void Dispose() {}
}


public class DialogSystem : MonoBehaviour
{
    public struct Command
    {
        public AudioClip scrollSound;
        public string content;
        public string title;
        public bool autoContinue;

        public Command(string content, string title = null, bool autoContinue = false, AudioClip scrollSound = null)
        {
            this.scrollSound = scrollSound;
            this.content = content;
            this.title = title;
            this.autoContinue = autoContinue;
        }
    }

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
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float autoContinueDelaySeconds;
    private float continueTimer;

    int currentPosition = 0;

    private LevelState prevLevelState;

    private void OnEnable()
    {
        PlayerController.Controls.Dialogue.Continue.performed += OnContinue;
        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        PlayerController.Controls.Dialogue.Continue.performed -= OnContinue;
    }

    void FixedUpdate()
    {
        switch (dialogSystemState)
        {
            case State.Inactive:
                break;
            case State.DisplayingLine:
                if (currentPosition <= currentDialog.Current.content.Length)
                {
                    contentText.text = currentDialog.Current.content.Substring(0, currentPosition);

                    if (currentPosition % 3 == 0 && currentDialog.Current.scrollSound != null && currentPosition < currentDialog.Current.content.Length)
                    {
                        float pitchOffset = (currentDialog.Current.content[currentPosition] % 32) / 32f;
                        audioSource.pitch = 0.5f + (pitchOffset);
                        audioSource.PlayOneShot(currentDialog.Current.scrollSound);
                    }

                    ++currentPosition;
                }
                else
                {
                    dialogSystemState = State.WaitingForContinue;
                }
                break;
            case State.WaitingForContinue:
                if (!currentDialog.Current.autoContinue)
                {
                    continueText.gameObject.SetActive(true);
                }
                else
                {
                    if (continueTimer > 0)
                    {
                        continueTimer -= Time.fixedDeltaTime;
                    }
                    else
                    {
                        NextLine();
                    }
                }
                break;
        }
    }

    public bool Play(GameDialog gameDialog)
    {
        if (dialogSystemState == State.Inactive && gameDialog.segments.Length > 0)
        {
            PlayerController.Controls.Dialogue.Enable();
            dialogSystemState = State.DisplayingLine;

            currentDialog = gameDialog;

            Command cmd = gameDialog.Current;
            if (titleText != null)
            {
                titleText.text = cmd.title;
            }
            contentText.text = "";

            continueTimer = autoContinueDelaySeconds;

            gameObject.SetActive(true);

            if (!currentDialog.Current.autoContinue)
            {
                prevLevelState = gameObject.MyLevelManager().levelState;
                gameObject.MyLevelManager().levelState = LevelState.Dialogue;
            }

            return true;
        }
        return false;
    }

    private void NextLine()
    {
        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }
        currentPosition = 0;
        continueTimer = autoContinueDelaySeconds;
        bool autoContinue = currentDialog.Current.autoContinue;

        if (currentDialog.MoveNext())
        {
            dialogSystemState = State.DisplayingLine;

            if (titleText != null)
            {
                titleText.text = currentDialog.Current.title;
            }
        }
        else
        {
            if (!autoContinue)
            {
                gameObject.MyLevelManager().levelState = prevLevelState;
            }
            dialogSystemState = State.Inactive;
            PlayerController.Controls.Dialogue.Enable();
            gameObject.SetActive(false);
        }
    }

    private void OnContinue(InputAction.CallbackContext context)
    {
        switch(dialogSystemState)
        {
            case State.DisplayingLine:
                if (!currentDialog.Current.autoContinue)
                {
                    currentPosition = currentDialog.Current.content.Length;
                    contentText.text = currentDialog.Current.content.Substring(0, currentPosition);
                }
                break;
            case State.WaitingForContinue:
                if (!currentDialog.Current.autoContinue)
                {
                    NextLine();
                }
                break;
        }        
    }
}
