using Metaverse.Interactions.BallPool;
using UnityEngine;

public class HittableManager : MonoBehaviour
{
    [SerializeField] private HittableChocolate[] chocolates;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    private void Awake()
    {
        // 초기에 위치/회전 저장
        initialPositions = new Vector3[chocolates.Length];
        initialRotations = new Quaternion[chocolates.Length];

        for (int i = 0; i < chocolates.Length; i++)
        {
            if (chocolates[i] == null) continue;
            initialPositions[i] = chocolates[i].transform.position;
            initialRotations[i] = chocolates[i].transform.rotation;
        }
    }

    /// <summary>
    /// 부서진 초콜릿을 초기 상태로 복원
    /// </summary>
    public void ResetChocolates()
    {
        for (int i = 0; i < chocolates.Length; i++)
        {
            // 기존 초콜릿이 파괴되었으면 새로 생성
            if (chocolates[i] == null)
            {
                print($"초콜릿 {i} 재생성");
                // 프리팹 참조를 위해 원본이 필요 (HittableChocolate 프리팹)
                // 예: chocolates[i]가 Prefab이 아니라면 따로 public 변수로 prefab 참조
                continue;
            }

            var choco = chocolates[i];
            Rigidbody rb = choco.GetComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            choco.transform.SetPositionAndRotation(initialPositions[i], initialRotations[i]);
        }

        // 남아 있는 작은 초콜릿 파편 제거
        foreach (Transform child in transform)
        {
            if (child.name.Contains("SmallChocolate"))
                Destroy(child.gameObject);
        }
    }
}