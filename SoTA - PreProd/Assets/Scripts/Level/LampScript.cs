using UnityEngine;
using System.Collections;
using UnityEditor;
/// <summary>
/// Author:Karin
/// 
/// Modified by:
/// 
/// </summary>
public class LampScript : MonoBehaviour, IActivatable

{
    [field: SerializeField] public bool IsLit { get; private set; } = false;
    public bool StartsAsActive {get; private set; } //is used to determine which SFX should be played when lamp is activated with button
    
    private LightTracker tracker;
    private ParticleSystem lampParticles;

    void Start()
    {
        tracker = GameObject.Find("RadialColorManager").GetComponent<LightTracker>();
        tracker.RegisterLightSource(transform);
        lampParticles = GetComponentInChildren<ParticleSystem>();
        FindParticleColor();

        StartsAsActive = IsLit;
    }

    public void Activate()
    {
        if (!IsLit)
        {
            TurnOnLamp();
        }
    }

    private void TurnOnLamp()
    {
        IsLit = true;
        tracker.RefreshLightSources();
        StartCoroutine(PlayAndStopParticleBurst());
    }

    public void Deactivate()
    {
        if (IsLit)
        {
            TurnOffLamp();
        }
    }

    private void TurnOffLamp()
    {
        IsLit = false;
        tracker.RefreshLightSources();
        StartCoroutine(PlayAndStopParticleBurst());
    }

    public void Interact()
    {
        if (!IsLit)
        {
            TurnOnLamp();
        } else if (IsLit)
        {
            TurnOffLamp();
        }
    }

    public void Interact(bool byStarThrow)
    {
        if (byStarThrow)
        {
            if (!IsLit)
            {
                TurnOnLamp();
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LampTurnOnSFX, Vector3.zero);
            }
            else if (IsLit)
            {
                TurnOffLamp();
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LampTurnOffSFX, Vector3.zero);
            }
        }
    }

    IEnumerator PlayAndStopParticleBurst()
    {
        lampParticles.Play();
        yield return new WaitForSeconds(0.1f); // wait for particles to spawn
        lampParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void FindParticleColor()
    {
        Transform lightSource = transform.Find("Lamp/Light_source");
        Transform lampParticlesTransform = transform.Find("LampParticles");

        if (lightSource != null && lampParticlesTransform != null)
        {
            MeshRenderer sourceRenderer = lightSource.GetComponent<MeshRenderer>();
            ParticleSystemRenderer psRenderer = lampParticlesTransform.GetComponent<ParticleSystemRenderer>();

            if (sourceRenderer != null && psRenderer != null)
            {
                psRenderer.material = sourceRenderer.sharedMaterial;
            }
            else
            {
                Debug.LogWarning("MeshRenderer or ParticleSystemRenderer not found.");
            }
        }
        else
        {
            Debug.LogError("Light_source or LampParticles not found in the hierarchy.");
        }
    }
}
