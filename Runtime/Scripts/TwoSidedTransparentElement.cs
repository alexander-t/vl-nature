using UnityEngine;

[System.Serializable]
public class ScaledMaterial
{
    public Material material;
    [Range(0.1f, 2.0f)]
    public float scale;
}
public class TwoSidedTransparentElement : MonoBehaviour
{
    [SerializeField] ScaledMaterial[] materials;

    private readonly Quaternion Left = Quaternion.Euler(new Vector3(0, 0, -0.5f));
    private readonly Quaternion Right = Quaternion.Euler(new Vector3(0, 0, 0.5f));

    float timer = 0;
    float nextTurn = 1.0f;
    bool turning = false;
    void Start()
    {
        nextTurn = Random.Range(1.0f, 3.0f);

        ScaledMaterial material = materials[Random.Range(0, materials.Length)];
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.material = material.material;
        }
        transform.localScale = new Vector3(1, Random.Range(0.1f, material.scale), 1);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= nextTurn) {
            transform.rotation *= turning ? Right : Left;
            
            timer = 0;
            turning = !turning;
            nextTurn = Random.Range(1.0f, 3.0f);
        }
    }
}
