using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    HashSet<Collider> hitColliders = new HashSet<Collider>();

    TrailRenderer trail;

    float speed;
    float maxLifetime = 2f;
    float damage = 1f;
    float skinWidth = .3f;

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
        hitColliders.Clear();

        Collider[] initialColliders = Physics.OverlapSphere(transform.position, skinWidth, collisionMask, QueryTriggerInteraction.Collide);
        foreach (var col in initialColliders)
        {
            if (!hitColliders.Contains(col))
            {
                OnHitObject(col, transform.position);
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy) return;

        float moveDistance = speed * Time.fixedDeltaTime;
        int steps = Mathf.CeilToInt(moveDistance / skinWidth);
        float stepDistance = moveDistance / steps;

        for (int i = 0; i < steps; i++)
        {
            if (CheckCollisions(stepDistance))
                break;
            transform.Translate(Vector3.forward * stepDistance);
            if (!gameObject.activeInHierarchy) break;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    bool CheckCollisions(float moveDistance)
    {
        Vector3 sphereCastStartPoint = transform.position - transform.forward * skinWidth;
        Ray ray = new Ray(sphereCastStartPoint, transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(ray, skinWidth, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            if (hit.distance > skinWidth && !hitColliders.Contains(hit.collider))
            {
                OnHitObject(hit.collider, hit.point);
                return true;
            }
        }
        return false;
    }

    void OnHitObject (Collider c, Vector3 hitPoint)
    {
        if (hitColliders.Contains(c)) return;
        hitColliders.Add(c);

        IDamageable damageableObj = c.GetComponentInParent<IDamageable>();
        if (damageableObj != null)
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
