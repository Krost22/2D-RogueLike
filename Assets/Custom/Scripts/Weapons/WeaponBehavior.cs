using UnityEngine;

public abstract class WeaponBehavior : MonoBehaviour
{
    public WeaponData data;
    protected float nextFireTime;

    public virtual void Initialize(WeaponData weaponData)
    {
        data = weaponData;
    }

    public virtual void TryAttack()
    {
        if (Time.time >= nextFireTime)
        {
            Attack();
            nextFireTime = Time.time + 1f / data.fireRate;
        }
    }

    protected abstract void Attack();
}
