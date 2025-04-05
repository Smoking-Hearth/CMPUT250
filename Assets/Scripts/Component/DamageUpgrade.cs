using UnityEngine;
using System.Collections.Generic;

public class DamageUpgrade : MonoBehaviour
{
    private static List<int> obtainedUpgrades = new List<int>();
    [SerializeField] private int id;
    [SerializeField] private float addedMultiplier;
    [SerializeField] private GameObject obtainableObject;
    [SerializeField] private GameObject greyObject;

    private void OnEnable()
    {
        if (obtainedUpgrades.Contains(id))
        {
            greyObject.SetActive(true);
            obtainableObject.SetActive(false);
        }
    }
    public void Obtain()
    {
        PlayerShoot.DamageMultiplier += addedMultiplier;
        obtainedUpgrades.Add(id);
        greyObject.SetActive(true);
        obtainableObject.SetActive(false);
    }
}
