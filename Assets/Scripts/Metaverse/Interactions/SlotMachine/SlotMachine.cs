using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Metaverse.Interactions.SlotMachine
{
    public class SlotMachine : MonoBehaviour
    {
        [Header("레버 설정")]
        [SerializeField] private HingeJoint hinge;
        [SerializeField] private float triggerAngle = -40f;
        [SerializeField] private float resetAngle = -10f;

        [Header("초콜릿 설정")]
        [SerializeField] private int spawnCount = 50;
        [FormerlySerializedAs("chocolatePrefab")] [SerializeField] private SlotMachineChocolate slotMachineChocolatePrefab;
        [SerializeField] private Transform burstPosition;

        [Header("폭발 설정")]
        [SerializeField] private float burstForce = 40f;
        [SerializeField] private float spreadAngle = 3f;

        private readonly WaitForSeconds burstInterval = new (0.03f);

        private ObjectPool<SlotMachineChocolate> chocolatePool;
        private readonly List<SlotMachineChocolate> activeChocolates = new();

        private bool hasTriggered;
        private bool isBursting;

        void Awake()
        {
            chocolatePool = new ObjectPool<SlotMachineChocolate>(
                createFunc: () => Instantiate(slotMachineChocolatePrefab, burstPosition),
                actionOnGet: go => go.gameObject.SetActive(true),
                actionOnRelease: go => go.gameObject.SetActive(false),
                actionOnDestroy: Destroy,
                defaultCapacity: spawnCount);
        }

        private void Update()
        {
            var angle = hinge.angle;

            if (!hasTriggered && angle <= triggerAngle)
            {
                hasTriggered = true;
                TriggerSlotMachine();
            }

            if (hasTriggered && angle >= resetAngle)
            {
                hasTriggered = false;
            }
        }

        [ContextMenu("Trigger Slot Machine")]
        private void TriggerSlotMachine()
        {
            if (isBursting) return;

            foreach (var chocolate in activeChocolates)
            {
                chocolatePool.Release(chocolate);
            }
            activeChocolates.Clear();

            StartCoroutine(BurstCoroutine());
        }

        private IEnumerator BurstCoroutine()
        {
            isBursting = true;

            for (var i = 0; i < spawnCount; i++)
            {
                var chocolate = chocolatePool.Get();
                chocolate.transform.position = burstPosition.position;
                chocolate.transform.rotation = Random.rotation;

                chocolate.Burst(burstPosition, spreadAngle, burstForce);
                activeChocolates.Add(chocolate);

                yield return burstInterval;
            }

            isBursting = false;
        }
    }
}
