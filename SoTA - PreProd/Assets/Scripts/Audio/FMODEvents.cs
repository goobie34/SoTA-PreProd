using UnityEngine;
using FMODUnity;
/// <summary>
/// Author:Gabbriel
/// 
/// Modified by:
/// 
/// </summary>
public class FMODEvents : MonoBehaviour
{
    [field: Header("Button SFX")]
    [field: SerializeField] public EventReference ButtonSFX { get; private set; }

    [field: Header("Timer Ticking SFX")]
    [field: SerializeField] public EventReference TimerTickingSFX { get; private set; }

    [field: Header("Pressure Plate SFX")]
    [field: SerializeField] public EventReference PressurePlateSFX { get; private set; }
    
    [field: Header("Slither SFX")]
    [field: SerializeField] public EventReference SlitherSound { get; private set; }
    
    [field: Header("Boulder SFX")]
    [field: SerializeField] public EventReference BoulderSFX { get; private set; }
    [field: SerializeField] public EventReference BoulderAttachSFX { get; private set; }
    [field: SerializeField] public EventReference BoulderDetachSFX { get; private set; }
    
    [field: Header("Low Health Warning SFX")]
    [field: SerializeField] public EventReference LowHealthWarningSFX { get; private set; }
    
    [field: Header("Death SFX")]
    [field: SerializeField] public EventReference DeathSFX { get; private set; }
    
    [field: Header("Background Music")]
    [field: SerializeField] public EventReference BackgroundMusic { get; private set; }
    [field: SerializeField] public EventReference AmbientTrack01a { get; private set; }
    [field: SerializeField] public EventReference AmbientTrack01b { get; private set; }
    [field: SerializeField] public EventReference AmbientTrack02 { get; private set; }
    [field: SerializeField] public EventReference AmbientTrack03 { get; private set; }
    [field: SerializeField] public EventReference AmbientTrack04 { get; private set; }
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference Ambience { get; private set; }
    [field: SerializeField] public EventReference DeepAmbience { get; private set; }
    
    [field: Header("Star")]
    [field: SerializeField] public EventReference StarThrowSFX  { get; private set; }
    [field: SerializeField] public EventReference StarThrowAttackSFX  { get; private set; }
    [field: SerializeField] public EventReference StrongThrowAttackSFX  { get; private set; }
    [field: SerializeField] public EventReference StarLandFloorSFX  { get; private set; }
    [field: SerializeField] public EventReference StarLandAbyssSFX  { get; private set; }
    [field: SerializeField] public EventReference StarHitWallSFX  { get; private set; }
    [field: SerializeField] public EventReference StarHitBoulderSFX  { get; private set; }
    [field: SerializeField] public EventReference StarHitAntiStarZoneSFX  { get; private set; }
    [field: SerializeField] public EventReference StarShimmerSFX { get; private set; }
    [field: SerializeField] public EventReference StarRecallSuccessSFX { get; private set; }
    [field: SerializeField] public EventReference StarRecallFailSFX { get; private set; }
    [field: SerializeField] public EventReference StarPickupSFX { get; private set; }
    [field: SerializeField] public EventReference StarGravityPullSFX { get; private set; }
    
    [field: Header("Activatable SFX")]
    [field: SerializeField] public EventReference SpikesAppearSFX { get; private set; }
    [field: SerializeField] public EventReference SpikesDisappearSFX { get; private set; }
    [field: SerializeField] public EventReference LampTurnOnSFX { get; private set; }
    [field: SerializeField] public EventReference LampTurnOffSFX { get; private set; }




    public static FMODEvents Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one FMOD Events instance in the scene.");
        }

        Instance = this;
    }
}
