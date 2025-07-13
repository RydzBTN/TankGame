using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GunnerCameraController : MonoBehaviour
{
    [Header("Sterowanie kamerą")]
    public float normalSensitivity = 120f; // Czułość myszy
    public float zoomSensitivity = 40f;
    
    public float yMax = -80f; // Minimalny kąt w pionie
    public float yMin = 80f;  // Maksymalny kąt w pionie

    public bool isZoomed = false;
    
    public float rotationX = 0f;
    public float rotationY = 0f;
    public float mouseSensitivity;
   
    void Start()
    {
        // Zapisz aktualne kąty rotacji kamery, jeśli kamera ma już jakieś obrócenie
        Vector3 rotation = transform.rotation.eulerAngles;
        rotationY = rotation.y;
        rotationX = rotation.x;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        mouseSensitivity = normalSensitivity;
    }

    
    void LateUpdate()
    {
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        rotationY = Mathf.Clamp(rotationY, yMin, yMax);
        
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (isZoomed)
            {
                gameObject.GetComponent<CinemachineCamera>().Lens.FieldOfView = 12;
                mouseSensitivity = normalSensitivity;
                isZoomed = false;
            }
            else
            {
                gameObject.GetComponent<CinemachineCamera>().Lens.FieldOfView = 4;
                mouseSensitivity = zoomSensitivity;
                isZoomed = true;
            }
        }
        
        
    }

    /// <summary>
    /// Ustawia rotację kamery tak, by patrzyła w zadany punkt (world space)
    /// </summary>
    public void LookAtPoint(Vector3 worldPoint)
    {
        Vector3 direction = worldPoint - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Vector3 angles = lookRotation.eulerAngles;
        
        // Fix angle normalization for proper up/down switching
        rotationX = angles.y;
        
        // Normalize X rotation angle to handle looking up/down correctly
        float normalizedX = angles.x;
        if (normalizedX > 180f)
            normalizedX -= 360f;
            
        rotationY = normalizedX;
        
        // Ensure the angle is clamped to our limits
        rotationY = Mathf.Clamp(rotationY, yMin, yMax);
    }

    /// <summary>
    /// Zwraca punkt, w który patrzy kamera (punkt na ray z kamery)
    /// </summary>
    public Vector3 GetLookPoint(float distance = 1000f)
    {
        return transform.position + transform.forward * distance;
    }
}
