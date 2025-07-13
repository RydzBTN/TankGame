using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TankSuspension : MonoBehaviour
{
    [Header("Referencje")]
    public Transform[] suspensionsPoints;
    public Transform[] wheelsMeshes;
    public Transform[] leftTrackPoints;
    public Transform[] rightTrackPoints;
    
    [Header("Suspension")]
    public float springStrength;
    public float damperStrength;
    public float wheelRadius = 0.45f;
    public float suspensionDistance = 0.3f;

    [Header("Engine")]
    public float engineForce = 5000f;
    public float groundFriction = 0.7f;
    public float slidingFriction = 0.3f;
    
    private Rigidbody tankRigidbody;
    
    private void Awake()
    {
        tankRigidbody = GetComponent<Rigidbody>();
        
        
        //ustawienie górnego punktu amortyzatora na suspensionDistance od ziemi
        foreach (Transform suspensionPoint in suspensionsPoints)
        {
            suspensionPoint.position = new Vector3(suspensionPoint.position.x, suspensionPoint.position.y - wheelRadius + suspensionDistance, suspensionPoint.position.z);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < suspensionsPoints.Length; i++)
        {
            Transform suspensionPoint = suspensionsPoints[i];
                
            //obliczanie lokalnych wektorow w dol i gore
            Vector3 downDirection = tankRigidbody.transform.TransformDirection(Vector3.down);
            Vector3 upDirection = tankRigidbody.transform.TransformDirection(Vector3.up);
            

            //sprawdz odległość od podłoża
            if (Physics.Raycast(suspensionPoint.position, downDirection, out RaycastHit hit, suspensionDistance))
            {
                //rysowanie zielonego promienia
                Debug.DrawLine(suspensionPoint.position, hit.point, Color.green);
                
                //Ustawianie mesha koła w odpowiednim miejscu
                wheelsMeshes[i].position = hit.point + upDirection * wheelRadius;
                
                // Obliczenie ugięcia sprężyny
                float compression = suspensionDistance - hit.distance;

                // Obliczenie prędkości względnej
                Vector3 suspensionWorldVelocity = tankRigidbody.GetPointVelocity(suspensionPoint.position);
                float verticalVelocity = Vector3.Dot(suspensionWorldVelocity, upDirection);

                // Siła amortyzatora
                float springForce = springStrength * compression;
                float damperForce = damperStrength * verticalVelocity;
                float totalForce = springForce - damperForce;
                
                // Zastosowanie siły do ciała
                tankRigidbody.AddForceAtPosition(upDirection * totalForce, suspensionPoint.position, ForceMode.Force);
                
            }
            else
            {
                // Promień nie trafił w teren rysujemy go na czerwono
                Debug.DrawRay(suspensionPoint.position, downDirection * suspensionDistance, Color.red);;
            }
        }
        
        
    }
    
    void ApplyTrackForces(Transform[] trackPoints, bool isLeft)
    {
        float input = isLeft ? Input.GetAxis("Vertical") + Input.GetAxis("Horizontal") : Input.GetAxis("Vertical") - Input.GetAxis("Horizontal");

        foreach (var point in trackPoints)
        {
            if (Physics.Raycast(point.position, -transform.up, out RaycastHit hit, 1.0f))
            {
                // Siła normalna (nacisk na punkt)
                float normalForce = tankRigidbody.mass * Physics.gravity.magnitude / (leftTrackPoints.Length + rightTrackPoints.Length);

                // Maksymalna przyczepność
                float maxTraction = normalForce * groundFriction;

                // Docelowa siła napędu/hamowania
                float desiredForce = input * engineForce;

                // Zerwanie przyczepności
                float appliedFriction = Mathf.Abs(desiredForce) > maxTraction ? slidingFriction : groundFriction;
                float appliedForce = Mathf.Clamp(desiredForce, -maxTraction, maxTraction);

                // Poślizg (opcjonalnie: niech pojazd się ślizga gdy zerwana przyczepność)
                if (appliedFriction == slidingFriction)
                {
                    appliedForce = Mathf.Sign(desiredForce) * maxTraction * slidingFriction;
                }

                // Dodaj siłę w kierunku jazdy w punkcie przy ziemi
                tankRigidbody.AddForceAtPosition(transform.forward * appliedForce, point.position);

                // Opór gruntu (hamowanie naturalne)
                Vector3 velocityAtPoint = tankRigidbody.GetPointVelocity(point.position);
                Vector3 groundDrag = -velocityAtPoint * appliedFriction;
                tankRigidbody.AddForceAtPosition(groundDrag, point.position);
            }
        }
    }
    
    
}
