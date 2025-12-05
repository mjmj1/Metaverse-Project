using UnityEngine;

namespace Metaverse.Interactions.BallPool
{
    [RequireComponent(typeof(Rigidbody))]
    public class HittableChocolate : MonoBehaviour
    {
        [SerializeField] private Transform manager;
        [Header("파편 설정")]
        [SerializeField] private GameObject smallChocolatePrefab;
        [SerializeField] private AudioClip chocolateBreakClip;
        [SerializeField] private int shardCount = 8;
        [SerializeField] private float spreadForce = 5f;
        [SerializeField] private float spreadAngle = 120f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        ///     망치에 맞았을 때 날아감 + 터짐 처리
        /// </summary>
        public void Burst(Vector3 direction, float force)
        {
            AudioManager.Instance.PlaySfx(chocolateBreakClip, transform.position);

            // 기존 초콜릿 본체 튕겨나감
            rb.AddForce(direction * force, ForceMode.Impulse);

            // 파괴 후 터짐 효과
            Explode();

            // 원본 제거 (잠시 후 제거하면 터짐 효과 후에 사라짐)
            Destroy(gameObject, 0.05f);
        }

        /// <summary>
        ///     작은 초콜릿들을 생성해 사방으로 퍼뜨림
        /// </summary>
        private void Explode()
        {
            if (smallChocolatePrefab == null) return;

            for (var i = 0; i < shardCount; i++)
            {
                var shard = Instantiate(smallChocolatePrefab, transform.position, Random.rotation, manager);
                var shardRb = shard.GetComponent<Rigidbody>();

                if (shardRb != null)
                {
                    var dir = GetRandomSpreadDirection();
                    var magnitude = Random.Range(spreadForce * 0.6f, spreadForce);
                    shardRb.AddForce(dir * magnitude, ForceMode.Impulse);
                }
            }
        }

        /// <summary>
        /// 퍼짐 방향을 랜덤한 반구 형태로 반환
        /// </summary>
        private Vector3 GetRandomSpreadDirection()
        {
            var halfAngle = spreadAngle * 0.5f;
            var angleY = Random.Range(-halfAngle, halfAngle);
            var angleX = Random.Range(-halfAngle, halfAngle);

            var spreadRot = Quaternion.Euler(angleX, angleY, 0);
            return spreadRot * Vector3.forward; // Z축 기준 퍼짐
        }
    }
}