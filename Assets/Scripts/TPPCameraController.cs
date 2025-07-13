using TMPro;
using UnityEngine;

public class TPPCameraController : MonoBehaviour
{
    [Header("Ustawienia celu")]
    public Transform target; // Transform czołgu, za którym podąża kamera
    public Vector3 offset;
    public float distance = 5f; // Dystans od celu
    
    [Header("Sterowanie kamerą")]
    public float mouseSensitivity = 120f; // Czułość myszy
    
    public float yMinLimit = -80f; // Minimalny kąt w pionie
    public float yMaxLimit = 80f;  // Maksymalny kąt w pionie
    
    public bool isZoomed = false;
    
    private float x = 0.0f; // Bieżący kąt w poziomie
    private float y = 0.0f; // Bieżący kąt w pionie
    private float tmpDistance;
    
    private void Start()
    {
        tmpDistance = distance;
        
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
	
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (isZoomed)
            {
                distance = tmpDistance;
                isZoomed = false;
            }
            else
            {
                distance = 0.5f;
                isZoomed = true;
            }
        }   
        
        
        // Odczyt ruchu myszy
        x += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Ograniczenie kąta pionowego
        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        // Wyznaczenie nowej pozycji i rotacji kamery
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position + offset;
        
        transform.rotation = rotation;
        transform.position = position;
    }

    /// <summary>
    /// Ustawia rotację kamery tak, by patrzyła w zadany punkt (world space)
    /// </summary>
    public void LookAtPoint(Vector3 worldPoint)
    {
        if (target == null) return;
        Vector3 direction = worldPoint - (target.position + offset);
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Vector3 angles = lookRotation.eulerAngles;
        
        // Set Y (horizontal) rotation
        x = angles.y;
        
        // Normalize X rotation angle to handle looking up/down correctly
        float normalizedX = angles.x;
        if (normalizedX > 180f)
            normalizedX -= 360f;
            
        y = normalizedX;
        
        // Ensure the angle is clamped to our limits
        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
    }

    /// <summary>
    /// Zwraca punkt, w który patrzy kamera (punkt na ray z kamery)
    /// </summary>
    public Vector3 GetLookPoint(float distance = 1000f)
    {
        return transform.position + transform.forward * distance;
    }
}
