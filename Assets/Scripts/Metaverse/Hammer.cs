using Metaverse.Interactions.BallPool;
using UnityEngine;

namespace Metaverse
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hammer : MonoBehaviour
    {
        [Header("속도 기반 파워 설정")]
        [SerializeField] private float minSpeed = 0.5f;

        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float maxForce = 10f;

        public float CurrentPower { get; private set; }

        private Vector3 lastPosition;

        private void Update()
        {
            UpdatePower();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out HittableChocolate chocolate))
            {
                if (CurrentPower <= 0.2f) return;

                var hitDir = (collision.transform.position - transform.position).normalized;
                var force = CurrentPower * maxForce;
                chocolate.Burst(hitDir, force);
            }
        }
        private void UpdatePower()
        {
            var velocity = (transform.position - lastPosition) / Time.deltaTime;
            var speed = velocity.magnitude;

            if (speed < minSpeed)
                CurrentPower = 0f;
            else
                CurrentPower = Mathf.Clamp01((speed - minSpeed) / (maxSpeed - minSpeed));

            lastPosition = transform.position;
        }
    }
}