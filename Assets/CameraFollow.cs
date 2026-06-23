using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // Nhân vật (Breathing Idle)
    public float distance = 3.0f;     // Khoảng cách đứng sau nhân vật
    public float height = 1.8f;       // Độ cao của camera so với chân nhân vật

    void LateUpdate()
    {
        if (target == null) return;

        // Tính toán vị trí mong muốn của camera dựa trên vị trí nhân vật
        Vector3 targetPosition = target.position - (target.forward * distance);
        targetPosition.y = target.position.y + height;

        // Cập nhật vị trí camera
        transform.position = targetPosition;

        // Cho camera nhìn thẳng vào phần ngực/đầu của nhân vật (né phần chân)
        Vector3 lookTarget = target.position + (Vector3.up * height);
        transform.LookAt(lookTarget);
    }
}