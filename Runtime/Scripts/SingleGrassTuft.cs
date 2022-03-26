using UnityEngine;

public class SingleGrassTuft : MonoBehaviour
{
    public float minScaleRange = 0.5f;
    public float maxScaleRange = 1.5f;

    void Start()
    {
        if (minScaleRange > maxScaleRange)
        {
            maxScaleRange = minScaleRange;
        }
        float scale = Random.Range(minScaleRange, maxScaleRange);
        transform.localScale = new Vector3(scale, scale, scale);
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 359), 0);
    }
}
