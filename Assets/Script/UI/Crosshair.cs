using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public LayerMask targetMask;
    public SpriteRenderer crosshairSR;
    public Color crosshairHighlightColor;
    Color originalCrosshairColor;

    void Start()
    {
        originalCrosshairColor = crosshairSR.color;
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * -40f * Time.deltaTime);
    }

    public void DetectTargets(Ray ray)
    {
        if (Physics.Raycast(ray, 100, targetMask))
        {
            crosshairSR.color = crosshairHighlightColor;
        }
        else
        {
            crosshairSR.color = originalCrosshairColor;
        }
    }
}
