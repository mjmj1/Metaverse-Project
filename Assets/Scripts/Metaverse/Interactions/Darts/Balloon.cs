using UnityEngine;
using DG.Tweening;

namespace Metaverse.Interactions.Darts
{
    public class Balloon : MonoBehaviour
    {
        [SerializeField] private float shrinkDuration = 0.2f;
        [SerializeField] private GameObject popEffectPrefab;

        private bool isPopped = false;

        private void OnTriggerEnter(Collider collision)
        {
            if (isPopped) return;

            if (collision.CompareTag("Dart"))
            {
                Pop();
            }
        }

        public void Pop()
        {
            isPopped = true;

            transform.DOScale(Vector3.zero, shrinkDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}