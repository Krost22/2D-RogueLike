using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public List<WeaponData> startingWeapons;
    private List<WeaponBehavior> activeWeapons = new List<WeaponBehavior>();

    void Start()
    {
        foreach (var data in startingWeapons)
        {
            AddWeapon(data);
        }
    }

    void Update()
    {
        // Check if we are in Calm Phase
        if (HordeManager.Instance != null && HordeManager.Instance.currentState == HordeManager.HordeState.CalmPhase)
        {
            return;
        }

        // Auto-fire all weapons
        foreach (var weapon in activeWeapons)
        {
            weapon.TryAttack();
        }
    }

    public void AddWeapon(WeaponData data)
    {
        WeaponBehavior behavior = null;

        if (data.isMelee)
        {
            behavior = gameObject.AddComponent<MeleeWeapon>();
        }
        else
        {
            behavior = gameObject.AddComponent<ProjectileWeapon>();
        }

        behavior.Initialize(data);
        activeWeapons.Add(behavior);
        Debug.Log($"Added weapon: {data.weaponName}");
    }
}
