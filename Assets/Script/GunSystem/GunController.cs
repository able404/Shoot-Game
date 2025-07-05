using UnityEngine;

public class GunController : MonoBehaviour
{
    Gun equippedGun;
    UIController uiController;

    public Transform weaponHold;
    public Gun startingGun;

    public float GunHeight => weaponHold.position.y;

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();

        if (startingGun != null)
        {
            EquipGun(startingGun);
        }
    }

    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null)
        {
            equippedGun.OnFireModeChanged -= OnFireModeChanged;
            equippedGun.OnAmmoChanged -= OnAmmoChanged;
            Destroy(equippedGun.gameObject);
        }

        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;

        equippedGun.OnFireModeChanged += OnFireModeChanged;
        equippedGun.OnAmmoChanged += OnAmmoChanged;

        OnFireModeChanged(equippedGun.fireMode);
        OnAmmoChanged(equippedGun.BulletsRemainingInMag, equippedGun.bulletsPerMag);
    }

    void OnAmmoChanged(int currentAmmo, int magSize)
    {
        if (uiController != null)
        {
            uiController.UpdateAmmoUI(currentAmmo, magSize);
        }
    }

    void OnFireModeChanged(Gun.FireMode newMode)
    {
        if (uiController != null)
        {
            uiController.UpdateFireModeUI(newMode.ToString());
        }
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }

    public void SwitchFireModeNext()
    {
        if (equippedGun != null)
        {
            equippedGun.SwitchFireModeNext();
        }
    }

    public void SwitchFireModePrev()
    {
        if (equippedGun != null)
        {
            equippedGun.SwitchFireModePrev();
        }
    }
}
