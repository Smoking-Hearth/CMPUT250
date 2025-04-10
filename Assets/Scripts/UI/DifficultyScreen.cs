using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyScreenScript: MonoBehaviour
{
    public static int difficulty;

    [SerializeField] Button back;
    [SerializeField] Button easy;
    [SerializeField] Button normal;
    [SerializeField] Button hard;
    [SerializeField] TextMeshProUGUI difficultyText;


    public void easyDifficulty(){

        difficulty = 1;
        DevLog.Info("Easy: Difficulty level set to 1");
        difficultyText.text = "Easy";

    }
    public void normalDifficulty(){

        difficulty = 2;
        DevLog.Info("Normal: Difficulty level set to 2");
        difficultyText.text = "Normal";

    }
    public void hardDifficulty(){

        difficulty = 3;
        DevLog.Info("Hard: Difficulty level set to 3");
        difficultyText.text = "Hard";

    }
}