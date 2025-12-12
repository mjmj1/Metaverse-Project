using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class PlayManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject[] molePrefabs; // 두더지 프리팹
        [SerializeField] private Transform playerHead;
        [SerializeField] private int totalMoles = 15;
        [SerializeField] private float radius = 2.0f;
        [SerializeField] private float spawnInterval = 1.5f;

        private List<Mole> spawnedMoles = new List<Mole>(); // Mole 스크립트가 있다고 가정
        private bool isSpawning = false;

        // 게임 시작 전 구멍 배치 (GameManager가 호출)
        public void InitializeMoles()
        {
            // 기존 두더지 정리
            foreach (var mole in spawnedMoles)
            {
                if (mole != null) Destroy(mole.gameObject);
            }
            spawnedMoles.Clear();

            // 돔 형태로 구멍 생성
            for (int i = 0; i < totalMoles; i++)
            {
                float yaw = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float pitch = Random.Range(-20f, 40f) * Mathf.Deg2Rad;

                float y = Mathf.Sin(pitch) * radius;
                float h = Mathf.Cos(pitch) * radius;
                float x = Mathf.Cos(yaw) * h;
                float z = Mathf.Sin(yaw) * h;

                Vector3 pos = (playerHead != null) ? playerHead.position + new Vector3(x, y, z) : new Vector3(x, y, z);

                // 프리팹이 할당되어 있다면 생성
                if (molePrefabs != null && molePrefabs.Length > 0)
                {
                    GameObject prefab = molePrefabs[Random.Range(0, molePrefabs.Length)];
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity);

                    if (playerHead != null) go.transform.LookAt(playerHead);

                    // Mole 컴포넌트가 있으면 리스트에 추가
                    Mole m = go.GetComponent<Mole>();
                    if (m != null) spawnedMoles.Add(m);
                }
            }
        }

        // 게임 시작 (GameManager가 호출)
        public void StartGameLoop()
        {

        }

        // 게임 종료 (GameManager가 호출)
        public void StopGameLoop()
        {
            isSpawning = false;
            StopAllCoroutines();

            // 모든 두더지 숨기기
            foreach (var mole in spawnedMoles)
            {
                // if(mole) mole.Hide();
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (isSpawning)
            {
                yield return new WaitForSeconds(spawnInterval);

                if (spawnedMoles.Count > 0)
                {
                    int idx = Random.Range(0, spawnedMoles.Count);
                    // if (spawnedMoles[idx] != null) spawnedMoles[idx].Rise();
                }
            }
        }
    }
}