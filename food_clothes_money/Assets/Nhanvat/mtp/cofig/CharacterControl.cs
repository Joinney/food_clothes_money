using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private CharacterController controller;
    public Transform cameraTransform; 

    [Header("Tốc độ di chuyển")]
    public float walkSpeed = 6.0f;     
    public float runSpeed = 15.0f;     
    
    [Header("Cơ chế Nhảy & Trọng lực")]
    public float jumpHeight = 3.5f;        // Độ cao bước nhảy
    public float gravityMultiplier = 3.0f; // Hệ số kéo rơi dứt khoát
    private Vector3 velocity;              
    private float jumpCooldown = 0f;       // Thời gian hồi để tránh spam nút trên không

    [Header("Xoay Chuột")]
    public float mouseSensitivity = 2.0f;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    public float upLookLimit = -40f;  
    public float downLookLimit = 60f; 

    private Vector3 cameraOffset;
    private Animator anim;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        
        // Khóa con trỏ chuột vào giữa màn hình game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
        {
            cameraOffset = cameraTransform.position - transform.position;
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

        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
            Vector3 targetCamPos = transform.position + (Quaternion.Euler(verticalRotation, horizontalRotation, 0f) * cameraOffset);
            cameraTransform.position = targetCamPos;
        }

        // 2. DI CHUYỂN MẶT PHẲNG (X/Z) VÀ XỬ LÝ SHIFT
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveZ = Input.GetAxisRaw("Vertical");   

        bool isRunning = false;

        // Chỉ chạy khi bấm Tiến (moveZ > 0), KHÔNG bấm phím ngang (moveX == 0) và đè Shift
        if (moveZ > 0f && moveX == 0f && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            isRunning = true;
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 forward = transform.forward * moveZ;
        Vector3 right = transform.right * moveX;
        
        // Tạo vector di chuyển phẳng tổng hợp
        Vector3 moveDirection = (forward + right).normalized * currentSpeed;

        // 3. XỬ LÝ NHẢY & TRỌNG LỰC TRỤC Y
        if (controller != null)
        {
            // Trừ thời gian hồi chiêu nhảy theo thời gian thực
            if (jumpCooldown > 0f) 
            {
                jumpCooldown -= Time.deltaTime;
            }

            // Đồng bộ trạng thái chạm đất thực tế từ CharacterController
            bool isGroundedCurrently = controller.isGrounded;

            if (isGroundedCurrently && velocity.y < 0)
            {
                velocity.y = -2f; // Ép lực nhỏ bám sàn ổn định, giúp isGrounded luôn chính xác
            }

            // ĐIỀU KIỆN NHẢY: Phải chạm đất và hết thời gian hồi chiêu
            if (Input.GetKeyDown(KeyCode.Space) && jumpCooldown <= 0f && isGroundedCurrently)
            {
                // Công thức vật lý tính toán gia tốc nhảy v = sqrt(h * -2 * g)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y * gravityMultiplier);
                jumpCooldown = 0.5f; // Khóa phím 0.5 giây tương thích với độ dài hoạt ảnh bật lên

                // Kích hoạt Trigger nhảy trong Animator
                if (anim != null) 
                {
                    anim.ResetTrigger("Jump"); // Reset để tránh trùng lặp lệnh cũ
                    anim.SetTrigger("Jump"); 
                }
            }

            // Áp dụng trọng lực liên tục kéo xuống theo thời gian
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

            // GỘP CHUNG VECTOR: Kết hợp vận tốc di chuyển ngang và vận tốc rơi/nhảy dọc vào một lệnh duy nhất
            Vector3 finalVelocity = moveDirection + Vector3.up * velocity.y;

            // Thực thi di chuyển toàn diện (Chỉ gọi lệnh Move duy nhất một lần để tránh xung đột sụt khung hình)
            if (controller.enabled)
            {
                controller.Move(finalVelocity * Time.deltaTime);
            }
        }

        // 4. ĐỒNG BỘ HOẠT ẢNH DI CHUYỂN VỚI BLEND TREE
        if (anim != null)
        {
            // Kiểm tra lại trạng thái chạm đất sau khi đã tịnh tiến tọa độ
            bool isGroundedCurrently = controller != null && controller.isGrounded;

            // Chỉ cập nhật tốc độ Blend Tree khi nhân vật ĐANG CHẠM ĐẤT để giữ nguyên hoạt ảnh nhảy trên không
            if (isGroundedCurrently)
            {
                if (moveX != 0f || moveZ != 0f)
                {
                    float targetSpeedParam = isRunning ? 2f : 1f;

                    if (!isRunning)
                    {
                        anim.SetFloat("Speed", targetSpeedParam);
                    }
                    else
                    {
                        anim.SetFloat("Speed", targetSpeedParam, 0.05f, Time.deltaTime);
                    }
                }
                else
                {
                    anim.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
                }
            }
        }
    }
}