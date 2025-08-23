using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:Gabbriel, Karin
/// 
/// Modified by:
/// 
/// </summary>
public class PressurePlateScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> puzzleElements = new List<GameObject>();
    [SerializeField] private float sinkAmount = 0.02f;

    private List<GameObject> objectsOnPlate = new List<GameObject>();
    private bool isPushedDown = false;
    private Vector3 originalPosition;

    private ParticleSystem pressurePlateParticles;

    private EventInstance pressurePlateSFX;

    //flags used for SFX so we only play spike sound effect (for example) once for all spikes being activated by this button
    private bool isConnectedToSpikes = false; //is set automatically in CheckConnectedPuzzleElements
    private bool spikesStartAsActive = false; //is set automatically in CheckConnectedPuzzleElements
    private bool isConnectedToLamps = false; //is set automatically in CheckConnectedPuzzleElements
    private bool lampsStartAsActive = false; //is set automatically in CheckConnectedPuzzleElements

    private void Start()
    {
        originalPosition = transform.position;
        pressurePlateParticles = GetComponentInChildren<ParticleSystem>();
        FindParticleColor();
        pressurePlateSFX = AudioManager.Instance.CreateInstance(FMODEvents.Instance.PressurePlateSFX);
        CheckConnectedPuzzleElements();
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") || other.CompareTag("Star") || other.CompareTag("Boulder"))
        {
            objectsOnPlate.Add(other.gameObject);
        }

        if (objectsOnPlate.Count > 0)
        {

            if (!isPushedDown)
            {
                isPushedDown = true;
                transform.position = originalPosition + Vector3.down * sinkAmount;

                pressurePlateSFX.setParameterByNameWithLabel("PressurePlateState", "PushDown");
                pressurePlateSFX.start();
                Interact();
                PlayActivationSFX();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Star") || other.CompareTag("Boulder"))
        {
            objectsOnPlate.Remove(other.gameObject);
        } 
        else if (other.CompareTag("Untagged"))
        {
            return;
        }

        if (objectsOnPlate.Count <= 0)
        {
            isPushedDown = false;
            transform.position = originalPosition;

            pressurePlateSFX.setParameterByNameWithLabel("PressurePlateState", "PushUp");
            pressurePlateSFX.start();


            PlayDeactivationSFX();

            Interact();
        }
    }

    public void Interact()
    {
        foreach (GameObject puzzleElement in puzzleElements)
        {
            IActivatable activatable = puzzleElement.GetComponent<IActivatable>();
            if (activatable != null)
            {
                if (isPushedDown)
                {
                    StartCoroutine(PlayAndStopParticleBurst());
                    activatable.Activate();
                }
                else
                {
                    StartCoroutine(PlayAndStopParticleBurst());
                    activatable.Deactivate();
                }
            }
        }
    }

    IEnumerator PlayAndStopParticleBurst()
    {
        pressurePlateParticles.Play();
        yield return new WaitForSeconds(0.1f); // wait for particles to spawn
        pressurePlateParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void FindParticleColor()
    {
        Transform pressurePlate = transform.Find("PressurePlate");
        Transform pressurePlateParticlesTransform = transform.Find("PressurePlateParticles");

        if (pressurePlate != null && pressurePlateParticlesTransform != null)
        {
            MeshRenderer sourceRenderer = pressurePlate.GetComponent<MeshRenderer>();
            ParticleSystemRenderer psRenderer = pressurePlateParticlesTransform.GetComponent<ParticleSystemRenderer>();

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
            Debug.LogError("pressurePlate or PressurePlateParticles not found in the hierarchy.");
        }
    }

    private void CheckConnectedPuzzleElements() //Only to be used for SFX
    {
        //SPIKES
        isConnectedToSpikes = false;

        foreach (GameObject element in puzzleElements)
        {
            if (element.gameObject.CompareTag("Spikes"))
            {
                isConnectedToSpikes = true;

                if (element.gameObject.GetComponent<SpikeScript>().StartsAsActive)
                {
                    spikesStartAsActive = true;
                }

                break; //for the purposes of SFX we only care about the first spike, so we break the loop here
            }
        }

        //LAMPS
        isConnectedToLamps = false;

        foreach (GameObject element in puzzleElements)
        {
            if (element.gameObject.CompareTag("Lamp"))
            {
                isConnectedToLamps = true;

                if (element.gameObject.GetComponent<LampScript>().StartsAsActive)
                {
                    lampsStartAsActive = true;
                }

                break; //for the purposes of SFX we only care about the first lamp, so we break the loop here
            }
        }
    }

    private void PlayActivationSFX()
    {
        if (isConnectedToSpikes)
        {
            if (spikesStartAsActive)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SpikesDisappearSFX);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SpikesAppearSFX);
            }
        }

        if (isConnectedToLamps)
        {
            if (lampsStartAsActive)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LampTurnOffSFX);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LampTurnOnSFX);
            }
        }
    }

    private void PlayDeactivationSFX()
    {
        if (isConnectedToSpikes)
        {
            if (spikesStartAsActive)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SpikesAppearSFX);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SpikesDisappearSFX);
            }
        }

        if (isConnectedToLamps)
        {
            if (lampsStartAsActive)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LampTurnOnSFX);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LampTurnOffSFX);
            }
        }
    }
}
