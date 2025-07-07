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
        Renderer skinRenderer = GetComponentInChildren<Renderer>();
        if (skinRenderer != null)
        {
            skinMaterial = skinRenderer.material;
            originalColor = skinMaterial.color;
        }

        health = startingHealth;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }
        damageFlashCoroutine = StartCoroutine(DamageFlash());

        if (this is PlayerController)
        {
            var ui = FindFirstObjectByType<UIController>();
            if (ui != null)
            {
                ui.UpdateHealthUI(health);
            }
        }

        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    public void RestoreFullHealth()
    {
        health = startingHealth;
        isDead = false;

        if (this is PlayerController)
        {
            var ui = FindFirstObjectByType<UIController>();
            if (ui != null)
            {
                ui.UpdateHealthUI(health);
            }
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

        if (this is PlayerController)
        {
            AudioManager.instance.PlaySound("Player Death", transform.position);
        }
        Destroy(gameObject);
    }

    IEnumerator DamageFlash()
    {
        skinMaterial.color = damageFlashColor;
        float flashTimer = 0f;

        while (flashTimer < flashDuration)
        {
            flashTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        skinMaterial.color = originalColor;
    }
}
