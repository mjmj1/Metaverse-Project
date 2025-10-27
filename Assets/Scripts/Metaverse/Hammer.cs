using Oculus.Interaction;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    public GrabInteractable interactable;
    public Vector3 lastPos;
    public Vector3 velocity;
    public float Speed;

    [Header("정렬 오프셋")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 90);
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.05f, 0);

    private void Awake()
    {
        interactable = GetComponentInChildren<GrabInteractable>();
    }
    
    private void Start()
    {
        lastPos = transform.position;
        
        interactable.WhenInteractorViewAdded += OnGrabbed;
    }

    private void Update()
    {
        // velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
        Speed = velocity.magnitude;
    }

    // ✅ 손으로 잡을 때 한 번만 호출
    public void OnGrabbed(IInteractorView view)
    {
        print("AlignTo");
        
        print(view.Data);
    }
}