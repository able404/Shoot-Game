using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    TrailRenderer trail;

    float speed;
    float maxLifetime = 2f;
    float damage = 1f;
    float skinWidth = .1f;

    public LayerMask collisionMask;
    public ObjectPool<GameObject> poolToReturnTo;

    void Awake()
    {
        trail = GetComponentInChildren<TrailRenderer>();
    }

    void OnEnable()
    {
        if (trail != null)
        {
            trail.Clear();
        }
        Invoke(nameof(ReleaseBullet), maxLifetime);
    }

    void Start()
    {
        Collider[] initialColliders = Physics.OverlapSphere(transform.position, skinWidth, collisionMask);
        if (initialColliders.Length > 0)
        {
            OnHitObject(initialColliders[0], transform.position);
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject (Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObj = c.GetComponent<IDamageable>();
        if (damageableObj!= null)
        {
            damageableObj.TakeHit(damage, hitPoint, transform.forward);
        }

        ReleaseBullet();
    }

    void ReleaseBullet()
    {
        CancelInvoke();

        if (poolToReturnTo != null)
        {
            poolToReturnTo.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }    
}
