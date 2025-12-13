using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Mole
{
    public enum MoleState
    {
        None,
        Warning,// 전조
        Rising, // 올라오는 중
        Idle,   // 대기 중
        Hiding, // 숨는 중
        Hit,    // 맞음
    }

    public class Mole : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private float risingTime;
        [SerializeField] private float idleTime;
        [SerializeField] private float hidingTime;

        private Action<Mole> returnToPoolCallback;

        public event UnityAction OnWarning;
        public event UnityAction OnRising;
        public event UnityAction OnIdle;
        public event UnityAction OnHiding;
        public event UnityAction OnHit;

        private MoleAnimation moleAnim;
        private Coroutine routine;

        private MoleState previousState;
        private MoleState currentState = MoleState.None;

        public MoleState MoleState
        {
            get => currentState;
            set
            {
                previousState = currentState;
                currentState = value;
                OnStateChanged(previousState, currentState);
            }
        }

        private void OnStateChanged(MoleState prevValue, MoleState newValue)
        {
            switch (newValue)
            {
                case MoleState.Warning:
                    OnWarning?.Invoke();
                    break;
                case MoleState.Rising:
                    OnRising?.Invoke();
                    break;
                case MoleState.Idle:
                    OnIdle?.Invoke();
                    break;
                case MoleState.Hiding:
                    OnHiding?.Invoke();
                    break;
                case MoleState.Hit:
                    OnHit?.Invoke();
                    break;
            }
        }

        private void Awake()
        {
            moleAnim = GetComponent<MoleAnimation>();

            Hide();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (MoleState is MoleState.None or MoleState.Warning) return;
            if (!other.CompareTag("Hammer")) return;

            Hit();
        }

        public void Hide()
        {
            if (routine != null) StopCoroutine(routine);
            moleAnim.InitPosition();
            MoleState = MoleState.None;
        }

        public void Pop(float warningDuration, Action<Mole> onFinished)
        {
            returnToPoolCallback = onFinished;

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(PopRoutine(warningDuration));
        }

        private void FinishCycle()
        {
            MoleState = MoleState.None;
            returnToPoolCallback?.Invoke(this);
        }

        private IEnumerator PopRoutine(float warningTime)
        {
            // 1. 전조
            MoleState = MoleState.Warning;
            var animFinished = false;
            // 애니메이션 실행 & 끝날 때까지 대기
            moleAnim.PlayWarning(warningTime, () => animFinished = true);
            yield return new WaitUntil(() => animFinished);

            // 2. 상승
            MoleState = MoleState.Rising;
            animFinished = false;
            moleAnim.PlayRise(() => animFinished = true);
            yield return new WaitUntil(() => animFinished);

            // 3. 대기 (로직상의 대기이므로 여기서 시간 셈)
            MoleState = MoleState.Idle;
            yield return new WaitForSeconds(idleTime);

            // 4. 하강
            MoleState = MoleState.Hiding;
            animFinished = false;
            moleAnim.PlayHide(() => animFinished = true);
            yield return new WaitUntil(() => animFinished);

            FinishCycle();
        }

        private void Hit()
        {
            if (currentState == MoleState.Hit) return;

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(HitDown());
        }

        private IEnumerator HitDown()
        {
            MoleState = MoleState.Hit;

            // 콤보 시스템을 통한 점수 추가
            if (GameManager.Instance?.ScoreManager != null)
            {
                GameManager.Instance.ScoreManager.AddMoleHit();
            }
            else
            {
                // ScoreManager가 없으면 기존 방식 사용
                GameManager.Instance?.AddScore(1);
            }

            var animFinished = false;
            moleAnim.PlayHit(() => animFinished = true);
            yield return new WaitUntil(() => animFinished);

            FinishCycle();
        }
    }
}