using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Globalne ustawienia fizyczne")]
    [Tooltip("Siła grawitacji w m/s²")]
    public Vector3 gravity = new Vector3(0, -9.81f, 0); // Standardowa grawitacja

    [Header("Warunki atmosferyczne")]
    [Tooltip("Temperatura powietrza na poziomie morza (°C)")]
    public float temperature = 20f;

    [Tooltip("Ciśnienie atmosferyczne na poziomie morza (hPa)")]
    public float pressure = 1013.25f;

    [Tooltip("Wilgotność względna (0-1)")]
    public float humidity = 0.5f;

    [Header("Wiatr")]
    [Tooltip("Kierunek wiatru")]
    public Vector3 windDirection = Vector3.right;

    [Tooltip("Siła wiatru w m/s")]
    public float windStrength = 0f;

    [Tooltip("Zmienność kierunku wiatru")]
    [Range(0f, 1f)]
    public float windVariability = 0.1f;

    [Tooltip("Zmienność siły wiatru")]
    [Range(0f, 1f)]
    public float windGustiness = 0.2f;

    [Header("Globalne ustawienia symulacji")]
    [Tooltip("Skala czasu dla symulacji balistycznej")]
    [Range(0.1f, 10f)]
    public float timeScale = 1.0f;

    [Tooltip("Czy uwzględniać efekt Coriolisa (rotacji Ziemi)")]
    public bool enableCoriolisEffect = false;

    [Tooltip("Maksymalny zasięg symulacji (w metrach)")]
    public float maxSimulationDistance = 10000f;

    // Zmienne prywatne dla obliczeń
    private Vector3 initialWindDirection;
    private float initialWindStrength;

    // Stałe atmosferyczne
    private const float AIR_DENSITY_SEA_LEVEL = 1.225f; // kg/m³ przy 15°C
    private const float TEMPERATURE_LAPSE_RATE = 0.0065f; // °C/m (spadek temperatury z wysokością)
    private const float GAS_CONSTANT = 287.05f; // J/(kg*K) dla suchego powietrza

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        initialWindDirection = windDirection.normalized;
        initialWindStrength = windStrength;

        Time.timeScale = timeScale;
    }

    public Vector3 GetGravity()
    {
        return gravity;
    }

    //gęstość powietrza na danej wysokości z użyciem modelu atmosfery standardowej
    public float GetAirDensity(float altitude)
    {
        float tempAtAltitude = temperature - (TEMPERATURE_LAPSE_RATE * altitude);
        float tempKel = tempAtAltitude + 273.15f;

        float exponent = (gravity.magnitude * altitude) / (GAS_CONSTANT * tempKel);
        float pressureAtAltitude = pressure * Mathf.Exp(-exponent);

        float airDensity = pressureAtAltitude / (GAS_CONSTANT * tempKel);
        return airDensity;
    }

    public Vector3 GetWindDirection()
    {
        return windDirection.normalized;
    }

    public float GetWindStrength()
    {
        return windStrength;
    }

    public float GetTemperature(float altitude)
    {
        // Obliczenie temperatury na danej wysokości (model liniowy)
        return temperature - (TEMPERATURE_LAPSE_RATE * altitude);
    }

    public float GetHumidity()
    {
        return humidity;
    }

    public float GetSpeedOfSound()
    {
        // Przybliżona formuła dla prędkości dźwięku w suchym powietrzu
        return 331.3f + (0.606f * temperature);
    }



}
