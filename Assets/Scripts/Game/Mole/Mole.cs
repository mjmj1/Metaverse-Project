using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

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
        [Header("Effects")]
        [SerializeField] private ParticleSystem hitVfx;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip warningClip;
        [SerializeField] private AudioClip risingClip;
        [SerializeField] private AudioClip hidingClip;
        [SerializeField] private AudioClip hitClip;

        [Header("Mole Settings")]
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

        private MoleMaterial moleMaterial;

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
            moleMaterial = GetComponent<MoleMaterial>();

            Hide();
        }

        private void Start()
        {
            OnWarning += OnOnWarning;
            OnRising += OnOnRising;
            OnHiding += OnOnHiding;
            OnHit += OnOnHit;
        }


        private void OnDestroy()
        {
            OnWarning -= OnOnWarning;
            OnRising -= OnOnRising;
            OnHiding -= OnOnHiding;
            OnHit -= OnOnHit;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (MoleState is MoleState.None or MoleState.Warning) return;

            if (other.CompareTag("Hammer") || other.CompareTag("Blade"))
            {
                Hit();
            }
        }

        private void OnOnWarning()
        {
            AudioManager.Instance.Play3DSfx(audioSource, warningClip);
        }

        private void OnOnRising()
        {
            AudioManager.Instance.Play3DSfx(audioSource, risingClip);
        }

        private void OnOnHiding()
        {
            AudioManager.Instance.Play3DSfx(audioSource, hidingClip);
        }

        private void OnOnHit()
        {
            AudioManager.Instance.Play3DSfx(audioSource, hitClip);
            hitVfx.Play();
        }

        public void Hide()
        {
            if (routine != null) StopCoroutine(routine);
            moleAnim.InitPosition();
            MoleState = MoleState.None;
        }

        public void Pop(float warningDuration, Action<Mole> onFinished)
        {
            moleMaterial?.Apply();

            returnToPoolCallback = onFinished;

            var rotX = Random.Range(0, 2) == 0 ? 20f : -20f;
            body.transform.localRotation = Quaternion.Euler(rotX, -90f, 90f);

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

            OVRInput.SetControllerVibration(1f, 1f);
            yield return new WaitForSeconds(0.1f);
            OVRInput.SetControllerVibration(0f, 0f);

            GameManager.Instance.ScoreManager.AddMoleHit();

            var animFinished = false;
            moleAnim.PlayHit(() => animFinished = true);
            yield return new WaitUntil(() => animFinished);

            yield return new WaitForSeconds(0.5f);

            FinishCycle();
        }
    }
}