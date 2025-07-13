using System;
using Unity.Cinemachine;
using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Ustawienia celowania")]
    public float maxRayDistance = 3000f;
    public LayerMask targetLayers = -1; // Domyślnie wszystkie warstwy

    [Header("Referencje")]
    public Camera mainCamera;
    public Transform turret;
    public Transform barrel;

    [Header("Targeting settings")]
    public float turretRotationSpeed = 30f;
    public float barrelElevationSpeed = 30f;
    public float minElevation = 5f;
    public float maxElevation = -20f;

    [Header("Dostępne Pociski")]
    public Bullet[] availableShells;
    public float ReloadTime = 7.1f;
    public int SelectedShellIndex = 0;
    public string SelectedShellName = ""; // Nazwa domyślnego pocisku

    [Header("Cinemachine Cameras")]
    public CinemachineCamera tppCamera;
    public CinemachineCamera gunnerCamera;

    private TPPCameraController tppCameraController;
    private GunnerCameraController gunnerCameraController;

    [Header("Debug")]
    public bool showDebugRay = true;
    public Color cameraRayColor = Color.red;
    public Color barrelRayColor = Color.green;
    public Vector3 targetPosition;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        tppCameraController = tppCamera.GetComponent<TPPCameraController>();
        gunnerCameraController = gunnerCamera.GetComponent<GunnerCameraController>();

        //włącznie kamery TPP
        SetActiveCameraByIndex(0);

        SelectedShellName = availableShells[SelectedShellIndex].name;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector3 lookPosition = targetPosition;

            if (tppCamera.gameObject.activeInHierarchy)
            {
                // włączenie kamery Gunner
                SetActiveCameraByIndex(1);
                gunnerCameraController.LookAtPoint(lookPosition);
            }
            else if (gunnerCamera.gameObject.activeInHierarchy)
            {
                // włączenie kamery TPP
                SetActiveCameraByIndex(0);
                tppCameraController.LookAtPoint(lookPosition);
            }
        }


        // zmian pocisku
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeSelectedShell(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeSelectedShell(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeSelectedShell(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeSelectedShell(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ChangeSelectedShell(4);
        }

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireShell(availableShells[SelectedShellIndex], barrel.position);
        }


        targetPosition = ShootRaycastFromCamera(maxRayDistance);
        AimAtTarget(turret, barrel, targetPosition);

        if (showDebugRay)
        {
            // raycast z kamery
            Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * maxRayDistance, cameraRayColor);
            // raycast z lufy działa
            Debug.DrawRay(barrel.position, barrel.forward * maxRayDistance, barrelRayColor);
        }
    }

    private void SetActiveCameraByIndex(int index)
    {
        // Deactivate all cameras
        tppCamera.Priority = 0;
        gunnerCamera.Priority = 0;

        // Activate the selected camera with higher priority
        switch (index)
        {
            // włączenie TPP camera
            case 0:
                tppCamera.gameObject.SetActive(true);
                tppCamera.Priority = 10;
                gunnerCamera.gameObject.SetActive(false);
                break;
            // włączenie Gunner camera
            case 1:
                gunnerCamera.gameObject.SetActive(true);
                gunnerCamera.Priority = 10;
                tppCamera.gameObject.SetActive(false);
                break;
        }
    }

    private Vector3 ShootRaycastFromCamera(float rayLength)
    {
        // ray ze środka ekranu
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        bool didHit = Physics.Raycast(ray, out hit, maxRayDistance, targetLayers);
        if (didHit)
        {
            return hit.point;
        }
        else //nie trafiamy w grunt
        {
            return (ray.origin + ray.direction * rayLength);
        }
    }

    public void AimAtTarget(Transform turret, Transform barrel, Vector3 targetPoint)
    {
        if (turret == null || barrel == null)
            return;

        Vector3 turretToTarget = targetPoint - turret.position;
        Vector3 localTurretToTarget = transform.InverseTransformDirection(turretToTarget);
        Vector3 horizontalLocalDirection = new Vector3(localTurretToTarget.x, 0f, localTurretToTarget.z);
        Vector3 horizontalDirection = transform.TransformDirection(horizontalLocalDirection);

        if (horizontalDirection.magnitude > 0.001f)
        {
            Quaternion targetTurretRotation = Quaternion.LookRotation(horizontalDirection, transform.up);
            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                targetTurretRotation,
                turretRotationSpeed * Time.deltaTime
            );
        }

        // ELEWACJA DZIAŁA (oś X)
        Vector3 barrelToTarget = targetPoint - barrel.position;
        Vector3 barrelLocalPosition = barrel.localPosition;
        Vector3 targetLocalPosition = turret.InverseTransformPoint(targetPoint);
        Vector3 localBarrelToTarget = targetLocalPosition - barrelLocalPosition;

        // kąt elewacji w przestrzeni lokalnej turret
        float targetElevation = -Mathf.Atan2(localBarrelToTarget.y,
                               Mathf.Sqrt(localBarrelToTarget.x * localBarrelToTarget.x +
                                         localBarrelToTarget.z * localBarrelToTarget.z)) * Mathf.Rad2Deg;

        float clampedElevation = Mathf.Clamp(targetElevation, maxElevation, minElevation);

        float currentElevation = barrel.localEulerAngles.x;
        if (currentElevation > 180f)
            currentElevation -= 360f;

        // Płynne dostosowanie elewacji
        float newElevation = Mathf.MoveTowards(currentElevation, clampedElevation, barrelElevationSpeed * Time.deltaTime);
        barrel.localEulerAngles = new Vector3(newElevation, 0f, 0f);
    }

    public void ChangeSelectedShell(int index)
    {
        if (index < 0 || index >= availableShells.Length)
        {
            Debug.LogWarning("Invalid shell index: " + index);
            return;
        }

        SelectedShellIndex = index;
        SelectedShellName = availableShells[SelectedShellIndex].name;
    }

    public void FireShell(Bullet projectile, Vector3 spawnPlace)
    {
        if (projectile == null)
        {
            Debug.LogWarning("No bullet projectile provided!");
            return;
        }
        Instantiate(projectile, spawnPlace, barrel.rotation);

        
        //Debug.Log($"Fired {projectile.name} from position {spawnPlace}");
    }
}
        
