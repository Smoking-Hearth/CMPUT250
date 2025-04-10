using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyScreenScript: MonoBehaviour
{
    [SerializeField] Button back;
    [SerializeField] Button easy;
    [SerializeField] Button normal;
    [SerializeField] Button hard;
    [SerializeField] TextMeshProUGUI difficultyText;


    public void easyDifficulty(){
        
        PlayerShoot.DamageMultiplier = 1.5f;
        DevLog.Info("Easy: Damage Multiplier set to 1.5f");
        difficultyText.text = "Easy";

    }
    public void normalDifficulty(){
        
        PlayerShoot.DamageMultiplier = 1f;
        DevLog.Info("Normal: Damage Multiplier set to 1f");
        difficultyText.text = "Normal";

    }
    public void hardDifficulty(){
        
        PlayerShoot.DamageMultiplier = 0.65f;
        DevLog.Info("Hard: Damage Multiplier set to 0.65f");
        difficultyText.text = "Hard";

    }
}