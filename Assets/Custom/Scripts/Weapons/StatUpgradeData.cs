using UnityEngine;

[CreateAssetMenu(fileName = "NewStatUpgrade", menuName = "Weapons/StatUpgradeData")]
public class StatUpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Stat Bonuses")]
    public int healthBonus = 0; // Increases Max Health and Heals
    public float speedBonus = 0f; // Additive to moveSpeed
    public float damageMultiplierBonus = 0f; // Additive to damageMultiplier (e.g. 0.1 for +10%)
    public float fireRateMultiplierBonus = 0f; // Additive to fireRateMultiplier (e.g. 0.1 for +10%)
}
