using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    Transform bulletParent;
    Transform shellParent;
    MuzzleFlash muzzleFlash;

    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }
    public FireMode fireMode;

    public Transform muzzle;
    public GameObject bullet;
    public GameObject shell;
    public Transform shellEjector;
    public ObjectPool<GameObject> bulletPool;
    public ObjectPool<GameObject> shellPool;

    public float msBetweenShots = 150f;
    public float muzzleVelocity = 15f;
    public int burstCount;

    void Start()
    {
        bulletParent = GameObject.Find("Bullets")?.transform;
        if (bulletParent == null)
            bulletParent = new GameObject("Bullets").transform;

        shellParent = GameObject.Find("Shells")?.transform;
        if (shellParent == null)
            shellParent = new GameObject("Shells").transform;

        bulletPool = new ObjectPool<GameObject>(CreateBullet, GetBullet, ReleaseBullet, DisposeBullet, true);
        shellPool = new ObjectPool<GameObject>(CreateShell, GetShell, ReleaseShell, DisposeShell, true);

        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
    }

    void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            nextShotTime = Time.time + msBetweenShots / 1000f;

            GameObject bulletObject = bulletPool.Get();
            if (bulletObject != null)
            {
                bulletObject.GetComponent<Bullet>().SetSpeed(muzzleVelocity);
            }

            shellPool.Get();

            muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }

    GameObject CreateBullet()
    {
        GameObject bulletObject = Instantiate(bullet, muzzle.position, muzzle.rotation);

        bulletObject.GetComponent<Bullet>().poolToReturnTo = bulletPool;
        return bulletObject;
    }

    void GetBullet(GameObject bullet)
    {
        bullet.transform.position = muzzle.position;
        bullet.transform.rotation = muzzle.rotation;
        bullet.transform.SetParent(bulletParent);
        bullet.SetActive(true);
    }

    void ReleaseBullet(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    void DisposeBullet(GameObject bullet)
    {
        Destroy(bullet);
    }

    GameObject CreateShell()
    {
        GameObject shellObject = Instantiate(shell.gameObject, shellEjector.position, shellEjector.rotation);

        shellObject.GetComponent<Shell>().poolToReturnTo = shellPool;
        return shellObject;
    }

    void GetShell(GameObject shellObj)
    {
        shellObj.transform.position = shellEjector.position;
        shellObj.transform.rotation = shellEjector.rotation;
        shellObj.transform.SetParent(shellParent);
        shellObj.SetActive(true);
    }

    void ReleaseShell(GameObject shellObj)
    {
        shellObj.SetActive(false);
    }

    void DisposeShell(GameObject shellObj)
    {
        Destroy(shellObj);
    }
}
