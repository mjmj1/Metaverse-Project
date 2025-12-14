using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Mole
{
    public class MoleAnimation : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform body; // 움직일 대상

        [Header("Animation Settings")]
        [SerializeField] private float riseDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.3f;
        [SerializeField] private float hitDuration = 0.2f;

        public void InitPosition()
        {
            body.DOKill();
            // 크기를 0으로 만들어 숨김
            body.localScale = Vector3.zero;
            // 위치는 항상 0,0,0 (구멍 중앙)
            body.localPosition = Vector3.zero;
        }

        // 전조 연출 (종이가 바스락거리는 느낌으로 회전/스케일 흔들기)
        public void PlayWarning(float duration, Action onComplete)
        {
            // 회전을 흔들어서 종이가 바람에 떨리는 느낌 구현
            body.localScale = Vector3.one * 0.1f; // 아주 작게 시작
            body.DOShakeRotation(duration, 30f, 10, 90f)
                .OnComplete(() => onComplete?.Invoke());
        }

        // 상승 연출 (펑! 하고 펴지기)
        public void PlayRise(Action onComplete)
        {
            // Scale 0 -> 1 (Elastic으로 띠용~ 하는 느낌)
            body.DOScale(Vector3.one, riseDuration)
                .SetEase(Ease.OutElastic)
                .OnComplete(() => onComplete?.Invoke());
        }

        // 하강 연출 (다시 접히며 사라짐)
        public void PlayHide(Action onComplete)
        {
            // Scale 1 -> 0 (Back으로 쏙 들어가는 느낌)
            body.DOScale(Vector3.zero, hideDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        // 피격 연출
        public void PlayHit(Action onComplete)
        {
            body.DOKill();

            var seq = DOTween.Sequence();

            // 1. [회전] X축(정면)을 축으로 회전해야 좌우로 기우뚱함
            // 랜덤하게 -45도(왼쪽 기울기) or 45도(오른쪽 기울기)
            var cutAngle = Random.Range(0, 2) == 0 ? -45f : 45f;

            // X축 회전 적용 (모델의 정면이 X축이므로)
            body.localRotation = Quaternion.Euler(cutAngle, 0, 0);

            seq.Join(body.DOScale(new Vector3(1f, 1.2f, 0.1f), hitDuration).SetEase(Ease.OutExpo));

            var slideDir = cutAngle > 0 ? 0.5f : -0.5f; // 각도가 양수면 오른쪽(Z+), 음수면 왼쪽(Z-)

            // Z축으로 미끄러지면서 Y축으로는 바닥으로 꺼짐
            seq.Join(body.DOLocalMove(new Vector3(0, slideDir, -0.5f), hitDuration)
                .SetRelative()
                .SetEase(Ease.OutQuad));

            // 4. 종료
            seq.OnComplete(() =>
            {
                body.localScale = Vector3.zero; // 확실하게 숨김
                onComplete?.Invoke();
            });
        }
    }
}