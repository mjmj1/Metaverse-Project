using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Mole
{
    public class MoleAnimation : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform body; // 움직일 대상

        [Header("Position Settings")]
        [SerializeField] private float startY = -0.5f;
        [SerializeField] private float endY = 0.5f;

        [Header("Animation Settings")]
        [SerializeField] private float riseDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.3f;
        [SerializeField] private float hitDuration = 0.15f;

        public void InitPosition()
        {
            body.DOKill();
            var pos = body.localPosition;
            pos.y = startY;
            body.localPosition = pos;
        }

        // 전조 연출 (예: 흔들기)
        public void PlayWarning(float duration, Action onComplete)
        {
            // 예: 0.1의 강도로 duration 동안 위치/회전 흔들기
            body.DOShakePosition(duration, 0.1f)
                .OnComplete(() => onComplete?.Invoke());
        }

        // 상승 연출
        public void PlayRise(Action onComplete)
        {
            body.DOLocalMoveY(endY, riseDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        // 하강 연출 (자진 복귀)
        public void PlayHide(Action onComplete)
        {
            body.DOLocalMoveY(startY, hideDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        // 피격 연출 (빠른 하강 + 찌그러짐 등)
        public void PlayHit(Action onComplete)
        {
            body.DOKill(); // 진행 중인 상승/하강 중단

            // 시퀀스로 묶어서 연출 (찌그러짐 + 하강 동시에)
            var seq = DOTween.Sequence();

            // 1. 살짝 눌림 (PunchScale)
            seq.Join(body.DOPunchScale(new Vector3(0.2f, -0.2f, 0.2f), 0.1f));

            // 2. 아래로 꺼짐
            seq.Join(body.DOLocalMoveY(startY, hitDuration).SetEase(Ease.OutFlash));

            seq.OnComplete(() => onComplete?.Invoke());
        }
    }
}