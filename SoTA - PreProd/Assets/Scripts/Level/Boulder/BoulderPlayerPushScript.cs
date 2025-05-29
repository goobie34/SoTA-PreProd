using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// Author: Gabbriel
/// 
/// Modified by: Linus, Sixten
/// 
/// </summary>
public class BoulderPlayerPushScript : MonoBehaviour
{
    //this script handles moving the boulder when player pushes it or pulls on it

    [field: Header("Player Push Parameters")]
    [SerializeField] float playerPushSpeed = 10f;
    [SerializeField] float playerPushDistance = 1f;
    [SerializeField] float playerPushCooldown = 0.5f;

    Rigidbody boulderRigidbody;
    BoulderController boulderController;
    BoulderPushController pushController;
    PlayerController playerController;

    IEnumerator PlayerPushCoroutine;

    private bool isBeingPlayerPushed = false;
    public bool IsBeingPlayerPushed { get { return isBeingPlayerPushed; } }
    public bool IsCurrentlyMoving { get; private set; } = false;

    void Start()
    {
        boulderRigidbody = GetComponent<Rigidbody>();
        boulderController = GetComponent<BoulderController>();
        pushController = GetComponent<BoulderPushController>();

        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

    }

    public void PlayerPushInDirection(Vector3 direction)
    {
        if (pushController.IsBeingPushed)
        {
            return;
        }

        if (pushController.CheckForValidPushDestination(direction, playerPushDistance) && CheckValidPlayerDestinationAfterPush(direction, playerPushDistance))
        {
            PlayerPushCoroutine = PlayerPushInDirection_IEnumerator(direction, playerPushDistance);
            StartCoroutine(PlayerPushCoroutine);
        }
        else
        {
            boulderController.Detach(); //fixes bug where player would start moving boulder in opposite direction when their movement input was held down
        }
    }

    public bool CheckValidPlayerDestinationAfterPush(Vector3 direction, float distance)
    {
        RaycastHit[] hits;
        Debug.DrawRay(playerController.transform.position, direction, Color.red, 1.0f);
        hits = Physics.RaycastAll(playerController.transform.position, direction, distance);
        if (!Physics.Raycast(playerController.transform.position + direction * distance, Vector3.down, 1f))
        {
            //checks if there is ground below the target destination to stop player from being pushed into the abyss
            return false;
        }

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.gameObject.CompareTag("PressurePlate") && !hit.collider.gameObject.CompareTag("AntiStarZone") && !hit.collider.gameObject.CompareTag("CameraPan") && !hit.collider.gameObject.CompareTag("Spikes"))
            {
                if (hit.collider.gameObject.CompareTag("BoulderSide"))
                {

                    if (hit.collider.gameObject.GetComponentInParent<BoulderController>() != BoulderController.GetCurrentlyActiveBoulder())
                    {
                        //Debug.Log("RAYCAST HIT SOMETHING WITH TAG: " + hit.collider.gameObject.tag);
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (hit.collider.gameObject.CompareTag("Boulder"))
                {
                    if (hit.collider.gameObject.GetComponent<BoulderController>() != BoulderController.GetCurrentlyActiveBoulder())
                    {
                        //Debug.Log("RAYCAST HIT SOMETHING WITH TAG: " + hit.collider.gameObject.tag);
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }
                //Debug.Log("RAYCAST HIT SOMETHING WITH TAG: " + hit.collider.gameObject.tag);


                return false;
            }
        }
       

        return true;
    }

    IEnumerator PlayerPushInDirection_IEnumerator(Vector3 direction, float distance)
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BoulderSFX);

        Vector3 targetDestination = transform.position + direction * distance;

        IsCurrentlyMoving = true;
        isBeingPlayerPushed = true;
        Vector3[] directionsToCheck = { direction, -direction };

        while (Vector3.Distance(transform.position, targetDestination) > pushController.PushDestinationAcceptanceRadius)
        {
            //sets velocity to zero as there could sometimes be a downward force (that was not gravity)
            //still unclear where it came from but setting velocity to 0 seems to fix it!

            if (!boulderRigidbody.isKinematic) //to avoid warning that sometimes would appear in editor
            {
                boulderRigidbody.velocity = new Vector3(0, 0, 0);
            }

            Vector3 tempDirection = targetDestination - transform.position;

            transform.position += tempDirection * playerPushSpeed * Time.deltaTime;

            if (boulderController.IsAttached)
                pushController.CheckSides(directionsToCheck);
            yield return null;
            
        }

        IsCurrentlyMoving = false;
        boulderController.SnapToFloor(); //looks weird if this snap happens AFTER the cooldown
        if (boulderController.IsAttached)
            pushController.CheckSides(directionsToCheck);
        if (!playerController.IsGrounded() && boulderController.IsAttached)
        {
            boulderController.Detach();
        }

        yield return new WaitForSeconds(playerPushCooldown);

        StopPlayerPush();
    }
    public void StopPlayerPush()
    {
        if (PlayerPushCoroutine != null)
        {
            StopCoroutine(PlayerPushCoroutine);
        }

        isBeingPlayerPushed = false;
    }
}

