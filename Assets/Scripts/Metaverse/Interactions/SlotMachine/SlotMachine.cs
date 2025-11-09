using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Metaverse.Interactions.SlotMachine
{
    public class SlotMachine : MonoBehaviour
    {
        [SerializeField] private int spawnCount = 50;
        [SerializeField] private Chocolate chocolatePrefab;
        [SerializeField] private Transform burstPosition;

        private readonly WaitForSeconds burstTick = new WaitForSeconds(0.03f);
        private ObjectPool<Chocolate> chocolatePool;

        private bool isBursting;

        void Awake()
        {
            chocolatePool = new ObjectPool<Chocolate>(
                createFunc: () => Instantiate(chocolatePrefab, burstPosition),
                actionOnGet: go => go.gameObject.SetActive(true),
                actionOnRelease: go => go.gameObject.SetActive(false),
                actionOnDestroy: Destroy,
                defaultCapacity: spawnCount);
        }

        void Start()
        {
            // StartCoroutine(BurstCo());
        }

        [ContextMenu("Burst")]
        public void Burst()
        {
            if (isBursting) return;

            isBursting = true;

            StartCoroutine(BurstCo());
        }


        private IEnumerator BurstCo()
        {
            for (var i = 0; i < spawnCount; i++)
            {
                var obj = chocolatePool.Get();
                obj.transform.position = burstPosition.position;

                obj.transform.rotation = Random.rotation;
                obj.Burst(burstPosition, 40f, 3f);

                yield return burstTick;
            }
        }
    }
}
