using System;
using UnityEngine;

/// <summary>
/// Attach this to GameObject that can burn. Note that this class works with temperature in Kelvin.
/// Just to remind: T_Kelvin = T_Celcius + ABSOLUTE_ZERO_CELCIUS. The class has a constant to avoid
/// needing to remember.
/// </summary>
public class Combustible : MonoBehaviour
{
    [Flags]
    public enum CombustibleKind
    {
        A_NonMetalSolid = 1 << 0,
        B_Liquid = 1 << 1,
        C_Gas = 1 << 2,
        D_Metal = 1 << 3,
        E_Electrical = 1 << 4,
        F_Cooking = 1 << 5
    }


    public const float ABSOLUTE_ZERO_CELCIUS = 273.15f;

    [SerializeField] private ParticleSystem firePrefab = null;
    ParticleSystem fire = null;
    CombustibleKind combustibleKind = CombustibleKind.A_NonMetalSolid;

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
            fule -= 0.001f * Time.deltaTime;
        }
        if (fire != null && (!haveFule || temperature < autoIgnitionTemperature))
        {
            fire.Stop();
        }
    }

    void Ignite()
    {
        if (fire == null) 
        {
            fire = Instantiate(firePrefab, transform);
            fire.transform.localPosition = Vector3.zero;
        } 
        fire.Play();
    }

    // TODO: This can be made more complex.
    void Extinguish(CombustibleKind agentWorksOn, float quantity_L)
    {
        if ((agentWorksOn & combustibleKind) > 0)
        {
            Temperature -= quantity_L;
        }
    }
}
