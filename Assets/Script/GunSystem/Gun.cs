using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    // 父对象与特效
    Transform bulletParent;
    Transform shellParent;
    MuzzleFlash muzzleFlash;

    // 状态控制
    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int bulletsRemainingInMag;
    bool isReloading;
    bool isFiringBurst;

    // 后坐力
    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampVelocity;
    float recoilAngle;
    Vector2 kickMinMax = new Vector2(.05f, .2f);            // 位移范围
    Vector2 recoilAngleMinMax = new Vector2(3f, 5f);        // 角度范围
    float recoilMoveSettleTime = .1f;                       // 位移复位时间
    float recoilRotationSettleTime = .1f;                   // 角度复位时间
    CinemachineImpulseSource impulseSource;

    public enum FireMode{ Auto, Burst, Single }
    public FireMode fireMode;

    [Header("Gun Components")]
    public Transform muzzle;
    public GameObject bullet;
    public GameObject shell;
    public Transform shellEjector;

    [Header("Object Pools")]
    public ObjectPool<GameObject> bulletPool;
    public ObjectPool<GameObject> shellPool;

    [Header("Gun Parameters")]
    public float msBetweenShots = 150f;
    public float muzzleVelocity = 15f;

    [Header("Burst Parameters")]
    public int burstCount;
    public int bulletsPerMag;
    public float reloadTime;

    [Header("SFX")]
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    // 只读属性与事件
    public int BulletsRemainingInMag => bulletsRemainingInMag;

    public event System.Action<FireMode> OnFireModeChanged;
    public event System.Action<int, int> OnAmmoChanged;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();

        shotsRemainingInBurst = burstCount;
        bulletsRemainingInMag = bulletsPerMag;
    }

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
    }

    void LateUpdate()
    {
        // Animate Recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0f, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isReloading && bulletsRemainingInMag == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && bulletsRemainingInMag > 0)
        {
            if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            FireOneBullet();
        }
    }

    void FireOneBullet()
    {
        bulletsRemainingInMag--;
        OnAmmoChanged?.Invoke(bulletsRemainingInMag, bulletsPerMag);

        nextShotTime = Time.time + msBetweenShots / 1000f;

        GameObject bulletObject = bulletPool.Get();
        if (bulletObject != null)
        {
            bulletObject.GetComponent<Bullet>().SetSpeed(muzzleVelocity);
        }

        shellPool.Get();
        muzzleFlash.Activate();

        // 后坐力动画
        transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
        recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
        recoilAngle = Mathf.Clamp(recoilAngle, 0f, 30f);
        CameraShakeManager.instance.CameraShake(impulseSource);

        // 音效
        AudioManager.instance.PlaySound(shootAudio, transform.position);
    }

    public void OnTriggerHold()
    {
        if (fireMode == FireMode.Burst)
        {
            if (triggerReleasedSinceLastShot && !isFiringBurst)
            {
                StartCoroutine(FireBurst());
            }
        }
        else
        {
            Shoot();
        }

        triggerReleasedSinceLastShot = false;
    }

    IEnumerator FireBurst()
    {
        if (isReloading || bulletsRemainingInMag == 0)
        {
            yield break;
        }

        isFiringBurst = true;
        float shotDelay = msBetweenShots / 1000f;

        for (int i = 0; i < burstCount; i++)
        {
            if (bulletsRemainingInMag > 0)
            {
                FireOneBullet();
                yield return new WaitForSeconds(shotDelay);
            }
            else
            {
                break;
            }
        }

        yield return new WaitForSeconds(.3f);

        isFiringBurst = false;
    }

    public void Reload()
    {
        if (!isReloading && bulletsRemainingInMag != bulletsPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.1f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0f;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 20f;

        while (percent < 1f)
        {
            percent += Time.deltaTime * reloadSpeed;

            float interpolation = (-Mathf.Pow(percent, 2f) + percent) * 4f;
            float reloadAngle = Mathf.Lerp(0f, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        bulletsRemainingInMag = bulletsPerMag;
        OnAmmoChanged?.Invoke(bulletsRemainingInMag, bulletsPerMag);
    }

    public void Aim(Vector3 aimPoint)
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
        }
    }

    public void SwitchFireModeNext()
    {
        fireMode = (FireMode)(((int)fireMode + 1) % System.Enum.GetValues(typeof(FireMode)).Length);
        OnFireModeChanged?.Invoke(fireMode);
    }

    public void SwitchFireModePrev()
    {
        int modeCount = System.Enum.GetValues(typeof(FireMode)).Length;
        fireMode = (FireMode)(((int)fireMode - 1 + modeCount) % modeCount);
        OnFireModeChanged?.Invoke(fireMode);
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
