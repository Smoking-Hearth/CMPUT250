using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup tutorialPopup;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text tutorialTitleText;
    [SerializeField] private TMP_Text tutorialPopupText;

    [SerializeField] private TutorialInfo[] tutorials;
    private float currentDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        //Doesn't put a duration on the tutorial if it is less than or equal to zero
        if (currentDuration > 0)
        {
            currentDuration -= Time.fixedDeltaTime;
            
            //Hide the tutorial when the current duration ends
            if (currentDuration <= 0)
            {
                HideTutorial();
            }
        }
    }

    //Brings up the tutorial with the specified index from the tutorials array
    public void PopupTutorial(int index)
    {
        tutorialPopup.gameObject.SetActive(true);
        tutorialTitleText.text = tutorials[index].title;
        tutorialPopupText.text = tutorials[index].message;
        currentDuration = tutorials[index].duration;

        if (tutorials[index].image != null)
        {
            tutorialImage.sprite = tutorials[index].image;
            tutorialImage.gameObject.SetActive(true);
        }
    }

    //Hides the current tutorial being displayed
    public void HideTutorial()
    {
        tutorialPopup.gameObject.SetActive(false);
        tutorialImage.gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct TutorialInfo
{
    public string title;
    [TextArea(0, 5)]
    public string message;
    public Sprite image;
    public float duration;
}
