using System.Collections.Generic;
using UnityEngine;

namespace VLNature
{
    class SimpleTransform
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public SimpleTransform(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
        }
    }

    public class TreeGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("Prefabs that make up the trunk, branches, and leaves")]
        [SerializeField] GameObject trunkBranchPrefab;
        [SerializeField] GameObject trunkBranchLowResPrefab;
        [SerializeField] GameObject[] foliagePrefab;
        [SerializeField] GameObject mediumResPrefab;
        [SerializeField] GameObject lowResPrefab;

        [Header("Settings")]
        public int height = 5;
        [Range(3, 6)]
        public int maxTiers = 5;
        [Range(0.0f, 1.0f)]
        public float probabilityToBranchInMidsection = 0.5f;
        [Range(0.0f, 1.0f)]
        public float probabilityToBranchOnTrunk = 0.0f;
        public bool showFoliage = true;
        public bool pruneSmallBranches = true;
        public bool createLODGroup = true;

        private int[] ODD_EVEN = { -1, 1 };

        private LODGroup lodGroup;
        private List<Renderer> lod0Renderers = new List<Renderer>();
        private List<Renderer> lod1Renderers = new List<Renderer>();
        private List<Renderer> lod2Renderers = new List<Renderer>();

        void Start()
        {

            CreateSection(new SimpleTransform(transform.position, Vector3.one, Quaternion.identity), 0);

            if (createLODGroup)
            {
                lodGroup = gameObject.AddComponent<LODGroup>();
                LOD[] lods = new LOD[3];
                lods[0] = new LOD(0.5f, lod0Renderers.ToArray());
                lods[1] = new LOD(0.3f, lod1Renderers.ToArray());
                lods[2] = new LOD(0.1f, lod2Renderers.ToArray());
                lodGroup.SetLODs(lods);
                lodGroup.fadeMode = LODFadeMode.SpeedTree;
                lodGroup.RecalculateBounds();
            }
        }
        private void CreateSection(SimpleTransform origin, int tier, int oddEven = 1)
        {

            if (tier >= maxTiers)
            {
                return;
            }

            SimpleTransform newOrigin = new SimpleTransform(origin.position + new Vector3(0, height * origin.scale.y, 0), origin.scale, origin.rotation);

            if (tier > 0)
            {
                if (tier == 1)
                {
                    // Initial rotation; given that the tree is seen from above like a clock and the branch is a hand
                    newOrigin.rotation *= Quaternion.AngleAxis(oddEven * Random.Range(0, 360), Vector3.up);
                }

                // Up/down: higher values here make the tree flatter.
                newOrigin.rotation *= Quaternion.AngleAxis(Random.Range(10, 30), Vector3.forward);
                if (tier > 1)
                {
                    newOrigin.rotation *= Quaternion.AngleAxis(oddEven * Random.Range(30, 50), Vector3.left);
                }
                newOrigin.position = newOrigin.rotation * (newOrigin.position - origin.position) + origin.position;
            }

            // Optimization: Don't draw branches above a certain height
            if (!pruneSmallBranches || (pruneSmallBranches && tier <= 3))
            {
                // Divide into three separate sections
                GameObject lowSection = Instantiate(trunkBranchPrefab, origin.position, newOrigin.rotation);
                lowSection.transform.localScale = new Vector3(lowSection.transform.localScale.x * origin.scale.x, origin.scale.y * 0.34f * height, lowSection.transform.localScale.z * origin.scale.z);
                GameObject midSection = Instantiate(trunkBranchPrefab, origin.position + ((newOrigin.position - origin.position) / 3.0f), newOrigin.rotation);
                midSection.transform.localScale = new Vector3(lowSection.transform.localScale.x * 0.9f, lowSection.transform.localScale.y, lowSection.transform.localScale.z * 0.9f);
                GameObject topSection = Instantiate(trunkBranchPrefab, origin.position + ((newOrigin.position - origin.position) * (2.0f / 3)), newOrigin.rotation);
                topSection.transform.localScale = new Vector3(lowSection.transform.localScale.x * 0.8f, lowSection.transform.localScale.y, lowSection.transform.localScale.z * 0.8f);
                lod0Renderers.Add(lowSection.GetComponentInChildren<Renderer>());
                lod0Renderers.Add(midSection.GetComponentInChildren<Renderer>());
                lod0Renderers.Add(topSection.GetComponentInChildren<Renderer>());

                if (createLODGroup)
                {
                    // LOD 1 & 2: Use lowres prefab
                    GameObject lowResSection = Instantiate(trunkBranchLowResPrefab, origin.position, newOrigin.rotation);
                    lowResSection.transform.localScale = new Vector3(lowSection.transform.localScale.x, height * origin.scale.y, lowSection.transform.localScale.z);
                    lod1Renderers.Add(lowResSection.GetComponentInChildren<Renderer>());
                    lod2Renderers.Add(lowResSection.GetComponentInChildren<Renderer>());
                }
            }

            //Debug.DrawLine(origin.position, newOrigin.position, Color.green, 60);

            if (tier + 1 == maxTiers && showFoliage)
            {
                GameObject foliage = Instantiate(foliagePrefab[Random.Range(0, foliagePrefab.Length)], newOrigin.position, Quaternion.Euler(0, Random.Range(0.0f, 360), 0));
                foliage.transform.localScale *= Random.Range(3, 8);
                lod0Renderers.Add(foliage.GetComponentInChildren<Renderer>()); // Special case: Voxel asset has its renderer in the child object!

                if (createLODGroup)
                {
                    GameObject mediumResFoliage = Instantiate(mediumResPrefab, newOrigin.position, foliage.transform.rotation);
                    mediumResFoliage.transform.localScale = foliage.transform.localScale;
                    lod1Renderers.Add(mediumResFoliage.GetComponent<Renderer>());
                    foreach (Renderer renderer in mediumResFoliage.GetComponentsInChildren<Renderer>())
                    {
                        lod1Renderers.Add(renderer);
                    }

                    GameObject lowResFoliage = Instantiate(lowResPrefab, newOrigin.position, foliage.transform.rotation);
                    lowResFoliage.transform.localScale = foliage.transform.localScale;
                    foreach (Renderer renderer in lowResFoliage.GetComponentsInChildren<Renderer>())
                    {
                        lod2Renderers.Add(renderer);
                    }
                }
            }

            newOrigin.scale.x = newOrigin.scale.z *= 0.7f; // Shrink the diameter proportionally

            // Create at least two branches, otherwise the trunk will look stupid
            newOrigin.scale.y = origin.scale.y * Random.Range(0.5f, 0.8f); // Vary length of branch
            CreateSection(newOrigin, tier + 1, 1);
            newOrigin.scale.y = origin.scale.y * Random.Range(0.5f, 0.8f);
            CreateSection(newOrigin, tier + 1, -1);

            if (tier == 0)
            {
                for (int branches = 0; branches < 4; ++branches)
                {
                    if (Random.Range(0.0f, 1.0f) >= 0.5)
                    {
                        newOrigin.scale.y = origin.scale.y * Random.Range(0.5f, 0.8f);
                        CreateSection(newOrigin, tier + 1, ODD_EVEN[Random.Range(0, 1)]);
                    }
                }
            }

            // Add extra branches in the middle of one straight section sometimes
            if (tier > 0 || (tier == 0 && probabilityToBranchOnTrunk > 0.0f))
            {
                float probabilityToBranch = tier > 0 ? probabilityToBranchInMidsection : probabilityToBranchOnTrunk;
                Vector3 branchMidpoint = origin.position + ((newOrigin.position - origin.position) / 2);
                SimpleTransform midpoint = new SimpleTransform(branchMidpoint, origin.scale, origin.rotation);
                midpoint.scale.y *= 0.5f;
                if (Random.Range(0f, 1.0f) >= 1 - probabilityToBranch)
                {
                    CreateSection(midpoint, tier + 1, 1);
                }
                if (Random.Range(0f, 1.0f) >= 1 - probabilityToBranch)
                {
                    CreateSection(midpoint, tier + 1, -1);
                }
            }
        }
    }
}