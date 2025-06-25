using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    float nextShotTime;

    public Transform muzzle;
    public GameObject bullet;
    public ObjectPool<GameObject> bulletPool;

    public float msBetweenShots = 200f;
    public float muzzleVelocity = 15f;

    void Start()
    {
        bulletPool = new ObjectPool<GameObject>(CreateBullet, GetBullet, ReleaseBullet, DisposeBullet, true, 10, 100);
    }

    void Update()
    {
        
    }

    GameObject CreateBullet()
    {
        GameObject bulletObject = Instantiate(bullet, muzzle.position, muzzle.rotation);

        bulletObject.GetComponent<Bullet>().poolToReturnTo = this.bulletPool;
        return bulletObject;
    }

    void GetBullet(GameObject bullet)
    {
        bullet.transform.position = muzzle.position;
        bullet.transform.rotation = muzzle.rotation;

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

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000f;

            GameObject bulletObject = bulletPool.Get();
            if (bulletObject != null)
            {
                bulletObject.GetComponent<Bullet>().SetSpeed(muzzleVelocity);
            }
        }
    }
}
