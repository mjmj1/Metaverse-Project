using System;
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

        private Rigidbody rb;
        private bool isGrabbed;

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
        }

        private void OnNormal()
        {
            isGrabbed = true;
            trail.enabled = true;
        }
    }
}
