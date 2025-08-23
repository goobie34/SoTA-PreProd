using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
/// <summary>
/// Author:Karin, Gabbriel, Linus
/// 
/// Modified by:
/// 
/// </summary>
public class ButtonScript : MonoBehaviour, IInteractable
{
    [SerializeField] private List<GameObject> puzzleElements = new List<GameObject>();
    [SerializeField] private bool hasTimer = false;
    [SerializeField] private float totalTimerDuration = 3;

    private Transform button;
    private bool isPushed = false;
    private bool isTimerRunning = false;

    private EventInstance buttonSFX;
    private EventInstance timerTickingSFX;

    private ParticleSystem buttonParticles;

    //flags used for SFX so we only play spike sound effect (for example) once for all spikes being activated by this button
    private bool isConnectedToSpikes = false; //is set automatically in CheckConnectedPuzzleElements
    private bool spikesStartAsActive = false; //is set automatically in CheckConnectedPuzzleElements
    private bool isConnectedToLamps = false; //is set automatically in CheckConnectedPuzzleElements
    private bool lampsStartAsActive = false; //is set automatically in CheckConnectedPuzzleElements
    private bool isConnectedToAntiStarZones = false; //is set automatically in CheckConnectedPuzzleElements
    private bool antiStarZonesStartAsActive = false; //is set automatically in CheckConnectedPuzzleElements

    private bool isConnectedToWalls = false; //is set automatically in CheckConnectedPuzzleElement
    EventInstance wallMoveSFXInstance;


    public bool IsActive { get { return isPushed; } }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    public void Start()
    {
        button = FindChildByName(transform, "Button_connection");
        buttonParticles = GetComponentInChildren<ParticleSystem>();
        FindParticleColor();

        if (button == null)
        {
            Debug.LogError("Button_connection child not found! Check the hierarchy.");
        }

        buttonSFX = AudioManager.Instance.CreateInstance(FMODEvents.Instance.ButtonSFX);
        timerTickingSFX = AudioManager.Instance.CreateInstance(FMODEvents.Instance.TimerTickingSFX);

        CheckConnectedPuzzleElements(); //is used only for SFX
    }

    public void Interact()
    {
        if (isPushed && isTimerRunning)
        {
            buttonSFX.setParameterByNameWithLabel("ButtonPushState", "PushFail");
            buttonSFX.start();

            return; //we busy
        }

        if (!isPushed && !hasTimer)
        {
            buttonSFX.setParameterByNameWithLabel("ButtonPushState", "PushDown");

            ActivateAllPuzzleElements();
            isPushed = true;
            FlipButtonDown();
            PlayActivationSFX();
        }
        else if (!isPushed && hasTimer)
        {
            buttonSFX.setParameterByNameWithLabel("ButtonPushState", "PushDown");
            timerTickingSFX.start();

            StartTimerForAllPuzzleElements();
            isPushed = true;
            FlipButtonDown();
            PlayActivationSFX();
        }
        else if (isPushed && !isTimerRunning)
        {
            buttonSFX.setParameterByNameWithLabel("ButtonPushState", "PushUp");

            DeactivateAllPuzzleElements();
            isPushed = false;
            FlipButtonUp();
            PlayDeactivationSFX();
        }

        buttonSFX.start();
    }

    private void ActivateAllPuzzleElements()
    {
        foreach (GameObject puzzleElement in puzzleElements)
        {
            IActivatable activatable = puzzleElement.GetComponent<IActivatable>();

            if (activatable == null)
            {
                continue;
            }

            activatable.Activate();
        }
    }
    private void DeactivateAllPuzzleElements()
    {
        foreach (GameObject puzzleElement in puzzleElements)
        {
            IActivatable activatable = puzzleElement.GetComponent<IActivatable>();

            if (activatable == null)
            {
                continue;
            }

            activatable.Deactivate();
        }
    }
    
    private void StartTimerForAllPuzzleElements()
    {
        if (isTimerRunning)
        {
            return;
        }

        isTimerRunning = true;
        
        foreach (GameObject puzzleElement in puzzleElements)
        {
            IActivatable activatable = puzzleElement.GetComponent<IActivatable>();

            if (activatable == null)
            {
                continue;
            }

            activatable.Activate();
        }
        
        StartCoroutine(DeactivateAllDelayed());
    }

