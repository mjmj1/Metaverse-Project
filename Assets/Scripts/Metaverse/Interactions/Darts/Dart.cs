using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Metaverse.Interactions.Darts
{
    public class Dart : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private HandGrabInteractable grab;
        [SerializeField] private TrailRenderer trail;

        [Header("Throw Settings")]
        [SerializeField] private float velocityMultiplier = 1.0f;
        [SerializeField] private float maxVelocity = 10f;

        private Rigidbody rb;
        private bool isGrabbed;

        private Vector3 lastPosition;
        private Vector3 throwVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            grab.WhenStateChanged += WhenStateChanged;

            Init();
        }

        private void OnDestroy()
        {
            grab.WhenStateChanged -= WhenStateChanged;
        }

        public void Init()
        {
            rb.isKinematic = true;
            isGrabbed = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY;
        }

        private void WhenStateChanged(InteractableStateChangeArgs args)
        {
            switch (args.NewState)
            {
                case InteractableState.Normal:
                    OnNormal();
                    break;
                case InteractableState.Select:
                    OnSelect();
                    break;
            }
        }

        private void OnSelect()
        {
            Debug.Log("🎯 Dart Grabbed (Select)");

            rb.isKinematic = false;
            isGrabbed = true;

            trail.Clear();
            trail.enabled = false;

            throwVelocity = (transform.position - lastPosition) / Time.deltaTime;
            var finalVelocity = throwVelocity * velocityMultiplier;

            if (finalVelocity.magnitude > maxVelocity)
            {
                finalVelocity = finalVelocity.normalized * maxVelocity;
            }

            rb.linearVelocity = finalVelocity;

            // 방향 정렬
            if (finalVelocity.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(finalVelocity.normalized, Vector3.up);
            }
        }

        private void OnNormal()
        {
            isGrabbed = true;
            trail.enabled = true;
        }

        private void Update()
        {
            if (isGrabbed)
            {
                var currentPos = transform.position;
                throwVelocity = (currentPos - lastPosition) / Time.deltaTime;
                lastPosition = currentPos;
            }
        }
    }
}
