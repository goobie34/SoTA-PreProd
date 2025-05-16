using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float SFXVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    private List<EventInstance> eventInstances;

    public static AudioManager Instance { get; private set; }

    public static EventInstance ambienceEventInstance;
    EventInstance backgroundMusic;

    [SerializeField] bool disableBgMusic = false;
    [SerializeField] bool disableAmbience = false;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one Audio Manager in the scene.");
        }

        Instance = this;

        eventInstances = new List<EventInstance>();

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    void Start()
    {
        if (!disableAmbience)
        {
            InitializeAmbience(FMODEvents.Instance.Ambience);

        }

        if (!disableBgMusic)
        {
            StartBgMusic();
        }
    }

    private void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(SFXVolume);
    }

    public void PlayOneShot(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound, Vector3.zero);
    }
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);

        return eventInstance;
    }

    public void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateInstance(ambienceEventReference);
        ambienceEventInstance.start();
    }

    private void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

    }
    private void OnDestroy()
    {
        CleanUp();
    }

    private void StartBgMusic()
    {
        backgroundMusic = Instance.CreateInstance(FMODEvents.Instance.BackgroundMusic);
        backgroundMusic.start();
    }

    public void SetBgMusicState(bool shouldPlay)
    { 
        PLAYBACK_STATE playbackState;
        backgroundMusic.getPlaybackState(out playbackState);

        if (shouldPlay)
        {
            if (playbackState == PLAYBACK_STATE.STOPPED || playbackState == PLAYBACK_STATE.STOPPING)
            {
                backgroundMusic.start();
            }
        } else
        {
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                backgroundMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}
