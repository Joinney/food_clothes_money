using UnityEngine;
// BẮT BUỘC phải có dòng này để gọi hệ thống Input mới
using UnityEngine.InputSystem; 

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 25f;
    public float runSpeed = 45f;
    public float rotationSpeed = 720f;

    private Animator animator;
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 inputDirection = Vector3.zero;

        // Đọc phím theo chuẩn Input System mới (hỗ trợ cả W-A-S-D và phím mũi tên)
        if (Keyboard.current != null)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal = 1f;

            inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }

        // Xử lý di chuyển
        if (inputDirection.magnitude >= 0.1f)
        {
            bool isRunning = Keyboard.current.leftShiftKey.isPressed;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            // Xoay hướng mượt mà
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            moveDirection = inputDirection * currentSpeed;

            float animationSpeedParam = isRunning ? 2f : 1f;
            if (animator != null) animator.SetFloat("Speed", animationSpeedParam, 0.15f, Time.deltaTime);
        }
        else
        {
            moveDirection = Vector3.zero;
            if (animator != null) animator.SetFloat("Speed", 0f, 0.15f, Time.deltaTime);
        }

        // Ép tịnh tiến tọa độ qua Character Controller
        if (controller != null && controller.enabled)
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
        else
        {
            // Nếu Character Controller đang tắt, ép tọa độ thô để test
            transform.position += moveDirection * Time.deltaTime;
        }
    }
}