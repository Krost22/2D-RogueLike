using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [TextArea] public string description;
    public GameObject prefab; // Projectile prefab or Effect prefab
    public float damage = 10f;
    public float fireRate = 1f; // Shots per second
    public float speed = 10f; // Projectile speed
    public bool isMelee = false;
    public bool isExplosive = false;
    public float areaOfEffect = 0f;
    
    [Header("Burst Settings")]
    public int projectileCount = 1;
    public float timeBetweenProjectiles = 0.1f;
}
