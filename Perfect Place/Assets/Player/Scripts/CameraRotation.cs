using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 30f;        // Degrees per second
    public float edgeThickness = 20f;        // Edge trigger distance in pixels

    [Header("Rotation Limits (Optional)")]
    public bool useYRotationLimits = false;
    public float minYRotation = -45f;
    public float maxYRotation = 45f;

    private float currentYRotation;

    void Start()
    {
        currentYRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        float rotateDirection = 0f;
        Vector3 mousePos = Input.mousePosition;

        // Check horizontal edges
        if (mousePos.x < edgeThickness)
        {
            rotateDirection = -1f; // Rotate left
        }
        else if (mousePos.x > Screen.width - edgeThickness)
        {
            rotateDirection = 1f; // Rotate right
        }

        if (rotateDirection != 0f)
        {
            float deltaRotation = rotateDirection * rotationSpeed * Time.deltaTime;
            currentYRotation += deltaRotation;

            if (useYRotationLimits)
            {
                currentYRotation = Mathf.Clamp(currentYRotation, minYRotation, maxYRotation);
            }

            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }
}
