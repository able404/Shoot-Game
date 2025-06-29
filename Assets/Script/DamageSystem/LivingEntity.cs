using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour, IDamageable
{
    Material skinMaterial;
    Color originalColor;
    Coroutine damageFlashCoroutine;

    protected float health;
    protected bool isDead;

    public float startingHealth;
    public Color damageFlashColor;
    float flashDuration = .2f;

    public event System.Action OnDeath;

    protected virtual void Start()
    {
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit)
    {

        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }
        damageFlashCoroutine = StartCoroutine(DamageFlash());

        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    protected void Die()
    {
        isDead = true;
        if (OnDeath != null)
        {
            OnDeath();
        }
        Destroy(gameObject);
    }

    IEnumerator DamageFlash()
    {
        skinMaterial.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        skinMaterial.color = originalColor;
    }
}