    private IEnumerator DeactivateAllDelayed()
    {
        yield return new WaitForSeconds(totalTimerDuration);

        foreach (GameObject puzzleElement in puzzleElements)
        {
            IActivatable activatable = puzzleElement.GetComponent<IActivatable>();

            if (activatable == null)
            {
                continue;
            }

            activatable.Deactivate();
        }

        isPushed = false;
        isTimerRunning = false;

        FlipButtonUp();

        timerTickingSFX.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        buttonSFX.setParameterByNameWithLabel("ButtonPushState", "PushUp");
        buttonSFX.start();
        PlayDeactivationSFX();
    }
    private void ToggleButtonState()
    {
        isPushed = !isPushed;
    }

    private void FlipButtonDown()
    {
        button.localRotation = Quaternion.Euler(180, 0, 0);
        StartCoroutine(PlayAndStopParticleBurst());
    }

    private void FlipButtonUp()
    {
        button.localRotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(PlayAndStopParticleBurst());
    }
    public void SetState(bool Active)
    {
        if (hasTimer)
        {
            if (isTimerRunning)
            {
                DeactivateAllPuzzleElements();
                FlipButtonUp();
                isTimerRunning = false;
                isPushed = false;
                timerTickingSFX.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                StopAllCoroutines();
            }
            return;
        }
        if (Active != isPushed)
        {
            isPushed = Active;
            if (Active)
            {
                FlipButtonDown();
                ActivateAllPuzzleElements();
            }
            else
            {
                FlipButtonUp();
                DeactivateAllPuzzleElements();
            }
        }
    }

    IEnumerator PlayAndStopParticleBurst()
    {
        buttonParticles.Play();
        yield return new WaitForSeconds(0.1f); // wait for particles to spawn
        buttonParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void FindParticleColor()
    {
        Transform buttonConnection = transform.Find("Button/Button_connection");
        Transform buttonParticlesTransform = transform.Find("ButtonParticles");

        if (buttonConnection != null && buttonParticlesTransform != null)
        {
            MeshRenderer sourceRenderer = buttonConnection.GetComponent<MeshRenderer>();
            ParticleSystemRenderer psRenderer = buttonParticlesTransform.GetComponent<ParticleSystemRenderer>();

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
            Debug.LogError("Button_connection or ButtonParticles not found in the hierarchy.");
        }
    }


    private void CheckConnectedPuzzleElements() //Only to be used for SFX
    {
        //SPIKES
        isConnectedToSpikes = false;

        foreach(GameObject element in puzzleElements)
        {
            if (element.gameObject.CompareTag("Spikes"))
            {
                isConnectedToSpikes = true;

                if(element.gameObject.GetComponent<SpikeScript>().StartsAsActive)
                {
                    spikesStartAsActive = true;
                }

                break; //for the purposes of SFX we only care about the first spike, so we break the loop here
            }
        }
        
        //LAMPS
        isConnectedToLamps = false;

        foreach(GameObject element in puzzleElements)
        {
            if (element.gameObject.CompareTag("Lamp"))
            {
                isConnectedToLamps = true;

                if(element.gameObject.GetComponent<LampScript>().StartsAsActive)
                {
                    lampsStartAsActive = true;
                }

                break; //for the purposes of SFX we only care about the first lamp, so we break the loop here
            }
        }

        //LAMPS
        isConnectedToAntiStarZones = false;

        foreach (GameObject element in puzzleElements)
        {
            if (element.gameObject.CompareTag("AntiStarZone"))
            {
                isConnectedToAntiStarZones = true;

                if (element.gameObject.GetComponent<AntiStarZoneScript>().StartsAsActive)
                {
                    antiStarZonesStartAsActive = true;
                }

                break; //for the purposes of SFX we only care about the first lamp, so we break the loop here
            }
        }
        
        //WALLS
        isConnectedToWalls = false;

        foreach (GameObject element in puzzleElements)
        {
            if (element.gameObject.CompareTag("Wall"))
            {
                isConnectedToWalls = true;
                break; //for the purposes of SFX we only care about the first lamp, so we break the loop here
            }
        }

        if (isConnectedToWalls)
        {
            wallMoveSFXInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.WallMoveSFX);
        }
    }

    private void PlayActivationSFX()
    {
        if (isConnectedToSpikes)
        {
            if(spikesStartAsActive)
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
        
        if (isConnectedToAntiStarZones)
        {
            if (antiStarZonesStartAsActive)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.AntiStarZoneOffSFX);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.AntiStarZoneOnSFX);
            }
        }

        if (isConnectedToWalls)
        {
            wallMoveSFXInstance.start();
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

        if (isConnectedToAntiStarZones)
        {
            if (antiStarZonesStartAsActive)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.AntiStarZoneOnSFX);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.AntiStarZoneOffSFX);
            }
        }

        if (isConnectedToWalls)
        {
            wallMoveSFXInstance.start();
        }
    }
}
