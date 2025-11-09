using System.Collections;
using CurvedPathGenerator;
using UnityEngine;

namespace Metaverse.Interactions.Rails
{
    public class Rails : MonoBehaviour
    {
        [SerializeField] private PathFollower followerPrefab;
        [SerializeField] private PathGenerator pathGenerator;
        [SerializeField] private int itemCount = 10;
        [SerializeField] private float spawnInterval = 1f;

        void Start()
        {
            StartCoroutine(ItemSpawnRoutine());
        }

        private IEnumerator ItemSpawnRoutine()
        {
            for (var i = 0; i < itemCount; i++)
            {
                var item = Instantiate(followerPrefab, transform);

                item.Generator = pathGenerator;

                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}
