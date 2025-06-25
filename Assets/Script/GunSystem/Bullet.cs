using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    float speed;
    float maxLifetime = 2f;
    float damage = 1f;

    public LayerMask collisionMask;
    public ObjectPool<GameObject> poolToReturnTo;

    void OnEnable()
    {
        Invoke(nameof(ReleaseBullet), maxLifetime);
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
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit)
    {
        IDamageable damageableObj = hit.collider.GetComponent<IDamageable>();
        if (damageableObj!= null)
        {
            damageableObj.TakeHit(damage, hit);
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
