using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hammer : MonoBehaviour
    {
        [Header("Haptic Settings")]
        [SerializeField] [Range(0f, 1f)] private float swingHapticStrength = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float missHapticStrength = 0.5f;
        [SerializeField] private float swingVelocityThreshold = 1.5f;

        private Rigidbody rb;
        private Vector3 lastPosition;
        private float currentVelocity;
        private bool isSwinging;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            CalculateVelocity();
            CheckSwing();
        }

        private void CalculateVelocity()
        {
            currentVelocity = (transform.position - lastPosition).magnitude / Time.deltaTime;
            lastPosition = transform.position;
        }

        private void CheckSwing()
        {
            if (currentVelocity > swingVelocityThreshold && !isSwinging)
            {
                isSwinging = true;
                OnSwing();
            }
            else if (currentVelocity < swingVelocityThreshold / 2f)
            {
                isSwinging = false;
            }
        }

        private void OnSwing()
        {
            // 망치 휘두르기 사운드
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayHammerSwing();
            }

            // 가벼운 햅틱 피드백
            OVRInput.SetControllerVibration(1f, swingHapticStrength, OVRInput.Controller.RTouch);
            Invoke(nameof(StopSwingHaptic), 0.05f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Mole"))
            {
                // 두더지를 맞췄을 때는 Mole.cs에서 햅틱 처리
                // 여기서는 아무것도 하지 않음
            }
            else
            {
                // 빗나간 경우 (바닥, 벽 등)
                OnMiss();
            }
        }

        private void OnMiss()
        {
            // 빗나간 사운드
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayMoleMiss();
            }

            // 중간 강도 햅틱 피드백
            OVRInput.SetControllerVibration(1f, missHapticStrength, OVRInput.Controller.RTouch);
            Invoke(nameof(StopMissHaptic), 0.1f);
        }

        private void StopSwingHaptic()
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        }

        private void StopMissHaptic()
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        }

        private void OnDestroy()
        {
            // 안전하게 햅틱 중지
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        }
    }
}
