using System.Collections;
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
        [SerializeField] GameObject sectionPrefab;
        [SerializeField] GameObject[] foliagePrefab;

        [Header("Settings")]
        public Vector3 position = Vector3.zero;
        public int height = 5;
        [Range(3, 6)]
        public int maxTiers = 5;
        [Range(0.0f, 1.0f)]
        [SerializeField] float probabilityToBranchInMidsection = 0.5f;
        [Range(0.0f, 1.0f)]
        [SerializeField] float probabilityToBranchOnTrunk = 0.0f;
        [SerializeField] bool showFoliage = true;

        private int[] ODD_EVEN = { -1, 1 };

        void Start()
        {
            CreateSection(new SimpleTransform(position, Vector3.one, Quaternion.identity), 0);
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

            GameObject firstSection = Instantiate(sectionPrefab, origin.position, newOrigin.rotation);
            firstSection.transform.localScale = new Vector3(1, height, 1) * origin.scale.y * 0.34f;
            GameObject section2 = Instantiate(sectionPrefab, origin.position + ((newOrigin.position - origin.position) / 3.0f), newOrigin.rotation);
            section2.transform.localScale = new Vector3(firstSection.transform.localScale.x * 0.9f, firstSection.transform.localScale.y, firstSection.transform.localScale.z * 0.9f);
            GameObject section3 = Instantiate(sectionPrefab, origin.position + ((newOrigin.position - origin.position) * (2.0f / 3)), newOrigin.rotation);
            section3.transform.localScale = new Vector3(firstSection.transform.localScale.x * 0.8f, firstSection.transform.localScale.y, firstSection.transform.localScale.z * 0.8f);

            Debug.DrawLine(origin.position, newOrigin.position, Color.green, 60);

            if (tier + 1 == maxTiers && showFoliage)
            {
                GameObject foliage = Instantiate(foliagePrefab[Random.Range(0, foliagePrefab.Length)], newOrigin.position, Quaternion.Euler(0, Random.Range(0.0f, 360), 0));
                foliage.transform.localScale *= Random.Range(3, 8);
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