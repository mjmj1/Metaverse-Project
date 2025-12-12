using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace Game
{
    public class VRButtonInteraction : MonoBehaviour
    {
        /*[Header("VR Interaction")]
        [SerializeField] private XRDirectInteractor rightHandInteractor;
        [SerializeField] private float rayDistance = 10f;
        [SerializeField] private LineRenderer laserPointer;

        private Camera vrCamera;
        private RaycastHit lastHitInfo;

        private void Start()
        {
            vrCamera = Camera.main;

            // VR 컨트롤러 자동 찾기
            if (rightHandInteractor == null)
            {
                var interactors = FindObjectsOfType<XRDirectInteractor>();
                foreach (var interactor in interactors)
                {
                    if (interactor.transform.name.ToLower().Contains("right"))
                    {
                        rightHandInteractor = interactor;
                        break;
                    }
                }
            }

            // 레이저 포인터 설정
            if (laserPointer == null)
                laserPointer = GetComponent<LineRenderer>();

            SetupLaserPointer();
        }

        private void SetupLaserPointer()
        {
            if (laserPointer != null)
            {
                laserPointer.positionCount = 2;
                laserPointer.startWidth = 0.005f;
                laserPointer.endWidth = 0.005f;
                laserPointer.material = new Material(Shader.Find("Sprites/Default"));
                laserPointer.startColor = Color.white;
                laserPointer.endColor = new Color(1, 1, 1, 0.5f);
            }
        }

        private void Update()
        {
            UpdateLaserPointer();
            HandleButtonInteraction();
        }

        private void UpdateLaserPointer()
        {
            if (laserPointer == null || vrCamera == null) return;

            Vector3 rayOrigin = vrCamera.transform.position;
            Vector3 rayDirection = vrCamera.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out lastHitInfo, rayDistance))
            {
                // 버튼에 레이저 닿았을 때
                laserPointer.SetPosition(0, rayOrigin);
                laserPointer.SetPosition(1, lastHitInfo.point);

                // 버튼 하이라이트 효과
                var button = lastHitInfo.collider.GetComponent<Button>();
                if (button != null)
                {
                    HighlightButton(button);
                }
            }
            else
            {
                // 레이저가 닿지 않을 때
                laserPointer.SetPosition(0, rayOrigin);
                laserPointer.SetPosition(1, rayOrigin + rayDirection * rayDistance);
            }
        }

        private void HandleButtonInteraction()
        {
            // VR 컨트롤러 트리거 입력 감지 (간단한 구현)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (lastHitInfo.collider != null)
                {
                    var button = lastHitInfo.collider.GetComponent<Button>();
                    if (button != null && button.interactable)
                    {
                        Debug.Log($"VR 버튼 클릭: {button.name}");
                        button.onClick.Invoke();
                    }
                }
            }
        }

        private void HighlightButton(Button button)
        {
            // 버튼 하이라이트 효과
            var colors = button.colors;
            colors.normalColor = Color.yellow;
            button.colors = colors;
        }

        // 간단한 테스트용 레이캐스트 (키보드 입력으로 테스트)
        [ContextMenu("테스트: 레이캐스트 테스트")]
        public void TestRaycast()
        {
            if (vrCamera == null)
            {
                Debug.LogWarning("VR 카메라를 찾을 수 없습니다.");
                return;
            }

            Vector3 rayOrigin = vrCamera.transform.position;
            Vector3 rayDirection = vrCamera.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
            {
                Debug.Log($"레이캐스트 히트: {hit.collider.name}");

                var button = hit.collider.GetComponent<Button>();
                if (button != null)
                {
                    Debug.Log($"버튼 발견: {button.name}");
                    button.onClick.Invoke();
                }
            }
        }*/
    }

    // VR 버튼을 위한 간단한 콜라이더 트리거
    public class VRButtonTrigger : MonoBehaviour
    {
        /*private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // VR 컨트롤러나 레이캐스트가 버튼에 닿았을 때
            if (button != null && button.interactable)
            {
                Debug.Log($"VR 버튼 트리거: {button.name}");
                button.onClick.Invoke();
            }
        }*/
    }
}