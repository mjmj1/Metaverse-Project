using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class MoleManager : MonoBehaviour
    {
        [SerializeField] private List<Transform> holes; // 각 홀의 Transform
        [SerializeField] private Mole molePrefab;
        [SerializeField] private int poolSize = 12;

        [Header("Spawn")]
        [SerializeField] private float spawnInterval = 0.7f; // 기본 간격
        [SerializeField] private Vector2 groupCountRange = new(1, 3); // 한번에 나오는 수

        private readonly HashSet<int> occupied = new();
        private readonly Queue<Mole> pool = new();
        private float timer;

        private void Start()
        {
            for (var i = 0; i < poolSize; i++)
            {
                var m = Instantiate(molePrefab);
                m.gameObject.SetActive(false);
                pool.Enqueue(m);
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnGroup();
                spawnInterval = Mathf.Max(0.3f, spawnInterval - 0.0008f);
            }
        }

        private void SpawnGroup()
        {
            var n = Random.Range((int)groupCountRange.x, (int)groupCountRange.y + 1);
            n = Mathf.Min(n, holes.Count - occupied.Count);

            for (var i = 0; i < n; i++)
            {
                var idx = GetRandomFreeHole();
                if (idx < 0) break;
                SpawnAt(idx);
            }
        }

        private int GetRandomFreeHole()
        {
            if (occupied.Count >= holes.Count) return -1;
            var guard = 0;
            while (guard++ < 1000)
            {
                var idx = Random.Range(0, holes.Count);
                if (!occupied.Contains(idx)) return idx;
            }

            return -1;
        }

        private void SpawnAt(int holeIndex)
        {
            if (pool.Count == 0) return;
            var m = pool.Dequeue();
            var h = holes[holeIndex];

            m.transform.SetParent(h, false);
            m.transform.localPosition = Vector3.zero;
            m.transform.localRotation = Quaternion.identity;
            m.gameObject.SetActive(true);
            occupied.Add(holeIndex);

            m.Play(onEnd: mole =>
            {
                occupied.Remove(holeIndex);
                mole.gameObject.SetActive(false);
                mole.transform.SetParent(transform, false);
                pool.Enqueue(mole);
            });
        }
    }
}