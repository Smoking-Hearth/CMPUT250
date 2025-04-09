using UnityEngine;
using UnityEngine.UI;

public class DifficultyScreenScript: MonoBehaviour
{
    [SerializeField] Button back;
    [SerializeField] Button easy;
    [SerializeField] Button normal;
    [SerializeField] Button hard;


    public void easyDifficulty(){
        
        PlayerShoot.DamageMultiplier = 1.5f;
        Debug.Log("Easy: Damage Multiplier set to 1.5f");
        //easy.image.color = new Color(241,152,41,255);

    }
    public void normalDifficulty(){
        
        PlayerShoot.DamageMultiplier = 1f;
        Debug.Log("Normal: Damage Multiplier set to 1f");
        //easy.image.color = new Color(255,255,255,255);

    }
    public void hardDifficulty(){
        
        PlayerShoot.DamageMultiplier = 0.65f;
        Debug.Log("Hard: Damage Multiplier set to 0.65f");

    }

    void OnBackClick()
    {
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.GoBack());
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.Unload((int)SceneIndex.Credits));
    }
}