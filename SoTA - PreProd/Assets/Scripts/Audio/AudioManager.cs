using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;
/// <summary>
/// Author:Gabbriel
/// 
/// Modified by:
/// 
/// </summary>
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
    public static EventInstance deepAmbienceEventInstance;

    [SerializeField] int backgroundMusicIndex = 0;
    EventInstance backgroundMusic;
    List<EventReference> backgroundMusicList;

    [SerializeField] bool disableBgMusic = false;
    [SerializeField] bool disableAmbience = false;
    [SerializeField] bool disableDeepAmbience = false;

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


        backgroundMusicList = new List<EventReference>();
        backgroundMusicList.Add(FMODEvents.Instance.BackgroundMusic);   //0
        backgroundMusicList.Add(FMODEvents.Instance.AmbientTrack01a);   //1
        backgroundMusicList.Add(FMODEvents.Instance.AmbientTrack01b);   //2
        backgroundMusicList.Add(FMODEvents.Instance.AmbientTrack02);    //3
        backgroundMusicList.Add(FMODEvents.Instance.AmbientTrack03);    //4
        backgroundMusicList.Add(FMODEvents.Instance.AmbientTrack04);    //5
    }

    void Start()
    {
        if (!disableAmbience)
        {
            InitializeAmbience(FMODEvents.Instance.Ambience);
        }
        
        if (!disableDeepAmbience) //want to have this in main menu without the more "wind like" ambience above
        {
            InitializeDeepAmbience(FMODEvents.Instance.DeepAmbience);
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
    public void InitializeDeepAmbience(EventReference ambienceEventReference)
    {
        deepAmbienceEventInstance = CreateInstance(ambienceEventReference);
        deepAmbienceEventInstance.start();
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
        backgroundMusic = Instance.CreateInstance(backgroundMusicList[backgroundMusicIndex]);
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

    public void SetAllAmbienceState(bool shouldPlay)
    {
        SetAmbienceState(shouldPlay);
        SetDeepAmbienceState(shouldPlay);
    }


    public void SetAmbienceState(bool shouldPlay)
    { 
        PLAYBACK_STATE playbackState;
        ambienceEventInstance.getPlaybackState(out playbackState);

        if (shouldPlay)
        {
            if (playbackState == PLAYBACK_STATE.STOPPED || playbackState == PLAYBACK_STATE.STOPPING)
            {
                ambienceEventInstance.start();
            }
        } else
        {
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                ambienceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    
    public void SetDeepAmbienceState(bool shouldPlay)
    { 
        PLAYBACK_STATE playbackState;
        deepAmbienceEventInstance.getPlaybackState(out playbackState);

        if (shouldPlay)
        {
            if (playbackState == PLAYBACK_STATE.STOPPED || playbackState == PLAYBACK_STATE.STOPPING)
            {
                deepAmbienceEventInstance.start();
            }
        } else
        {
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                deepAmbienceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    public void PlayUIHoverSFX()
    {
        PlayOneShot(FMODEvents.Instance.UIButtonHoverSFX);
    }

    public void PlayUIClickSFX()
    {
        PlayOneShot(FMODEvents.Instance.UIButtonClickSFX);
    }
}
