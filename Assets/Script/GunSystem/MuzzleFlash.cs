using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    float flashTime = .05f;

    public GameObject flashHolder;
    public Sprite[] flashSprites;
    public SpriteRenderer[] spriteRenderers;

    void Start()
    {
        Deactivate();
    }

    public void Activate()
    {
        flashHolder.SetActive(true);

        int flashSpriteIndex = Random.Range(0, flashSprites.Length);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
        }

        Invoke(nameof(Deactivate), flashTime);
    }

    void Deactivate()
    {
        flashHolder.SetActive(false);
    }
}
