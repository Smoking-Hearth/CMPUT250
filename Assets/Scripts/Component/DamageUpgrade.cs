using UnityEngine;
using System.Collections.Generic;

public class DamageUpgrade : MonoBehaviour
{
    private static List<DamageUpgrade> obtainedUpgrades = new List<DamageUpgrade>();
    [SerializeField] private float addedMultiplier;
    [SerializeField] private GameObject obtainableObject;
    [SerializeField] private GameObject greyObject;

    private void OnEnable()
    {
        if (obtainedUpgrades.Contains(this))
        {
            greyObject.SetActive(true);
            obtainableObject.SetActive(false);
        }
    }
    public void Obtain()
    {
        PlayerShoot.DamageMultiplier += addedMultiplier;
        obtainedUpgrades.Add(this);
        Destroy(gameObject);
    }
}
