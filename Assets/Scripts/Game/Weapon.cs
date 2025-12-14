using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody))]
    public class Weapon : MonoBehaviour
    {
        [Header("Sound Effects")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip attackClip;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Mole"))
            {
                AudioManager.Instance.Play3DSfx(audioSource, attackClip);
            }
        }
    }
}
