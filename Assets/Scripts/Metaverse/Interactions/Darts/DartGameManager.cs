using System.Collections.Generic;
using UnityEngine;

namespace Metaverse.Interactions.Darts
{
    public class DartGameManager : MonoBehaviour
    {
        [Header("Dart")] [SerializeField] private DartHolder dartHolder;

        [Header("Balloon")] [SerializeField] private GameObject balloonPrefab;

        [SerializeField] private BoxCollider spawnArea;
        [SerializeField] private Material[] balloonMaterials;
        [SerializeField] private int balloonCount = 5; // 풍선 개수 제한

        private readonly List<GameObject> spawnedBalloons = new();

        private void Start()
        {
            ResetDart();
        }

        public void ResetDart()
        {
            dartHolder.Init();
            ResetBalloons();
            SpawnBalloons();
        }

        public void SpawnBalloons()
        {
            if (!balloonPrefab || !spawnArea || balloonMaterials.Length == 0) return;

            for (var i = 0; i < balloonCount; i++)
            {
                var spawnPosition = GetRandomPointInBox(spawnArea);
                var spawnRotation = Quaternion.Euler(0, 180f, 0);
                var balloon = Instantiate(balloonPrefab, spawnPosition, spawnRotation);
                spawnedBalloons.Add(balloon);

                // 랜덤 머티리얼
                var rend = balloon.GetComponentInChildren<MeshRenderer>();
                if (rend != null)
                {
                    var mat = balloonMaterials[Random.Range(0, balloonMaterials.Length)];
                    rend.material = mat;
                }
            }
        }

        public void ResetBalloons()
        {
            foreach (var balloon in spawnedBalloons) if (balloon != null) Destroy(balloon);

            spawnedBalloons.Clear();
        }

        private Vector3 GetRandomPointInBox(BoxCollider box)
        {
            var center = box.center + box.transform.position;
            var size = box.size;

            return center + new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                Random.Range(-size.y / 2f, size.y / 2f),
                Random.Range(-size.z / 2f, size.z / 2f)
            );
        }
    }
}