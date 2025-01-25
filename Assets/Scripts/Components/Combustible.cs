using System;
using System.Collections;
using UnityEngine;

[Flags]
public enum CombustibleKind
{
    A_COMMON = 1 << 0,
    B_LIQUID = 1 << 1,
    C_ELECTRICAL = 1 << 2,
    D_METAL = 1 << 3,
    K_COOKING = 1 << 4
}

// TODO: Combustible materials should check their surroundings for other combustible materials. 
// Given these the combustible should update it's temperature according to the environment.

// TODO: Fule burns at different speeds. Add that.

/// <summary>
/// Attach this to GameObject that can burn. Note that this class works with temperature in Kelvin.
/// Just to remind: T_Kelvin = T_Celcius + ABSOLUTE_ZERO_CELCIUS. The class has a constant to avoid
/// needing to remember.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))] 
public class Combustible : MonoBehaviour, IExtinguishable
{
    public const float ABSOLUTE_ZERO_CELCIUS = 273.15f;
    public const float MAX_TEMP = 20_000f;

    [Header("Visual")]
    [SerializeField] ParticleSystem firePrefab = null;
    ParticleSystem fire = null;
    CombustibleKind combustibleKind = CombustibleKind.A_COMMON;

    [SerializeField] AnimationCurve temperatureToLifetime = AnimationCurve.Constant(0f, MAX_TEMP, 1f);
    [SerializeField] float maxLifetime;

    [SerializeField] AnimationCurve extinguishEffectiveness = AnimationCurve.Constant(0f, MAX_TEMP, 1f);

    public bool Burning
    {
        get { return fire != null && fire.isPlaying; }
    }
    
    // This is in Kelvin because it makes things nice.
    [Header("Temperature")]
    [SerializeField] float temperature = 0f;
    public float Temperature 
    {
        get { return temperature; }    
        set 
        {
            temperature = Mathf.Clamp(value, 0f, MAX_TEMP);
        }
    }

    [SerializeField] float autoIgnitionTemperature = 373.15f;
    public float AutoIgnitionTemperature 
    {
        get { return autoIgnitionTemperature; }
        set 
        {
            autoIgnitionTemperature = Mathf.Max(value, 0f);
        }
    }

    
    [Tooltip("Units are Kelvin/second")]
    [SerializeField] float heatCopyRate = 1f;

    [Header("Fule")]
    [SerializeField] float fule = 100f;
    public float Fule 
    {
        get { return fule; }
        set 
        {
            fule = Mathf.Max(value, 0f);
        }
    }

    // In Kelvin per unit of fule
    [SerializeField] float fuleToTemp = 10f;

    void Start()
    {
        if (firePrefab == null)
        {
            Debug.Log("Missing fire prefab.");
        }
    }

    void Update()
    {
        bool haveFule = fule > 0f;
        if (temperature > autoIgnitionTemperature && haveFule)
        {
            Ignite();
        }
        if (Burning)
        {
            float consumed = Mathf.Min(0.1f * Time.deltaTime, fule);
            fule -= consumed;
            Temperature += fuleToTemp * consumed;

            ParticleSystem.MainModule main = fire.main;
            main.startLifetime = temperatureToLifetime.Evaluate(Temperature) * maxLifetime;
        }
        if (fire != null && (temperature < autoIgnitionTemperature))
        {
            fire.Stop();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Combustible neighboor = other.GetComponent<Combustible>();
        if (neighboor != null)
        {
            float diff = neighboor.Temperature - temperature;
            if (diff > 0f)
            {
                Temperature += diff * heatCopyRate * Time.deltaTime;
            }
        }
    }

    public void Ignite()
    {
        if (Burning) return;
        if (fire == null) 
        {
            fire = Instantiate(firePrefab, transform);
            fire.transform.localPosition = Vector3.zero;
        } 
        fire.Play();
    }

    public void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if (!Burning) return;
        if ((extinguishClass & combustibleKind) > 0)
        {
            Temperature -= quantity_L * extinguishEffectiveness.Evaluate(Mathf.Min(temperature - autoIgnitionTemperature, MAX_TEMP));
        }
    }
}
