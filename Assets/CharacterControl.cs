using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private CharacterController controller;
    public Transform cameraTransform; 

    [Header("Tốc độ di chuyển")]
    public float walkSpeed = 7.5f;     // Tốc độ đi bộ dứt khoát (đã tăng nhẹ từ 6 lên 7.5)
    public float runSpeed = 18.0f;     // Tốc độ chạy nhanh bứt tốc (tăng từ 15 lên 18)
    
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
        // 1. XOAY CAMERA VÀ NHÂN VẬT
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

        // 2. TÍNH TOÁN ĐI BỘ VÀ CHẠY NHANH
        float moveX = Input.GetAxis("Horizontal"); // A, D
        float moveZ = Input.GetAxis("Vertical");   // W, S

        Vector3 forward = transform.forward * moveZ;
        Vector3 right = transform.right * moveX;
        
        bool isMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f);
        
        // KIỂM TRA PHÍM SHIFT
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDirection = (forward + right).normalized * currentSpeed;

        if (controller != null)
        {
            controller.SimpleMove(moveDirection);
        }

        // 3. ĐỒNG BỘ TỐC ĐỘ ANIMATION THEO TỪNG ĐỘNG TÁC
        if (anim != null)
        {
            float animSpeedValue = 0f;
            
            if (isMoving)
            {
                animSpeedValue = isRunning ? 1.0f : 0.5f;
            }

            // Chuyển đổi trạng thái mượt mà
            float currentAnimFloat = anim.GetFloat("Speed");
            float smoothedSpeed = Mathf.MoveTowards(currentAnimFloat, animSpeedValue, Time.deltaTime * 6f);
            anim.SetFloat("Speed", smoothedSpeed);

            // TĂNG MẠNH NHỊP CHÂN TẠI ĐÂY:
            if (!isMoving)
            {
                anim.speed = 1.0f; // Đứng im giữ nguyên tốc độ thở (Idle)
            }
            else if (isRunning)
            {
                // Tăng từ 1.35f lên 1.75f hoặc 2.0f (Ép chân guồng nhanh gấp 1.75 - 2 lần bình thường khi chạy)
                anim.speed = 1.65f; 
            }
            else
            {
                // Tăng từ 1.15f lên 1.45f (Ép chân bước dứt khoát, liên tục, không còn lờ đờ thong thả)
                anim.speed = 1.55f; 
            }
        }
    }
}