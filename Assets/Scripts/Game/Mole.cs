using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    public enum MoleState
    {
        Hidden,
        Rising,
        Idle,
        Falling,
        Hit
    }

    public class Mole : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Vector3 hidePosition = new(0f, -1f, 0f);

        [Header("Motion")] [SerializeField] private float popHeight = 1f; // 위로 나오는 높이

        [SerializeField] private float riseTime = 0.22f; // 상승 시간
        [SerializeField] private float idleTime = 0.8f; // 밖에 나와있는 시간
        [SerializeField] private float fallTime = 0.18f; // 하강 시간
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 baseLocalPos; // 숨은 기준 위치(로컬)
        private Action<Mole> onFinished; // 매니저에게 알림
        private Coroutine routine;

        public MoleState State { get; private set; } = MoleState.Hidden;

        private void Awake()
        {
            HideInstant();
            baseLocalPos = body.localPosition;
        }

        // 망치에 맞았을 때
        private void OnTriggerEnter(Collider other)
        {
            if (State is MoleState.Hidden or MoleState.Falling) return;
            if (!other.CompareTag("Hammer")) return;

            State = MoleState.Hit;

            // 즉시 아래로 “쏙” 들어가는 연출
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(HitDown());
        }

        public void Play(Action<Mole> onEnd)
        {
            if (routine != null) StopCoroutine(routine);
            onFinished = onEnd;
            routine = StartCoroutine(PopRoutine());
        }

        [ContextMenu("Play Pop")]
        public void TestPlay()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(PopRoutine());
        }

        private void HideInstant()
        {
            body.localPosition = hidePosition;
            State = MoleState.Hidden;
        }

        private IEnumerator PopRoutine()
        {
            State = MoleState.Rising;
            yield return LerpHeight(riseTime, 0f, popHeight);

            State = MoleState.Idle;
            yield return new WaitForSeconds(idleTime);

            State = MoleState.Falling;
            yield return LerpHeight(fallTime, popHeight, 0f);

            State = MoleState.Hidden;
            onFinished?.Invoke(this);
        }

        private IEnumerator LerpHeight(float t, float from, float to)
        {
            if (t <= 0f)
            {
                body.localPosition = baseLocalPos + body.transform.parent.up * to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < t)
            {
                elapsed += Time.deltaTime;
                var k = Mathf.Clamp01(elapsed / t);
                var eased = curve.Evaluate(k);
                var h = Mathf.LerpUnclamped(from, to, eased);
                body.localPosition = baseLocalPos + body.transform.parent.up * h;
                yield return null;
            }

            body.localPosition = baseLocalPos + body.transform.parent.up * to;
        }

        private IEnumerator HitDown()
        {
            yield return LerpHeight(0.1f, popHeight, 0f);
            State = MoleState.Hidden;
            onFinished?.Invoke(this);
        }
    }
}