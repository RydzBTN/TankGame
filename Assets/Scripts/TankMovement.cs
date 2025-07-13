using Unity.VisualScripting;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public float motorForce = 8000f;      // siła napędu
    public float turnTorque = 3000f;      // moment skręcający
    public float maxSpeed = 20f;          // ograniczenie prędkości
    
    [Header("Pomiar prędkości")]
    public float currentSpeed;            // aktualna prędkość
    public float speedKmh;                // prędkość w km/h
    

   
    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Obliczanie aktualnej prędkości
        UpdateSpeed();
        
        // Ograniczenie prędkości
        LimitSpeed();

        
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(rb.transform.forward * motorForce);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(-rb.transform.forward * motorForce);
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.S))
            {
                rb.AddTorque(Vector3.up * turnTorque);
            }
            else
            {
                rb.AddTorque(Vector3.down * turnTorque);
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.S))
            {
                rb.AddTorque(Vector3.down * turnTorque);
            }
            else
            {
                rb.AddTorque(Vector3.up * turnTorque);
            }
        }

        ApplyFriction();

    }
    
    private void UpdateSpeed()
    {
        // Obliczenie prędkości z uwzględnieniem kierunku (projekcja wektora prędkości na wektor "do przodu")
        currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        
        // Przeliczenie na km/h (1 m/s = 3.6 km/h)
        speedKmh = currentSpeed * 3.6f;
    }
    
    private void LimitSpeed()
    {
        // Jeśli przekraczamy maksymalną prędkość
        if (currentSpeed > maxSpeed)
        {
            // Normalizacja wektora prędkości i ograniczenie do maxSpeed
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
	
    private void ApplyFriction()
    {
        // sztuczne hamowanie, jeśli nie naciskasz W/S
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 counterForce = -horizontalVelocity * 3.0f; // tuning: 0.1–5.0

        rb.AddForce(counterForce, ForceMode.Acceleration);

        // sztuczne opóźnienie obrotu, jeśli nie skręcasz
        Vector3 angularVelocity = rb.angularVelocity;
        Vector3 horizontalAngular = new Vector3(0f, angularVelocity.y, 0f);
        Vector3 counterTorque = -horizontalAngular * 4.0f; // tuning

        rb.AddTorque(counterTorque, ForceMode.Acceleration);
    }

}
