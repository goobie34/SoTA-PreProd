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
public class UIAudioPlayer : MonoBehaviour
{
    public void PlayUIHoverSFX()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.UIButtonHoverSFX);
    }

    public void PlayUIClickSFX()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.UIButtonClickSFX);
    }
}
