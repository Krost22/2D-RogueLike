using UnityEngine;

public class ProjectileWeapon : WeaponBehavior
{
    protected override void Attack()
    {
        if (data.prefab == null) return;

        StartCoroutine(FireBurst());
    }

    private System.Collections.IEnumerator FireBurst()
    {
        int count = data.projectileCount;
        if (count <= 0) count = 1;

        for (int i = 0; i < count; i++)
        {
            SpawnProjectile();
            if (i < count - 1 && data.timeBetweenProjectiles > 0)
            {
                yield return new WaitForSeconds(data.timeBetweenProjectiles);
            }
        }
    }

    private void SpawnProjectile()
    {
        // Spawn projectile
        GameObject proj = PoolManager.Instance.Spawn(data.prefab, transform.position, transform.rotation);
        PlayerProjectile pp = proj.GetComponent<PlayerProjectile>();
        
        if (pp != null)
        {
            // Use current rotation for direction
            Vector3 dir = transform.right; 
            
            pp.Initialize(dir, data.damage, data.speed, data.isExplosive, data.areaOfEffect);
        }
    }
}
