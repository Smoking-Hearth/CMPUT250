using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup tutorialPopup;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text tutorialPopupText;

    [SerializeField] private TutorialInfo[] tutorials;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopupTutorial(int index)
    {
        tutorialPopup.gameObject.SetActive(true);
        tutorialPopupText.text = tutorials[index].message;

        if (tutorials[index].image != null)
        {
            tutorialImage.sprite = tutorials[index].image;
            tutorialImage.gameObject.SetActive(true);
        }
    }

    public void HideTutorial()
    {
        tutorialPopup.gameObject.SetActive(false);
        tutorialImage.gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct TutorialInfo
{
    [TextArea(0, 5)]
    public string message;
    public Sprite image;
}
