using UnityEngine;

namespace Metaverse.Interactions.BallPool
{
    public class SmallChocolate : MonoBehaviour
    {
        [SerializeField] private Material[] randomMaterials;

        private Renderer rend;

        private void Awake()
        {
            rend = GetComponent<Renderer>();
        }
        
        private void Start()
        {
            ApplyRandomMaterial();
        }

        private void ApplyRandomMaterial()
        {
            if (randomMaterials == null || randomMaterials.Length == 0) return;

            var index = Random.Range(0, randomMaterials.Length);
            rend.material = randomMaterials[index];
        }
    }
}