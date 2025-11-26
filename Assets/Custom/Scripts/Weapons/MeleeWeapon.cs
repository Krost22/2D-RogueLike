using UnityEngine;

public class MeleeWeapon : WeaponBehavior
{
    protected override void Attack()
    {
        if (data.prefab == null) return;

        // Spawn melee effect attached to player or slightly offset
        Vector3 offset = transform.right * 1.5f; // Offset in facing direction
        GameObject slash = Instantiate(data.prefab, transform.position + offset, transform.rotation, transform);
        
        // The slash prefab should handle its own animation and damage via a collider/script
        // For simplicity, let's assume the prefab has a script that deals damage on trigger enter
        // We can pass damage data to it if needed.
        
        PlayerProjectile pp = slash.GetComponent<PlayerProjectile>();
        if (pp != null)
        {
            // Melee "projectile" that doesn't move much but has lifetime
            pp.Initialize(Vector3.zero, data.damage, 0f, false, 0f);
        }
    }
}
