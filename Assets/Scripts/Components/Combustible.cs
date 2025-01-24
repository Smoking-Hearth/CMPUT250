using System;
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

// TODO: Fule should produce heat. As of the moment it doesn't. Add some parameters to represent
// how much energy different types of fule store and how much heat they can generate.

// TODO: Fule burns at different speeds. Add that.

/// <summary>
/// Attach this to GameObject that can burn. Note that this class works with temperature in Kelvin.
/// Just to remind: T_Kelvin = T_Celcius + ABSOLUTE_ZERO_CELCIUS. The class has a constant to avoid
/// needing to remember.
/// </summary>
public class Combustible : MonoBehaviour, IExtinguishable
{
    public const float ABSOLUTE_ZERO_CELCIUS = 273.15f;
    public const float MAX_TEMP = 20_000f;

    [SerializeField] private ParticleSystem firePrefab = null;
    ParticleSystem fire = null;
    CombustibleKind combustibleKind = CombustibleKind.A_COMMON;

    [SerializeField] AnimationCurve extinguishEffectiveness = AnimationCurve.Constant(0f, MAX_TEMP, 1f);

    public bool Burning
    {
        get { return fire != null && fire.isPlaying; }
    }
    
    // This is in Kelvin because it makes things nice.
    [SerializeField] float temperature = 0f;
    public float Temperature 
    {
        get { return temperature; }    
        set 
        {
            temperature = Mathf.Max(value, 0f);
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

    float fule = 100f;
    public float Fule 
    {
        get { return fule; }
        set 
        {
            fule = Mathf.Max(value, 0f);
        }
    }

    void Start()
    {
        if (firePrefab == null)
        {
            Debug.Log("Missing fire prefab.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool haveFule = fule > 0f;
        if (temperature > autoIgnitionTemperature && haveFule)
        {
            Ignite();
        }
        if (Burning)
        {
            fule -= 0.1f * Time.deltaTime;
        }
        if (fire != null && (!haveFule || temperature < autoIgnitionTemperature))
        {
            fire.Stop();
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
