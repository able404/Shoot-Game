using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class Shell : MonoBehaviour
{
    float lifetime = 2f;
    float fadetime = 1f;

    public Rigidbody theRB;
    public float forceMin;
    public float forceMax;

    public ObjectPool<GameObject> poolToReturnTo;

    void OnEnable()
    {
        float force = Random.Range(forceMin, forceMax);
        theRB.AddForce(transform.right * force);
        theRB.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);

        float percent = 0f;
        float fadeSpeed = 1f / fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while (percent < 1f)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }

        ReleaseShell();
    }

    void ReleaseShell()
    {
        StopAllCoroutines();
        if (poolToReturnTo != null)
        {
            poolToReturnTo.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
