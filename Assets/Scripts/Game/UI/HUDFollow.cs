using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class HUDFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        public float distance = 1.0f;
        public float heightOffset = 0.0f;
        public float smoothSpeed = 5f;

        void LateUpdate()
        {
            if (!target) return;

            var targetPos = target.position + (target.forward * distance) + (target.up * heightOffset);
            var targetRot = Quaternion.LookRotation(transform.position - target.position);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }
}
