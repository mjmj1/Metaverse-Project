using System.Collections;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody))]
    public class Weapon : MonoBehaviour
    {
        [Header("Sound Effects")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip attackClip;

        private WaitForSeconds wait = new (0.1f);
        private bool isAttacked;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Mole"))
            {
                StartCoroutine(PlayOnceSfx());
            }
        }

        private IEnumerator PlayOnceSfx()
        {
            if(isAttacked) yield break;

            isAttacked = true;
            AudioManager.Instance.Play3DSfx(audioSource, attackClip);

            yield return wait;
            isAttacked = false;
        }
    }
}
