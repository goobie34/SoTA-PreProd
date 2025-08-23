using System;
using UnityEngine;
/// <summary>
/// Author:Emil
/// 
/// Modified by:
/// 
/// </summary>
public class AntiStarZoneScript : MonoBehaviour, IActivatable
{
    public StarActions starActions;
    public PlayerStarActionController playerStarActionController;
    ParticleSystemRenderer particleSystemRenderer;
    MeshRenderer parentMeshRenderer;
    Color color;
    [SerializeField] int EjectStarX;
    [SerializeField] int EjectStarZ;
    bool playerInZone = false;

    public bool StartsAsActive { get; private set; }

    void Start()
    {
        // Fetch the star in the scene
        starActions = GameObject.FindGameObjectWithTag("Star").GetComponent<StarActions>();
        playerStarActionController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStarActionController>();
        particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        parentMeshRenderer = GetComponentInParent<MeshRenderer>();
        //make the color match that of the top of the tile
        color = parentMeshRenderer.materials[4].color;
        particleSystemRenderer.material.color = color;

        StartsAsActive = isActiveAndEnabled;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStarActionController.DisallowStarOnPlayer();
            playerInZone = true;
        }
        if (other.CompareTag("Star"))
        {
            //remove star from player
            if (starActions.IsOnPlayer == true)
            {
                starActions.CarryToggle();
            }
            if (starActions.TravelCoroutine != null)
            {
                if (starActions.IsTraveling) //making an extra check here to ensure sfx doesn't play repeated times
                {
                    AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StarHitAntiStarZoneSFX, Vector3.zero);
                }
                starActions.StopTravelToDestination(true);
            }

            //determine where star should be pushed
            Vector3 dir = other.transform.position + (other.transform.position - transform.position);
            if (!starActions.IsTraveling)
            {
                starActions.TravelOutOfAntiStarZone(new Vector3(dir.x, playerStarActionController.transform.position.y, dir.z));

            }

        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStarActionController.DisallowStarOnPlayer();
            playerInZone = true;
        }
        if (other.CompareTag("Star"))
        {
            //remove star from player
            if (starActions.IsOnPlayer == true)
            {
                starActions.CarryToggle();
            }
            //traveling out from inside an anti-star zone
            if (!starActions.IsTraveling)
            {
                starActions.TravelOutOfAntiStarZone(new Vector3(EjectStarX * 100, playerStarActionController.transform.position.y, EjectStarZ * 100));
            }

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            playerStarActionController.AllowStarOnPlayer();
            playerInZone = false;
        
        }
        if (other.CompareTag("Star"))
        {
            if (starActions.IsTraveling)
            {
                starActions.StopTravelToDestination(false);
            }

        }
    }

    public void Activate()
    {
        if (playerInZone)
        {
            playerStarActionController.AllowStarOnPlayer();
            playerInZone = false;
        }
        gameObject.SetActive(false);
    }

    public void Deactivate()
    {

        gameObject.SetActive(true);


    }
}
