using UnityEngine;

namespace Game.Mole
{
    public class MoleMaterial : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material[] variantMaterials;

        private void Awake()
        {
            if (!targetRenderer)
                targetRenderer = GetComponentInChildren<Renderer>(true);

            if (!defaultMaterial && targetRenderer)
                defaultMaterial = targetRenderer.sharedMaterial;
        }

        public void Apply(int index = -1)
        {
            if (!targetRenderer || variantMaterials == null || variantMaterials.Length == 0)
                return;

            if (index < 0)
                index = Random.Range(0, variantMaterials.Length);

            index = Mathf.Clamp(index, 0, variantMaterials.Length - 1);
            targetRenderer.material = variantMaterials[index];

            print("applying material " + index);
        }

        public void Apply(Material material)
        {
            if (!targetRenderer || !material) return;
            targetRenderer.material = material;
        }

        public void ResetToDefault()
        {
            if (!targetRenderer || !defaultMaterial) return;
            targetRenderer.material = defaultMaterial;
        }
    }
}