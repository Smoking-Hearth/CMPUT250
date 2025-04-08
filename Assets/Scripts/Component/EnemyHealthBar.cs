using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text title;
    [SerializeField] private string enemyName;
    [SerializeField] private Slider slider;

    public void ActivateBar()
    {
        overlay.SetActive(true);
        title.text = enemyName;
    }    
    public void DeactivateBar()
    {
        if (overlay != null)
        {
            overlay.SetActive(false);
        }
    }
    public void UpdateHealthBar(float currentHealth, float maxHealth){

        slider.value = currentHealth/maxHealth;
    }
}
