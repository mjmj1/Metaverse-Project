using System.Collections;
using System.Collections.Generic;
using System.Linq; // Dictionary Keys 복사용
using UnityEngine;

namespace Game
{
    public class MoleManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Mole.Mole molePrefab;
        [SerializeField] private int poolSize = 20;

        [Header("Dome Generation")]
        [SerializeField] private Transform playerHead;
        [SerializeField] private int holeCount = 15;
        [SerializeField] private float radius = 2.0f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 40f;

        [Header("Spawn Logic")]
        [SerializeField] private float initialSpawnInterval = 1.5f;
        [SerializeField] private Vector2Int groupCountRange = new(1, 2);

        // 내부 변수
        private readonly List<Transform> holes = new();
        private readonly Queue<Mole.Mole> pool = new();
        private readonly Dictionary<int, Mole.Mole> activeMoles = new();

        private Coroutine spawnRoutine;
        private float currentSpawnInterval;

        private void OnDrawGizmosSelected()
        {
            // 기준점 설정 (플레이어 머리 or 매니저 위치)
            var center = playerHead ? playerHead.position : transform.position;

            Gizmos.color = Color.red;

            var step = 10f; // 해상도 (낮을수록 촘촘함)
            var extraRange = GameManager.Instance ? GameManager.Instance.GetSpawnRange() : 0f;

            for (var p = minPitch - extraRange; p <= maxPitch + extraRange; p += step)
            for (var y = 0f; y < 360f; y += step)
            {
                var pos = GetDomePosition(center, p, y);
                var nextPosYaw = GetDomePosition(center, p, y + step);
                var nextPosPitch = GetDomePosition(center, p + step, y);

                Gizmos.DrawLine(pos, nextPosYaw);

                if (p + step <= maxPitch) Gizmos.DrawLine(pos, nextPosPitch);
            }

            // 3. 실제 생성된 구멍 위치 표시 (게임 실행 중일 때)
            Gizmos.color = Color.red;
            foreach (var hole in holes.Where(hole => hole))
            {
                Gizmos.DrawWireSphere(hole.position, 0.15f);
            }
        }

        private Vector3 GetDomePosition(Vector3 center, float pitchDeg, float yawDeg)
        {
            var pitch = pitchDeg * Mathf.Deg2Rad;
            var yaw = yawDeg * Mathf.Deg2Rad;

            var y = Mathf.Sin(pitch) * radius;
            var h = Mathf.Cos(pitch) * radius;
            var x = Mathf.Cos(yaw) * h;
            var z = Mathf.Sin(yaw) * h;

            return center + new Vector3(x, y, z);
        }

        public void Initialize()
        {
            GenerateHoles();

            // 풀 초기화
            foreach (var m in pool.Where(m => m)) Destroy(m.gameObject);
            pool.Clear();

            // 활성화된 두더지 초기화
            foreach (var m in activeMoles.Values.Where(m => m)) Destroy(m.gameObject);
            activeMoles.Clear();

            for (var i = 0; i < poolSize; i++)
            {
                var m = Instantiate(molePrefab, transform);
                m.gameObject.SetActive(false);
                pool.Enqueue(m);
            }
        }

        private void GenerateHoles()
        {
            // 기존 구멍 제거
            foreach (var t in holes.Where(t => t)) Destroy(t.gameObject);
            holes.Clear();
            activeMoles.Clear();

            var center = playerHead.position;
            var extraRange = GameManager.Instance.GetSpawnRange();

            for (var i = 0; i < holeCount; i++)
            {
                var yaw = Random.Range(0f, 360f);
                var pitch = Random.Range(minPitch - extraRange, maxPitch + extraRange);

                var pos = GetDomePosition(center, pitch, yaw);

                var holeObj = new GameObject($"Hole_{i}");
                holeObj.transform.SetParent(transform);
                holeObj.transform.position = pos;

                var dirToPlayer = (center - pos).normalized;
                holeObj.transform.rotation = Quaternion.LookRotation(dirToPlayer) * Quaternion.Euler(90f, 0f, 0f);

                holes.Add(holeObj.transform);
            }
        }

        public void StartGameLoop()
        {
            currentSpawnInterval = initialSpawnInterval;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(SpawnRoutine());
        }

        public void StopGameLoop()
        {
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);

            var activeIndices = activeMoles.Keys.ToList();

            foreach (var idx in activeIndices)
            {
                if (!activeMoles.TryGetValue(idx, out var mole)) continue;

                mole.Hide();
                ReturnToPool(mole, idx);
            }

            activeMoles.Clear();
        }

        private IEnumerator SpawnRoutine()
        {
            while (GameManager.Instance.GameState is GameState.Playing or GameState.Paused)
            {
                yield return new WaitForSeconds(currentSpawnInterval);
                SpawnGroup();

                var waringTime = Random.Range(0.5f, 1.5f);
                currentSpawnInterval = Mathf.Max(waringTime, currentSpawnInterval - 0.02f);
            }
        }

        private void SpawnGroup()
        {
            var count = Random.Range(groupCountRange.x, groupCountRange.y + 1);
            count = Mathf.Min(count, holes.Count - activeMoles.Count);

            for (var i = 0; i < count; i++)
            {
                if (pool.Count == 0) break;

                var holeIndex = GetRandomFreeHole();
                if (holeIndex != -1)
                {
                    SpawnMoleAt(holeIndex);
                }
            }
        }

        private int GetRandomFreeHole()
        {
            if (activeMoles.Count >= holes.Count) return -1;

            var idx = Random.Range(0, holes.Count);
            for (var i = 0; i < 20; i++)
            {
                if (!activeMoles.ContainsKey(idx))
                {
                    return idx;
                }
                idx = (idx + 1) % holes.Count;
            }
            return -1;
        }

        private void SpawnMoleAt(int holeIndex)
        {
            var mole = pool.Dequeue();
            var holeTr = holes[holeIndex];

            mole.transform.SetParent(holeTr, false);
            mole.transform.localPosition = Vector3.zero;
            mole.transform.localRotation = Quaternion.identity;
            mole.gameObject.SetActive(true);

            activeMoles.Add(holeIndex, mole);

            mole.Pop(0.5f, m =>
            {
                if (this) ReturnToPool(m, holeIndex);
            });
        }

        private void ReturnToPool(Mole.Mole mole, int holeIndex)
        {
            activeMoles.Remove(holeIndex);

            mole.gameObject.SetActive(false);
            mole.transform.SetParent(transform, false);
            pool.Enqueue(mole);
        }
    }
}