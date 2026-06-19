using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private CharacterController controller;
    public Transform cameraTransform; 

    [Header("Di chuyển")]
    public float moveSpeed = 5.0f;
    
    [Header("Xoay Chuột")]
    public float mouseSensitivity = 2.0f;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    public float upLookLimit = -40f;  
    public float downLookLimit = 60f; 

    // Tự động lưu khoảng cách bạn đã kéo tay ngoài Scene
    private Vector3 cameraOffset;
    private Animator anim;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        // Khóa con trỏ chuột vào giữa màn hình game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ĐOẠN MỚI: Tính toán khoảng cách (Offset) giữa Camera và Player lúc bắt đầu game
        if (cameraTransform != null)
        {
            // Tính toán vị trí tương đối của Camera so với Player cha
            cameraOffset = cameraTransform.position - transform.position;
            
            // Đưa Camera ra khỏi cơ chế tính vị trí gốc của cha để di chuyển độc lập bằng code
            cameraTransform.SetParent(null);
        }
    }

    void Update()
    {
        // 1. XOAY CAMERA VÀ NHÂN VẬT CHUẨN THEO CHUỘT
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, upLookLimit, downLookLimit);

        // Xoay toàn bộ Player (Cha) theo trục Ngang (Trái/Phải)
        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Chỉ xoay riêng Camera theo trục Dọc (Lên/Xuống)
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
            
            // ĐOẠN MỚI: Ép vị trí Camera chạy tịnh tiến bám sát theo Player theo khoảng cách đã kéo tay
            Vector3 targetCamPos = transform.position + (Quaternion.Euler(verticalRotation, horizontalRotation, 0f) * cameraOffset);
            cameraTransform.position = targetCamPos;
        }

        // 2. DI CHUYỂN CHUẨN HƯỚNG (SỬA LỖI LOẠN PHÍM)
        float moveX = Input.GetAxis("Horizontal"); // A, D
        float moveZ = Input.GetAxis("Vertical");   // W, S

        // Tính toán hướng đi đồng bộ với hướng xoay của Player cha
        Vector3 forward = transform.forward * moveZ;
        Vector3 right = transform.right * moveX;
        Vector3 moveDirection = (forward + right).normalized * moveSpeed;

        // Dùng SimpleMove: Tự động áp dụng trọng lực thích hợp và giúp leo cầu thang mượt hơn
        if (controller != null)
        {
            controller.SimpleMove(moveDirection);
        }
        if (anim != null)
        {
            // Tính toán độ lớn của lệnh di chuyển (bằng 0 nếu đứng im, tiến gần bằng 1 nếu chạy)
            float currentMovementSpeed = new Vector2(moveX, moveZ).sqrMagnitude;
            
            // Truyền giá trị vào biến "Speed" trong Animator để kích hoạt trạng thái chạy
            anim.SetFloat("Speed", currentMovementSpeed);
        }
    }
}