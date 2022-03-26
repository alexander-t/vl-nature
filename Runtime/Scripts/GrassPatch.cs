using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GrassPatch : MonoBehaviour
{
    public float radius = 5;
    public int tufts = 30;
    [SerializeField] GameObject prefab;
    [SerializeField] Material finalMeshMaterial;

    private readonly Vector3 BlenderScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Color startColor;
    private Color darkenedColor;
    private bool darkened = false;

    private float colorUpdateTime = 1.0f;
    private float colorElapsedTime =  0;

    void Start()
    {
        startColor = finalMeshMaterial.color;
        darkenedColor = new Color(startColor.r - 0.012f, startColor.g - 0.012f, startColor.b - 0.012f);

        CreateMeshes();
        CombineMeshes();
    }

    public void CreateMeshes()
    {
        for (int i = 0; i < tufts; ++i)
        {
            GameObject go = Instantiate(prefab, transform.gameObject.transform);
            Vector2 offset = Random.insideUnitCircle * radius;
            go.transform.Translate(new Vector3(offset.x, 0, offset.y));
        }
    }

    public void CombineMeshes()
    {
        Quaternion oldRotation = transform.rotation;
        Vector3 oldPosition = transform.position;

        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combiners = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; ++i)
        {

            // Skip parent container
            if (meshFilters[i].transform == transform)
            {
                continue;
            }

            combiners[i].subMeshIndex = 0;
            combiners[i].mesh = meshFilters[i].sharedMesh;

            Vector3 scale = new Vector3(Random.Range(0.5f, 0.85f), Random.Range(0.25f, 0.65f), Random.Range(0.5f, 0.85f));
            // Order is super important! First base transformation, then local to world!
            combiners[i].transform = Matrix4x4.Rotate(Quaternion.Euler(0, Random.Range(0, 359), 0))
                * Matrix4x4.Scale(scale) * Matrix4x4.Scale(BlenderScale)
                * meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combiners);
        GetComponent<MeshFilter>().sharedMesh = finalMesh;
        GetComponent<MeshRenderer>().material = finalMeshMaterial;

        transform.rotation = oldRotation;
        transform.position = oldPosition;

        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        colorElapsedTime += Time.deltaTime;
        if (colorElapsedTime > colorUpdateTime) { 
            finalMeshMaterial.color = darkened == true ? startColor : darkenedColor;
            colorUpdateTime = Random.Range(1.0f, 2.0f);
            colorElapsedTime = 0;
            darkened = !darkened;
        }
    }
}
