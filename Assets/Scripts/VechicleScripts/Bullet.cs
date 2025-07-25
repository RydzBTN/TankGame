using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour
{
    [Header("Statystyki pocisku")]
    public float startVelocity = 1700f;       // Prędkość początkowa w m/s
    public float penetratorMass = 5f;         // Masa penetratora w kg
    public float penetratorDiameter = 0.03f;  // Średnica penetratora w metrach
    public float dragCoefficient = 0.05f;     // Współczynnik oporu powietrza

    [Header("Zdolność penetracyjna")]
    public float basePenetrationPower = 600f; // Bazowa penetracja w mm RHA
    public float criticalAngle = 80f;         // Kąt przy którym pocisk się odbija (w stopniach)

    [Header("Pomiar prędkości")]
    public float currentSpeed;                // Aktualna prędkość w m/s
    public float distanceTraveled;            // Przebyty dystans w metrach

    [Header("Efekty")]
    public GameObject penetrationEffect;      // Efekt przebicia pancerza
    public GameObject ricochetEffect;         // Efekt rykoszetu
    public GameObject impactEffect;           // Standardowy efekt uderzenia
    public GameObject trailEffect;            // Efekt śladu balistycznego

    [Header("Zaawansowane czynniki fizyczne")]
    public bool enableMagnusEffect = false;   // Efekt Magnusa
    public float spinRate = 0f;               // Prędkość obrotowa pocisku (RPM)
    public bool enableGyroscopicDrift = false; // Precesja żyroskopowa i dryf
    public bool enableCoriolisEffect = false; // Efekt Coriolisa (rotacja Ziemi)
    public float ballisticCoefficient = 0.4f; // Współczynnik balistyczny (G1 model)

    [Header("Czynniki środowiskowe")]
    public Vector3 windDirection = Vector3.zero; // Kierunek wiatru (wektor)
    public float windStrength = 0f;           // Siła wiatru (m/s)
    public float airDensity = 1.225f;         // Gęstość powietrza (kg/m^3) na poziomie morza
    public float temperature = 20f;           // Temperatura powietrza (°C)
    public float humidity = 0.5f;             // Wilgotność względna (0-1)
    public float altitude = 0f;               // Wysokość nad poziomem morza (m)

    [Header("Parametry trajektorii")]
    public Vector3 initialPosition;           // Pozycja początkowa dla śledzenia trajektorii
    public bool useRealisticDropoff = true;   // Użyj realistycznego spadku prędkości
    public bool logTrajectoryData = false;    // Zapisywanie danych o trajektorii

    private Rigidbody rigidBody;
    private Vector3 lastPosition;
    private float crossSectionalArea;
    private GameObject activeTrailEffect;
    private float speedOfSound;
    private Vector3 earthRotationAxis = Vector3.up; // Oś obrotu Ziemi (uproszczenie)
    private float earthRotationSpeed = 0.0000727f;  // Prędkość obrotowa Ziemi (rad/s)
    private float timeInFlight = 0f;

    private Vector3[] trajectoryPoints;
    private int trajectoryPointIndex = 0;
    private int maxTrajectoryPoints = 100;

    void Start()
    {
        // Inicjalizacja punktów trajektorii
        if (logTrajectoryData)
        {
            trajectoryPoints = new Vector3[maxTrajectoryPoints];
            initialPosition = transform.position;
        }

        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Pobierz globalne ustawienia pogodowe
        airDensity = GetAirDensity(altitude);
        windDirection = GetWindDirection();
        windStrength = GetWindStrength();
        temperature = GetTemperature(altitude);
        humidity = GetHumidity();
        altitude = transform.position.y; // Ustawienie wysokości pocisku na aktualną pozycję

        rigidBody.mass = penetratorMass;
        rigidBody.linearDamping = 0; // Wyłączamy domyślny opór, implementujemy własny model
        rigidBody.angularDamping = 0;

        // Ustawienie prędkości początkowej
        rigidBody.linearVelocity = transform.forward * startVelocity;

        // Obliczenie przekroju poprzecznego
        crossSectionalArea = Mathf.PI * Mathf.Pow(penetratorDiameter / 2, 2);

        lastPosition = transform.position;

        // Utworzenie śladu balistycznego
        if (trailEffect != null)
        {
            activeTrailEffect = Instantiate(trailEffect, transform.position, Quaternion.identity);
            activeTrailEffect.transform.parent = this.transform;
        }

        // Dostosuj prędkość dźwięku do temperatury (przybliżenie)
        speedOfSound = 331.3f + (0.606f * temperature);

       
    }

    private void FixedUpdate()
    {
        if (rigidBody == null) return;

        // Zliczanie czasu lotu dla efektów zależnych od czasu
        timeInFlight += Time.fixedDeltaTime;

        // Pomiar aktualnej prędkości
        currentSpeed = rigidBody.linearVelocity.magnitude;
        Vector3 moveDirection = rigidBody.linearVelocity.normalized;

        // Aktualizacja gęstości powietrza na podstawie wysokości (jeśli się zmienia)
        if (altitude != transform.position.y)
        {
            altitude = transform.position.y;
            airDensity = GetAirDensity(altitude);
        }

        // Opór powietrza - wykorzystujemy współczynnik balistyczny i model G1
        if (useRealisticDropoff)
        {
            ApplyRealisticDrag();
        }
        else
        {
            // Uproszczony model oporu powietrza
            float dragForce = 0.5f * dragCoefficient * airDensity * rigidBody.linearVelocity.sqrMagnitude * crossSectionalArea;
            rigidBody.AddForce(-moveDirection * dragForce);
        }

        // Aktualizacja przebytego dystansu
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // Efekt wiatru - uwzględniamy wysokość (wiatr silniejszy na większych wysokościach)
        if (windStrength > 0)
        {
            // Wpływ wiatru zależy od prędkości pocisku (słabszy przy większych prędkościach)
            float windFactor = Mathf.Clamp01(1.0f / (currentSpeed * 0.01f));
            // Dodatkowy mnożnik zwiększający wpływ wiatru z wysokością
            float altitudeMultiplier = 1.0f + (altitude / 1000f) * 0.1f;

            rigidBody.AddForce(windDirection.normalized * windStrength * windFactor * altitudeMultiplier, ForceMode.Acceleration);
        }

        // Efekt Magnusa (siła działająca prostopadle do kierunku ruchu i osi obrotu)
        if (enableMagnusEffect && spinRate > 0)
        {
            Vector3 magnusDirection = Vector3.Cross(transform.forward, rigidBody.linearVelocity).normalized;
            float magnusFactor = spinRate * 0.0001f * currentSpeed * airDensity;
            rigidBody.AddForce(magnusDirection * magnusFactor);
        }

        // Precesja żyroskopowa i dryf (subtelne odchylenie od idealnego toru)
        if (enableGyroscopicDrift && currentSpeed > 100f)
        {
            float driftMagnitude = 0.01f * Mathf.Sin(timeInFlight * 2f) * (distanceTraveled * 0.001f);
            Vector3 driftDirection = Vector3.Cross(transform.forward, Vector3.up).normalized;
            rigidBody.AddForce(driftDirection * driftMagnitude);
        }

        // Efekt Coriolisa (wpływ rotacji Ziemi)
        if (enableCoriolisEffect && distanceTraveled > 1000f)
        {
            // Uproszczony model efektu Coriolisa
            Vector3 coriolisAcceleration = Vector3.Cross(2f * earthRotationAxis * earthRotationSpeed, rigidBody.linearVelocity);
            rigidBody.AddForce(coriolisAcceleration, ForceMode.Acceleration);
        }

        // Utrzymanie orientacji pocisku w kierunku ruchu (efekt stabilizacji brzechwowej)
        if (rigidBody.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation,
                                               Quaternion.LookRotation(rigidBody.linearVelocity),
                                               Time.deltaTime * 10f);
        }

        // Grawitacja - możemy użyć niestandardowej wartości jeśli BallisticsManager jest dostępny
        rigidBody.AddForce(new Vector3(0, -9.82f, 0), ForceMode.Acceleration);

        

        // Zapisywanie punktów trajektorii
        if (logTrajectoryData && trajectoryPointIndex < maxTrajectoryPoints)
        {
            trajectoryPoints[trajectoryPointIndex] = transform.position;
            trajectoryPointIndex++;

            // Opcjonalnie możemy rysować linię trajektorii dla celów debugowania
            if (trajectoryPointIndex > 1)
            {
                Debug.DrawLine(
                    trajectoryPoints[trajectoryPointIndex - 2],
                    trajectoryPoints[trajectoryPointIndex - 1],
                    Color.red,
                    10f
                );
            }
        }

        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        Debug.Log("Pocisk zderzył się z obiektem: " + collision.gameObject.name);
    }   

    
    // Aplikowanie realistycznego oporu powietrza z użyciem współczynnika balistycznego
    private void ApplyRealisticDrag()
    {
        // Model G1 dla współczynnika oporu - zależy od prędkości pocisku 
        // względem prędkości dźwięku (liczba Macha)
        float machNumber = currentSpeed / speedOfSound;
        float cd = CalculateDragCoefficientG1(machNumber);

        // Obliczenie siły oporu z użyciem współczynnika balistycznego
        float dragForce = (cd * airDensity * currentSpeed * currentSpeed * crossSectionalArea) / (2f * ballisticCoefficient);

        // Zastosowanie siły oporu w kierunku przeciwnym do ruchu
        rigidBody.AddForce(-rigidBody.linearVelocity.normalized * dragForce);
    }

    // Obliczenie współczynnika oporu dla modelu G1 (standardowy model balistyczny)
    private float CalculateDragCoefficientG1(float mach)
    {
        // Uproszczona implementacja współczynnika oporu G1
        if (mach < 0.8f)
            return 0.2f; // Prędkość poddźwiękowa
        else if (mach < 1.1f)
            return 0.25f + (mach - 0.8f) * 0.5f; // Przejście przez barierę dźwięku
        else if (mach < 2f)
            return 0.4f - (mach - 1.1f) * 0.1f; // Prędkość naddźwiękowa
        else
            return 0.3f - (mach - 2f) * 0.05f; // Wysoka prędkość naddźwiękowa
    }

    private float GetAirDensity(float altitude)
    {
        // Uproszczony model gęstości powietrza w zależności od wysokości
        return Mathf.Max(1.225f * Mathf.Pow(1 - (altitude / 44330f), 4.256f), 0.001f);
    }
    private Vector3 GetWindDirection()
    {
        // Uproszczony model kierunku wiatru (można rozszerzyć o dynamiczne źródła)
        return new Vector3(1, 0, 0); // Przykładowy kierunek wiatru
    }
    private float GetWindStrength()
    {
        // Uproszczony model siły wiatru (można rozszerzyć o dynamiczne źródła)
        return 5f; // Przykładowa siła wiatru w m/s
    }
    private float GetTemperature(float altitude)
    {
        // Uproszczony model temperatury w zależności od wysokości
        return Mathf.Max(20f - (altitude / 1000f) * 6.5f, -50f); // Przykładowa temperatura w °C
    }
    private float GetHumidity()
    {
        // Uproszczony model wilgotności (można rozszerzyć o dynamiczne źródła)
        return 0.5f; // Przykładowa wilgotność względna (0-1)
    }



}
