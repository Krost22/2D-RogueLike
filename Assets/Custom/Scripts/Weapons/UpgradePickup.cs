using UnityEngine;

public class UpgradePickup : MonoBehaviour
{
    public WeaponData weaponUpgrade;
    public StatUpgradeData statUpgrade;
    public SpriteRenderer iconRenderer;

    private void Start()
    {
        if (iconRenderer != null)
        {
            if (weaponUpgrade != null)
            {
                iconRenderer.sprite = weaponUpgrade.icon;
            }
            else if (statUpgrade != null)
            {
                iconRenderer.sprite = statUpgrade.icon;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.weaponController != null)
            {
                // Apply upgrade
                if (weaponUpgrade != null)
                {
                    player.weaponController.AddWeapon(weaponUpgrade);
                }
                else if (statUpgrade != null)
                {
                    player.ApplyStatUpgrade(statUpgrade);
                }

                // Notify HordeManager
                HordeManager horde = FindFirstObjectByType<HordeManager>();
                if (horde != null)
                {
                    horde.OnUpgradePicked(this);
                }

                // Destroy self is handled by HordeManager clearing pickups, 
                // but we can disable collider to prevent double trigger
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}
