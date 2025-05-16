using FMOD.Studio;
using UnityEngine;

public class BoulderController : MonoBehaviour, IInteractable
{
    //this script handles the the boulder-player interaction

    private static BoulderController currentlyActiveBoulder = null; //used to fix bug where player could attach to two boulders at once
    public static BoulderController GetCurrentlyActiveBoulder() { return currentlyActiveBoulder; }
    public bool IsAboutToSnapToFloor { get; private set; } = false; //used to make sure pressure plate is not retriggered when boulder snaps to floor

    private bool isAttached = false;
    private float interactionRange = 2f;

    public bool IsAttached { get { return isAttached; } }
    
    private Vector3 playerHitscan;
    private Vector3 offsetToPlayer;

    private GameObject player;
    private Rigidbody boulderRigidbody;
    private PlayerController playerController;

    private BoulderPushController pushController;
    private BoulderStarPushScript boulderStarPushScript;
    private BoulderPlayerPushScript boulderPlayerPushScript;

    private GameObject sidePushIndicatorXPositive;
    private GameObject sidePushIndicatorXNegative;
    private GameObject sidePushIndicatorZPositive;
    private GameObject sidePushIndicatorZNegative;

    private bool sideXPositiveBlocked = false;
    private bool sideXNegativeBlocked = false;
    private bool sideZPositiveBlocked = false;
    private bool sideZNegativeBlocked = false;

    public bool SideXPositiveBlocked
    {
        get { return sideXPositiveBlocked; } 
        set 
        { 
            sideXPositiveBlocked = value;
            sidePushIndicatorXPositive.SetActive(!value);
        }
    }
    public bool SideXNegativeBlocked
    {
        get { return sideXNegativeBlocked; }
        set
        {
            sideXNegativeBlocked = value;
            sidePushIndicatorXNegative.SetActive(!value);
        }
    }
    public bool SideZPositiveBlocked
    {
        get { return sideZPositiveBlocked; }
        set
        {
            sideZPositiveBlocked = value;
            sidePushIndicatorZPositive.SetActive(!value);
        }
    }
    public bool SideZNegativeBlocked
    {
        get { return sideZNegativeBlocked; }
        set
        {
            sideZNegativeBlocked = value;
            sidePushIndicatorZNegative.SetActive(!value);
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        pushController = GetComponent<BoulderPushController>();
        boulderStarPushScript = GetComponent<BoulderStarPushScript>();
        boulderPlayerPushScript = GetComponent<BoulderPlayerPushScript>();

        boulderRigidbody = GetComponent<Rigidbody>();
        sidePushIndicatorXPositive = transform.GetChild(6).gameObject;
        sidePushIndicatorXNegative = transform.GetChild(7).gameObject;
        sidePushIndicatorZPositive = transform.GetChild(8).gameObject;
        sidePushIndicatorZNegative = transform.GetChild(9).gameObject;
    }

    private void Update()
    {
        if (isAttached && !pushController.IsBeingPushed)
        {
            //if (playerController.IsGrounded())
            //    Debug.Log("PLAYER GROUNDED");

            if (playerController.GetBoulderPushDirection() != Vector3.zero)
            {
                boulderPlayerPushScript.PlayerPushInDirection(playerController.GetBoulderPushDirection());
            }
        }

        if (isAttached)
        {
            //player adheres to the boulders position, respecting the offset that was there when they attached
            player.transform.position = transform.position - offsetToPlayer; 
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!isAttached)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Level Floor") || collision.gameObject.CompareTag("Abyss") || collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        
        if (collision.gameObject.CompareTag("AntiStarZone") || collision.gameObject.CompareTag("PressurePlate"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Star") && collision.gameObject.GetComponent<StarActions>().IsOnPlayer) //so that carrying the star doesn't block the boulder push
        {
            return;
        }

        Detach();
    }

    public void SnapToFloor()
    {
        IsAboutToSnapToFloor = true;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1f))
        {
            Vector3 targetPosition = new Vector3(hit.transform.position.x, transform.position.y, hit.transform.position.z);
            transform.position = targetPosition;
        }

        IsAboutToSnapToFloor = false;
    }

    public void Interact()
    {
        if(currentlyActiveBoulder != null && currentlyActiveBoulder != this) //fixes bug where player could sometimes attach to two boulders at once
        {
            currentlyActiveBoulder.Detach();
            return;
        }

        if (isAttached)
        {
            Detach();
            return;
        }

        if (!PlayerIsClose())
        {
            return;
        }

        //ask player if they can find this boulder and if so, which direction the player is coming from
        playerHitscan = playerController.RayBoulderInteraction(interactionRange, this.gameObject);

        if (playerHitscan == Vector3.zero)
        {
            return;
        }

        if (!isAttached)
        {
            Attach();
        }
    }

    private bool PlayerIsClose()
    {
        return Vector3.Distance(transform.position, player.transform.position) <= interactionRange;
    }

    private void Attach()
    {
        isAttached = true;
        boulderRigidbody.isKinematic = false; 
        LockPlayerMovement();

        offsetToPlayer = transform.position - player.transform.position;
        currentlyActiveBoulder = this;

        playerController.AttachToBoulder();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BoulderAttachSFX);
    }
    public void Detach() //Added so when Load can detach the boulder from the player by Linus
    {
        isAttached = false;
        boulderRigidbody.isKinematic = true; //solves jank with boulder pushing away player when walked into
        playerController.UnlockMovement();

        currentlyActiveBoulder = null;
        playerController.DetachFromBoulder();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BoulderDetachSFX);
    }

    private void LockPlayerMovement()
    {
        if (playerHitscan == Vector3.forward || playerHitscan == Vector3.back)
        {
            playerController.LockMovement(Vector3.forward);
        }
        if (playerHitscan == Vector3.right || playerHitscan == Vector3.left)
        {
            playerController.LockMovement(Vector3.right);
        }
    }
    
}
