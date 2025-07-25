using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows.Speech;

public class Powertrain : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI gearDisplay;
    [SerializeField] private TextMeshProUGUI engineRPMDisplay;
    [SerializeField] private TextMeshProUGUI maxSpeedDisplay;
    [SerializeField] private TextMeshProUGUI torqueOnWheelsDisplay;
    [SerializeField] private TextMeshProUGUI speedDisplay;
    [SerializeField] private TextMeshProUGUI rotationSpeedDisplay;

    [Header("Gearbox")]
    [SerializeField] private float shiftChangingTime;
    [SerializeField] private byte forwardGears;
    [SerializeField] private byte reverseGears;
    [SerializeField] private float[] forwardGearRatios;
    [SerializeField] private float[] reverseGearRatios;
    [SerializeField] private float wheelRadius;
    [SerializeField] private float finalDriveRatio;
    [SerializeField] private float maxRotateSpeed;
    [SerializeField] private GameObject[] rightWheels;
    [SerializeField] private GameObject[] leftWheels;
    [SerializeField] private Transform leftTrack;
    [SerializeField] private Transform rightTrack;


    [Header("Engine")]
    [SerializeField] private ushort minEngineRPM;
    [SerializeField] private ushort maxEngineRPM;
    [SerializeField] private AnimationCurve engineTorqueCurve;
    [SerializeField] private float brakeForce;
    [SerializeField] private AudioSource engineSound;

    private Rigidbody tankRigidbody;
    [Header("Testing values")]
    [Space(5)]
    public bool isEngineOn = false;
    public short currentGear;
    private float currentEngineRPM = 0;
    public bool isShifting = false;
    public bool isClutchEngaged = true;
    public float leftTrackRatio = 0f;
    public float rightTrackRatio = 0f;
    public bool enableAutoShifting = true;
    private float speed;
    private float rotationSpeed;
    public float leftTrackBrakeRatio = 0f;
    public float rightTrackBrakeRatio = 0f;

    private bool isForwardKeyPressed = false;
    private bool isReverseKeyPressed = false;
    private bool isTurnLeftKeyPressed = false;
    private bool isTurnRightKeyPressed = false;

    private void OnValidate()
    {
        // Dostosowuje rozmiar tablic gdy zmienia się liczba biegów
        // 0 to neutral
        if (forwardGearRatios == null || forwardGearRatios.Length != forwardGears + 1)
        {
            System.Array.Resize(ref forwardGearRatios, forwardGears + 1);

        }

        if (reverseGearRatios == null || reverseGearRatios.Length != reverseGears)
        {
            System.Array.Resize(ref reverseGearRatios, reverseGears);
        }
    }
    private void Awake()
    {
        tankRigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        currentGear = 0;
    }
    private void Update()
    {
        UpdateUI();
        HandleInput();

    }
    private void FixedUpdate()
    {
        speed = Vector3.Dot(tankRigidbody.linearVelocity, transform.forward) * 3.6f;
        rotationSpeed = tankRigidbody.angularVelocity.magnitude * Mathf.Rad2Deg;
        CalculateEngineRPM();
        RotateDriveWheels();
        ApplyTrackForce(leftTrackRatio, rightTrackRatio);
        ApplyBrakeForce(leftTrackBrakeRatio, rightTrackBrakeRatio);
        LimitRotateSpeed();
    }
    private void UpdateUI()
    {
        gearDisplay.text = $"Gear: {currentGear}";
        engineRPMDisplay.text = $"RPM: {currentEngineRPM:F0}";
        maxSpeedDisplay.text = $"MaxSpd: {MaxSpeedOnCurrentGear():F1} km/h";
        torqueOnWheelsDisplay.text = $"wheel Nm: {engineTorqueCurve.Evaluate(currentEngineRPM) * GetCurrentGearRatio() * finalDriveRatio:F0}";
        speedDisplay.text = $"Speed: {speed} km/h";
        rotationSpeedDisplay.text = $"Rotation Speed: {rotationSpeed:F1} deg/s";
    }
    private void HandleInput()
    {
        isForwardKeyPressed = Input.GetKey(KeyCode.W);
        isReverseKeyPressed = Input.GetKey(KeyCode.S);
        isTurnLeftKeyPressed = Input.GetKey(KeyCode.A);
        isTurnRightKeyPressed = Input.GetKey(KeyCode.D);

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleEngine();
        }


        if (isForwardKeyPressed && !isReverseKeyPressed)
        {
            if (speed < -0.1f)
            {
                leftTrackRatio = 0f;
                rightTrackRatio = 0f;

                leftTrackBrakeRatio = 1f;
                rightTrackBrakeRatio = 1f;

                if (currentEngineRPM <= minEngineRPM * 1.5f)
                {
                    ShiftGear((short)(currentGear + 1));
                }
            }
            else
            {
                isClutchEngaged = false;

                if(isTurnLeftKeyPressed && !isTurnRightKeyPressed)
                {
                    leftTrackRatio = 0.5f;
                    rightTrackRatio = 1f;

                    leftTrackBrakeRatio = 0.2f;
                    rightTrackBrakeRatio = 0f;
                }
                else if (isTurnRightKeyPressed && !isTurnLeftKeyPressed)
                {
                    leftTrackRatio = 1f;
                    rightTrackRatio = 0.5f;

                    leftTrackBrakeRatio = 0f;
                    rightTrackBrakeRatio = 0.2f;
                }
                else
                {
                    leftTrackRatio = 1f;
                    rightTrackRatio = 1f;

                    leftTrackBrakeRatio = 0f;
                    rightTrackBrakeRatio = 0f;
                }

                if (currentGear == 0)
                {
                    ShiftGear(1);
                }
                else
                {
                    if (currentEngineRPM >= maxEngineRPM * 0.95f)
                    {
                        ShiftGear((short)(currentGear + 1));
                    }
                    else if (currentEngineRPM <= minEngineRPM * 1.5f && currentGear >= 2)
                    {
                        
                        ShiftGear((short)(currentGear - 1));
                    }

                }
            }

            

        }
        else if (isReverseKeyPressed && !isForwardKeyPressed)
        {
            if(speed > 0)
            {
                leftTrackRatio = 0f;
                rightTrackRatio = 0f;

                leftTrackBrakeRatio = 1f;
                rightTrackBrakeRatio = 1f;

                if (currentEngineRPM <= minEngineRPM * 1.5f)
                {
                    ShiftGear((short)(currentGear - 1));
                }
            }
            else
            {

                isClutchEngaged = false;

                if (isTurnLeftKeyPressed && !isTurnRightKeyPressed)
                {
                    leftTrackRatio = 0.5f;
                    rightTrackRatio = 1f;
                }
                else if (isTurnRightKeyPressed && !isTurnLeftKeyPressed)
                {
                    leftTrackRatio = 1f;
                    rightTrackRatio = 0.5f;
                }
                else
                {
                    leftTrackRatio = 1f;
                    rightTrackRatio = 1f;
                }

                leftTrackBrakeRatio = 0f;
                rightTrackBrakeRatio = 0f;

                if (currentGear == 0)
                {
                    ShiftGear(-1);
                }
                else
                {
                    if (currentEngineRPM >= maxEngineRPM * 0.95f)
                    {
                        ShiftGear((short)(currentGear - 1));
                    }
                    
                }
            }

            
        }
        else if (isTurnLeftKeyPressed && !isTurnRightKeyPressed)
        {
            if(Mathf.Abs(speed) >= 2)
            {
                leftTrackRatio = 0f;
                rightTrackRatio = 1f;

                leftTrackBrakeRatio = 0f;
                rightTrackBrakeRatio = 0f;
            }
            else
            {
                if(currentGear == 0)
                {
                    ShiftGear(1);
                }
                leftTrackRatio = -1f;
                rightTrackRatio = 1f;

                leftTrackBrakeRatio = 0f;
                rightTrackBrakeRatio = 0f;
            }
        }
        else if (isTurnRightKeyPressed && !isTurnLeftKeyPressed)
        {
            if (Mathf.Abs(speed) >= 2)
            {
                leftTrackRatio = 1f;
                rightTrackRatio = 0f;

                leftTrackBrakeRatio = 0f;
                rightTrackBrakeRatio = 0f;
            }
            else
            {
                if (currentGear == 0)
                {
                    ShiftGear(1);
                }
                leftTrackRatio = 1f;
                rightTrackRatio = -1f;

                leftTrackBrakeRatio = 0f;
                rightTrackBrakeRatio = 0f;
            }
        }
        else if (Mathf.Abs(speed) >= 2)
        {
            isClutchEngaged = false;
            leftTrackRatio = 0f;
            rightTrackRatio = 0f;

            leftTrackBrakeRatio = 0f;
            rightTrackBrakeRatio = 0f;


            if (currentEngineRPM <= minEngineRPM * 1.5f && currentGear != 0)
            {
                if (speed > 0)
                {
                    ShiftGear((short)(currentGear - 1));
                }
                else if (speed < 0)
                {
                    ShiftGear((short)(currentGear + 1));
                }
            }
        }
        else if (Mathf.Abs(speed) < 2)
        {
            isClutchEngaged = true;

            leftTrackRatio = 0f;
            rightTrackRatio = 0f;

            leftTrackBrakeRatio = 1f;
            rightTrackBrakeRatio = 1f;

            ShiftGear(0);

        }


    }
    private void CalculateEngineRPM()
    {
        if(!isEngineOn)
        {
            currentEngineRPM = 0;
            return;
        }
        else if (currentGear == 0 || isClutchEngaged)
        {
            currentEngineRPM = Mathf.Lerp(currentEngineRPM, minEngineRPM, Time.fixedDeltaTime * 2f);
        }
        else
        {
            // Calculate wheel RPM from vehicle speed
            float wheelCircumference = 2 * Mathf.PI * wheelRadius; // obwód koła 2πr
            float speedInMPS = Mathf.Abs(speed) / 3.6f;
            float wheelRPM = (speedInMPS / wheelCircumference) * 60f;

            float currentGearRatio = Mathf.Abs(GetCurrentGearRatio());
            float calculatedEngineRPM = wheelRPM * currentGearRatio * finalDriveRatio;
            calculatedEngineRPM = Mathf.Clamp(calculatedEngineRPM, minEngineRPM, maxEngineRPM);

            currentEngineRPM = calculatedEngineRPM;
        }
    }
    private void LimitRotateSpeed()
    {
        float dampingFactor = rotationSpeed / maxRotateSpeed;
        tankRigidbody.angularDamping *= dampingFactor;
        tankRigidbody.angularDamping = Mathf.Clamp(tankRigidbody.angularDamping, 1f, 100f);

    }
    private float GetCurrentGearRatio()
    {
        if (currentGear > 0 && currentGear <= forwardGears)
        {
            return forwardGearRatios[currentGear];
        }
        else if (currentGear < 0 && Mathf.Abs(currentGear) <= reverseGears)
        {
            return -reverseGearRatios[Mathf.Abs(currentGear) - 1];
        }
        return 0f;
    }
    private void ToggleEngine()
    {
        isEngineOn = !isEngineOn;
    }

    private void ApplyTrackForce(float leftTrackRatio, float rightTrackRatio)
    {
        if(currentEngineRPM < maxEngineRPM && !isClutchEngaged)
        {
            float torqueOnWheels = engineTorqueCurve.Evaluate(currentEngineRPM) * GetCurrentGearRatio() * finalDriveRatio;
            float forceOnWheels = torqueOnWheels / wheelRadius; //F = T / r
            tankRigidbody.AddForceAtPosition(transform.forward * forceOnWheels * leftTrackRatio, leftTrack.position, ForceMode.Force);
            tankRigidbody.AddForceAtPosition(transform.forward * forceOnWheels * rightTrackRatio, rightTrack.position, ForceMode.Force);
        }
    }
    private void ApplyBrakeForce(float leftTrackBrakeRatio, float rightTrackBrakeRatio)
    {
        tankRigidbody.AddForceAtPosition(
            brakeForce * leftTrackBrakeRatio * -tankRigidbody.linearVelocity.normalized,
            leftTrack.position,
            ForceMode.Force
            );
        tankRigidbody.AddForceAtPosition(
            brakeForce * rightTrackBrakeRatio * -tankRigidbody.linearVelocity.normalized,
            rightTrack.position,
            ForceMode.Force
            );

    }
    private void RotateDriveWheels()
    {
        if (rightWheels != null && leftWheels != null && Mathf.Abs(speed) > 0.1)
        {
            // Calculate wheel rotation speed based on actual vehicle speed
            float speedInMPS = Mathf.Abs(speed) / 3.6f; // Convert km/h to m/s
            float wheelCircumference = 2 * Mathf.PI * wheelRadius; // Wheel circumference
            float wheelRPM = (speedInMPS / wheelCircumference) * 60f; // Convert to RPM

            // Convert RPM to degrees per second
            float baseRotationSpeed = wheelRPM * 6f; // RPM * 360° / 60s = RPM * 6

            // Apply track ratios to each wheel
            float leftWheelRotationSpeed = baseRotationSpeed;
            float rightWheelRotationSpeed = baseRotationSpeed;

            // Apply rotation based on speed direction (forward/reverse)
            if (speed < 0)
            {
                leftWheelRotationSpeed = -leftWheelRotationSpeed;
                rightWheelRotationSpeed = -rightWheelRotationSpeed;
            }

            foreach (GameObject wheel in rightWheels)
            {
                wheel.transform.Rotate(rightWheelRotationSpeed * Time.fixedDeltaTime, 0, 0);
            }
            foreach (GameObject wheel in leftWheels)
            {
                wheel.transform.Rotate(leftWheelRotationSpeed * Time.fixedDeltaTime, 0, 0);
            }
        }
    }
    private float MaxSpeedOnCurrentGear()
    {
        if (currentGear == 0) return 0f;
        float wheelRPM = maxEngineRPM / (Mathf.Abs(GetCurrentGearRatio()) * finalDriveRatio);
        float wheelCircumference = 2 * Mathf.PI * wheelRadius; // obwód koła 2πr
        float maxSpeed = (wheelRPM * wheelCircumference) / 60f * 3.6f;
        return maxSpeed;
    }
    private void CheckForGearChange()
    {
        if(currentEngineRPM >= maxEngineRPM  * 0.95f)
        {
            ShiftGear((short)(currentGear + 1));
        }
        else if(currentEngineRPM <= minEngineRPM * 1.4f)
        {
            ShiftGear((short)(currentGear - 1));
        }
    }
    private void ShiftGear(short newGear)
    {
        if (!enableAutoShifting) return;
        if (isShifting || !isEngineOn) return;
        if (newGear == currentGear) return;
        if(newGear < 0)
        {
            if(Mathf.Abs(newGear) > reverseGears) return;
        }
        else
        {
            if(newGear > forwardGears) return;
        }

        StartCoroutine(ShiftGearCoroutine(newGear));
    }
    private IEnumerator ShiftGearCoroutine(short newGear)
    {
        isShifting = true;
        isClutchEngaged = true;
        float elapsedTime = 0f;
        float initialRPM = currentEngineRPM;
        while (elapsedTime < shiftChangingTime)
        {
            isClutchEngaged = true;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        currentGear = newGear;
        isClutchEngaged = false;
        isShifting = false;
    }
}
