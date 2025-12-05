using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Metaverse.Interactions.SlotMachine
{
    public class SlotMachine : MonoBehaviour
    {
        [Header("레버 설정")] [SerializeField] private HingeJoint hinge;

        [SerializeField] private float triggerAngle = -40f;
        [SerializeField] private float resetAngle = -5f;
        [SerializeField] private float returnThreshold = -10f;

        [SerializeField] private AudioClip slotMachineClip;
        [SerializeField] private AudioClip slotMachineRollClip;

        [SerializeField] private Transform[] reels;

        [Header("Settings")] public float spinTime = 1.5f; // 전체 회전 시간

        public float delayBetweenStops = 0.3f; // 릴 멈추는 시간 간격
        public Ease decelerateEase = Ease.OutCubic;

        [Header("초콜릿 설정")] [SerializeField] private int spawnCount = 50;

        [FormerlySerializedAs("chocolatePrefab")] [SerializeField]
        private SlotMachineChocolate slotMachineChocolatePrefab;

        [SerializeField] private Transform burstPosition;

        [Header("폭발 설정")] [SerializeField] private float burstForce = 40f;

        [SerializeField] private float spreadAngle = 3f;
        private readonly List<SlotMachineChocolate> activeChocolates = new();

        private readonly WaitForSeconds burstInterval = new(0.03f);

        private ObjectPool<SlotMachineChocolate> chocolatePool;

        private bool hasTriggered;
        private bool isBursting;
        private bool isReturning;
        private Sequence spinSequence;

        private void Awake()
        {
            chocolatePool = new ObjectPool<SlotMachineChocolate>(
                () => Instantiate(slotMachineChocolatePrefab, burstPosition),
                go => go.gameObject.SetActive(true),
                go => go.gameObject.SetActive(false),
                Destroy,
                defaultCapacity: spawnCount);
        }

        private void Update()
        {
            var angle = hinge.angle;

            // ★ 1) 아직 트리거가 안 됐고, 일정 각도를 넘어서면 한 번 발동
            if (!hasTriggered && angle <= triggerAngle)
            {
                hasTriggered = true;
                isReturning = false;
                TriggerSlotMachine();
            }

            // ★ 2) 트리거된 상태 → 원래 위치로 반환 중인지 체크
            if (hasTriggered && !isReturning && angle > triggerAngle)
                // 레버가 올라가기 시작했음
                isReturning = true;

            // ★ 3) 충분히 올라왔을 때 reset
            if (isReturning && angle >= resetAngle)
            {
                hasTriggered = false;
                isReturning = false;
            }
        }

        [ContextMenu("Trigger Slot Machine")]
        private void TriggerSlotMachine()
        {
            if (isBursting) return;

            // Play();

            AudioManager.Instance.PlaySfx(slotMachineClip, transform.position);
            AudioManager.Instance.PlaySfx(slotMachineRollClip, transform.position);

            foreach (var chocolate in activeChocolates) chocolatePool.Release(chocolate);
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

        public void Play()
        {
            // 실행 중이던 트윈 모두 종료
            if (spinSequence != null && spinSequence.IsActive())
                spinSequence.Kill();

            spinSequence = DOTween.Sequence();

            // 1) 모든 릴을 빠르게 무한 회전시키기
            foreach (var reel in reels)
            {
                spinSequence.Join(
                    reel.DOLocalRotate(
                            new Vector3(-360f, 0, 0),
                            0.4f,
                            RotateMode.FastBeyond360
                        )
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Incremental)
                );
            }

            // 2) 일정 시간 회전 유지
            spinSequence.AppendInterval(spinTime);

            // 3) 릴을 순차적으로 감속시키면서 자연스럽게 멈추게 만들기
            for (int i = 0; i < reels.Length; i++)
            {
                int index = i;

                spinSequence.AppendCallback(() =>
                {
                    Transform r = reels[index];

                    // 현재 무한 회전 중단
                    r.DOKill();

                    // 멈출 X축 최종 각도 (예: 0도로 정렬)
                    float currentX = r.localEulerAngles.x;
                    float targetX = Mathf.Round(currentX / 90f) * 90f; // 슬롯 정렬(90 단위 예시)

                    // 감속 후 멈추는 모션
                    r.DOLocalRotate(
                            new Vector3(targetX, 0, 0),
                            0.5f
                        )
                        .SetEase(decelerateEase);
                });

                spinSequence.AppendInterval(delayBetweenStops);
            }
        }

    }
}