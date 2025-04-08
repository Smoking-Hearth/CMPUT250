using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
                    DialogSegment s = segments[segmentIndex];
                    return new DialogSystem.Command(lines[lineIndex], s.title, s.autoContinue,
                        s.scrollSound, s.startSound, s.DoEvent, s.font, s.image, s.background, s.backgroundMultiplier, s.isTiled);
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
        public AudioClip startSound;
        public TMP_FontAsset font;
        public Sprite image;
        public Sprite background;
        public Color backgroundMultiplier;
        public bool isTiled;
        public string content;
        public string title;
        public bool autoContinue;
        public UnityEvent DoEvent;

        public Command(
            string content, 
            string title = null, 
            bool autoContinue = false, 
            AudioClip scrollSound = null, 
            AudioClip startSound = null, 
            UnityEvent doEvent = null, 
            TMP_FontAsset font = null,
            Sprite image = null,
            Sprite background = null,
            Color? backgroundMultiplier = null,
            bool isTiled = false
        )
        {
            this.scrollSound = scrollSound;
            this.startSound = startSound;
            this.content = content;
            this.title = title;
            this.autoContinue = autoContinue;
            this.DoEvent = doEvent; 
            this.font = font;
            this.image = image;
            this.background = background;
            if (backgroundMultiplier != null)
            {
                this.backgroundMultiplier = (Color)backgroundMultiplier;
            }
            else
            {
                this.backgroundMultiplier = Color.white;
            }
            this.isTiled = isTiled; 
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
    [SerializeField] private Image image;

    [SerializeField] private Image background;
    [SerializeField] private Image thumbnailBackground;
    [SerializeField] private Image titleBackground;
    private Sprite defaultBackground;
    private Color defaultColor;
    

    [SerializeField] private RectTransform imageBackdrop;

    private float nextCharTimer;
    private float continueTimer;

    int currentPosition = 0;

    [SerializeField] private TMP_FontAsset defaultDialogFont;

    private LevelState prevLevelState;


    private Sprite Background 
    {
        set 
        {
            if (value == null)
            {
                background.sprite = defaultBackground;
                thumbnailBackground.sprite = defaultBackground;
                titleBackground.sprite = defaultBackground;
            }
            else 
            {
                background.sprite = value;
                thumbnailBackground.sprite = value;
                titleBackground.sprite = value;
            }
        }
    }

    private Color BackgroundMultiplier
    {
        set
        {
            if (value == null)
            {
                background.color = defaultColor;
                thumbnailBackground.color = defaultColor;
                titleBackground.color = defaultColor;
            }
            else
            {
                background.color = value;
                thumbnailBackground.color = value;
                titleBackground.color = value;
            }
        }
    }

    private bool BackgroundTiled
    {
        set
        {
            if (value)
            {
                background.type = Image.Type.Tiled;
                thumbnailBackground.type = Image.Type.Tiled;
                titleBackground.type = Image.Type.Tiled;
            }
            else
            {
                background.type = Image.Type.Sliced;
                thumbnailBackground.type = Image.Type.Sliced;
                titleBackground.type = Image.Type.Sliced;
            }
        }
    }

    private void SetImage(Sprite sprite = null)
    {
        if (image == null) return;

        if (sprite == null)
        {
            imageBackdrop.gameObject.SetActive(false);
        }
        else
        {
            image.preserveAspect = true;
            image.sprite = sprite;
            imageBackdrop.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (image != null)
        {
            SetImage();
        }

        defaultBackground = background.sprite;
        defaultColor = background.color;

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
                    if (nextCharTimer > 0)
                    {
                        nextCharTimer -= Time.fixedDeltaTime;
                        break;
                    }
                    contentText.text = currentDialog.Current.content.Substring(0, currentPosition);

                    if (currentPosition % 3 == 0 && currentDialog.Current.scrollSound != null && currentPosition < currentDialog.Current.content.Length)
                    {
                        float pitchOffset = (currentDialog.Current.content[currentPosition] % 32) / 32f;
                        audioSource.pitch = 0.5f + pitchOffset;
                        audioSource.PlayOneShot(currentDialog.Current.scrollSound);
                    }

                    if (currentPosition >= 1)
                    {
                        char currentCharacter = currentDialog.Current.content[currentPosition - 1];
                        if (currentCharacter == '.' || currentCharacter == '?' || currentCharacter == '!')
                        {
                            nextCharTimer = 0.2f;
                        }
                        else if (currentCharacter == ',')
                        {
                            nextCharTimer = 0.1f;
                        }
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
            contentText.text = "";

            continueTimer = autoContinueDelaySeconds;

            gameObject.SetActive(true);

            if (!currentDialog.Current.autoContinue)
            {
                prevLevelState = gameObject.MyLevelManager().levelState;
                gameObject.MyLevelManager().levelState = LevelState.Dialogue;
            }

            SetupForSegment();

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

        if (currentDialog.Current.DoEvent != null)
        {
            currentDialog.Current.DoEvent.Invoke();
        }

        if (currentDialog.MoveNext())
        {
            dialogSystemState = State.DisplayingLine;
            SetupForSegment();
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

    private void SetupForSegment() 
    {
        Command cmd = currentDialog.Current;
        if (titleText != null)
        {
            titleText.text = cmd.title;
        }
        if (cmd.startSound != null)
        {
            audioSource.PlayOneShot(cmd.startSound);
        }
        if (cmd.font != null)
        {
            contentText.font = cmd.font;
        }
        else 
        {
            contentText.font = defaultDialogFont;
        }
        Background = cmd.background;
        BackgroundMultiplier = cmd.backgroundMultiplier;
        BackgroundTiled = cmd.isTiled;
        SetImage(cmd.image);
    }

    private void OnContinue(InputAction.CallbackContext context)
    {
        if (gameObject.MyLevelManager().levelState != LevelState.Dialogue)
        {
            return;
        }

        nextCharTimer = 0;
        switch (dialogSystemState)
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
