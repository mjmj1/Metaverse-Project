using UnityEngine;

namespace Metaverse.Interactions.SlotMachine
{
    public class SlotMachineChocolate : MonoBehaviour
    {
        
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private Vector3 GetFanDirection(Transform origin, float spreadAngleDeg)
        {
            var halfAngle = spreadAngleDeg * 0.5f;

            var angle = Random.Range(-halfAngle, halfAngle);

            var rotation = Quaternion.AngleAxis(angle, origin.up);

            return rotation * origin.forward;
        }

        public void Burst(Transform parent, float spreadAngle, float burstForce)
        {
            var direction = GetFanDirection(parent, spreadAngle);
            rb.AddForce(direction * burstForce, ForceMode.Impulse);

            rb.AddForce(Random.insideUnitSphere * 1.5f, ForceMode.Impulse);
        }
    }
}