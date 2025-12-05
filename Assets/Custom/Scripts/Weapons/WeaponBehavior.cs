using UnityEngine;

public abstract class WeaponBehavior : MonoBehaviour
{
    public WeaponData data;
    protected float nextFireTime;
    protected PlayerController player;

    public virtual void Initialize(WeaponData weaponData)
    {
        data = weaponData;
        player = GetComponent<PlayerController>();
    }

    public virtual void TryAttack()
    {
        if (Time.time >= nextFireTime)
        {
            Attack();
            
            float currentFireRate = data.fireRate;
            if (player != null)
            {
                currentFireRate *= player.fireRateMultiplier;
            }
            
            // Prevent division by zero
            if (currentFireRate <= 0) currentFireRate = 0.1f;

            nextFireTime = Time.time + 1f / currentFireRate;
        }
    }

    protected abstract void Attack();
}
