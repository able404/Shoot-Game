using System.Collections;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    NavMeshAgent pathFinder;
    Transform target;
    LivingEntity targetEntity;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1f;
    float nextAttackTime;
    float damage = 1f;

    float myColliderRadius;
    float targetColliderRadius;

    bool hasTarget;

    public ParticleSystem deathEffect;

    public enum State{ Idle, Chasing, Attacking }
    State currentState;

    void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            CapsuleCollider myCollider = GetComponentInChildren<CapsuleCollider>();
            if (myCollider != null)
            {
                myColliderRadius = myCollider.radius;
            }

            CapsuleCollider targetCollider = target.GetComponentInChildren<CapsuleCollider>();
            if (targetCollider != null)
            {
                targetColliderRadius = targetCollider.radius;
            }
        }
    }

    protected override void Start()
    {
        base.Start();

        if (hasTarget)
        {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }
    }

    void Update()
    {
        bool shouldBeStopped = (UIController.CurrentState != GameState.Running);

        if (pathFinder.isOnNavMesh)
        {
            pathFinder.isStopped = shouldBeStopped;
        }
        if (shouldBeStopped) return;

        if (hasTarget && Time.time > nextAttackTime)
        {
            float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

            if (sqrDstToTarget < (Mathf.Pow(attackDistanceThreshold + myColliderRadius + targetColliderRadius, 2)))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                StartCoroutine(Attack());
            }
        }
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth)
    {
        pathFinder.speed = moveSpeed;

        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;

        StopAllCoroutines();
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathFinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * myColliderRadius;

        float percent = 0f;
        float attackSpeed = 3f;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        currentState = State.Chasing;
        pathFinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myColliderRadius + targetColliderRadius + attackDistanceThreshold/2);

                if (!isDead)
                    pathFinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    void OnDestroy()
    {
        if (targetEntity != null)
        {
            targetEntity.OnDeath -= OnTargetDeath;
        }
    }
}
