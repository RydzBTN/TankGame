using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TankSuspension : MonoBehaviour
{
    [Header("Referencje")]  
    [SerializeField] private Transform[] wheels;
    [SerializeField] private Transform[] suspensionPoints;

    [Header("Suspension")]
    [SerializeField] private float springStrength;
    [SerializeField] private float damperStrength;
    [SerializeField] private float wheelRadius;
    [SerializeField] private float suspensionDistance;

    private Rigidbody tankRigidbody;

    private void Awake()
    {
        tankRigidbody = GetComponent<Rigidbody>();
        
        foreach (Transform suspensionPoint in suspensionPoints)
        {
            suspensionPoint.position = new Vector3(
                suspensionPoint.position.x,
                suspensionPoint.position.y - wheelRadius + suspensionDistance,
                suspensionPoint.position.z
            );
        }
    }

    private void FixedUpdate()
    {

        for (int i = 0; i < suspensionPoints.Length; i++)
        {
            Transform suspensionPoint = suspensionPoints[i];

            Vector3 downDirection = tankRigidbody.transform.TransformDirection(Vector3.down);
            Vector3 upDirection = tankRigidbody.transform.TransformDirection(Vector3.up);

            if (Physics.Raycast(suspensionPoint.position, downDirection, out RaycastHit hit, suspensionDistance))
            {
                Debug.DrawLine(suspensionPoint.position, hit.point, Color.green);
                wheels[i].position = hit.point + upDirection * wheelRadius;

                float springCompression = suspensionDistance - hit.distance;

                Vector3 suspensionWorldVelocity = tankRigidbody.GetPointVelocity(suspensionPoint.position);
                float verticalVelocity = Vector3.Dot(suspensionWorldVelocity, upDirection);

                float springForce = springStrength * springCompression;
                float damperForce = damperStrength * verticalVelocity;
                float totalForce = springForce - damperForce;
                tankRigidbody.AddForceAtPosition(upDirection * totalForce, suspensionPoint.position, ForceMode.Force);

            }
            else
            {
                Debug.DrawRay(suspensionPoint.position, downDirection * suspensionDistance, Color.red);
            }
        }


    }
    
    
}
