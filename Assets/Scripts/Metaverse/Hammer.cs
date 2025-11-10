using System.Linq;
using Metaverse.Interactions;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Metaverse
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hammer : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private HandGrabInteractable interactable;

        [Header("정렬 오프셋")]
        [SerializeField] private Vector3 rotationOffset = new(0, 0, 90);
        [SerializeField] private Vector3 positionOffset = new(0, -0.05f, 0);

        [Header("속도 기반 파워 설정")]
        [SerializeField] private float minSpeed = 0.5f;

        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float maxForce = 10f;

        private Vector3 lastPosition;

        private Transform originalParent;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Rigidbody rb;

        public float CurrentPower { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            originalParent = transform.parent;
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;

            lastPosition = transform.position;
        }

        private void Update()
        {
            UpdatePower();
        }

        private void OnEnable()
        {
            interactable.WhenStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            interactable.WhenStateChanged -= OnStateChanged;
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

        private void OnStateChanged(InteractableStateChangeArgs args)
        {
            if (args.NewState == InteractableState.Select)
            {
                var view = interactable.SelectingInteractorViews.FirstOrDefault();
                if (view is Component comp) AlignToHand(comp.transform);
            }
            else if (args.NewState == InteractableState.Normal ||
                     args.NewState == InteractableState.Hover)
            {
                ReleaseHand();
            }
        }

        private void AlignToHand(Transform hand)
        {
            transform.SetParent(hand, false);
            transform.localPosition = positionOffset;
            transform.localRotation = Quaternion.Euler(rotationOffset);
        }

        private void ReleaseHand()
        {
            transform.SetParent(originalParent, false);
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
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