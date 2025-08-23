using FMOD.Studio;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Author:Gabbriel
/// 
/// Modified by:Karin, Linus, Emil
/// 
/// </summary>
/// 

public class PlayerStarActionController : MonoBehaviour
{
    // COMPONENTS
    private Transform starTransform;
    private StarActions starActions;
    private PlayerInput playerInput;
    private PlayerController playerController;


    // TWEAKABLE VARIABLES
    private bool pickUpAllowed = true;
    [SerializeField] private bool recallAllowed = false;
    private bool recallAllowedAtStart = false;
    [SerializeField] private bool gravityPullAllowed = false;
    [SerializeField] private bool strongThrowAllowed = false;

    [SerializeField] private float normalThrowRange = 4;
    [SerializeField] private float strongThrowRange = 10;

    [SerializeField] private float starPickupRange = 1.0f;
    [SerializeField] private float recallRange = 4.0f;

    [SerializeField] private float gravityPullRange = 3.0f;
    [SerializeField] private float gravityPullAcceptanceRadius = 0.01f;
    [SerializeField] private float gravityPullSpeed = 5.0f;

    [SerializeField] private float aimSensitivity = 0.5f;
    [SerializeField] private float strongThrowSensitivity = 1f;
    [SerializeField] private float aimRotationByDegrees = 45;

    [SerializeField] private float controllerAimSmoothness = 3f;

    public float GravityPullRange
    {
        get => gravityPullRange;
        private set => gravityPullRange = value;
    }

    public float RecallRange
    {
        get => recallRange;
        private set => recallRange = value;
    }

    // STORING/VALUE VARIABLES
    private bool isAiming = false;
    private bool strongThrow = false;
    private bool Controller = false;
    private bool isBeingGravityPulled = false;

    public bool IsBeingGravityPulled
    {
        get => isBeingGravityPulled;
        private set => isBeingGravityPulled = value;
    }

    private Vector3 mouseDownPosition;
    private Vector3 mouseReleasePosition;
    private Vector3 rotationAxis = Vector3.up;

    private Vector3 aimInput;
    private Vector3 aimSmooth = Vector3.zero;
    private Vector3 throwDirection;
    private Vector3 throwTargetDestination;

    private LineRenderer lineRenderer;

    private EventInstance lowHealthWarningSFX;

    private IEnumerator GravityPull_IEnumerator;

    private PlayerHealth playerHealth;

    private EventInstance gravityPullSFX;

    private bool smoothAim = false;
    // ENGINE METHODS ====================================== // 

    void Start()
    {
        GameObject star = GameObject.FindGameObjectWithTag("Star");
        starActions = star.GetComponent<StarActions>();
        starTransform = star.GetComponent<Transform>();

        playerController = this.GetComponent<PlayerController>();
        playerHealth = this.GetComponent<PlayerHealth>();
        playerInput = this.GetComponent<PlayerInput>();

        gravityPullSFX = AudioManager.Instance.CreateInstance(FMODEvents.Instance.StarGravityPullSFX);

        InitializeLineRenderer();
        
        if (playerInput.currentActionMap.name == "PlayerControlController")
        {
            Controller = true;
        }
        else
        {
            Controller = false;
        }

            lowHealthWarningSFX = AudioManager.Instance.CreateInstance(FMODEvents.Instance.LowHealthWarningSFX);

        if (recallAllowed)
        {
            recallAllowedAtStart = true;
        }
    }

    void Update()
    {
        if (playerInput.currentActionMap != null && playerInput.currentActionMap.name == "PlayerControlController")
        {
            Controller = true;
        }
        else
        {
            Controller = false;
        }

        if (isAiming)
        {
            if (Controller)
            {
                throwDirection = aimSmooth;
            }
            else
            {
                mouseReleasePosition = Input.mousePosition;
                throwDirection = mouseDownPosition - mouseReleasePosition; // Drag direction
                throwDirection.z = throwDirection.y; // Map vertical screen movement to Z-axis movement
                throwDirection.y = 0; // Keep movement on XZ plane

                if (strongThrow)
                {
                    throwDirection *= strongThrowSensitivity / 100; //controlling the length of the throw was way too sensitive without this
                }
                else
                {
                    throwDirection *= aimSensitivity / 100; //controlling the length of the throw was way too sensitive without this
                }
            }

            if (strongThrow && throwDirection.sqrMagnitude > MathF.Pow(strongThrowRange, 2))
            {
                throwDirection = throwDirection.normalized * strongThrowRange;
            }
            else if (!strongThrow && throwDirection.sqrMagnitude > MathF.Pow(normalThrowRange, 2))
            {
                throwDirection = throwDirection.normalized * normalThrowRange;
            }

            throwDirection = HelperScript.RotateVector3(throwDirection, aimRotationByDegrees, rotationAxis);

            StopAimAtColliders(transform.position, throwDirection.magnitude);

            DrawAimLine();
        }
        else
        {
            HideAimLine();
        }
    }

    // METHODS ====================================== //

    void DrawAimLine()
    {
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + throwDirection);
    }

    void HideAimLine()
    {
        if (lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
        }
    }

    void StopAimAtColliders(Vector3 startFrom, float distance)
    {
        RaycastHit hitInfo;
        Ray aimRay = new Ray(startFrom, throwDirection.normalized);
        if (Physics.Raycast(aimRay, out hitInfo, distance, LayerMask.GetMask("StopStar", "StarLayer")))
        {
            throwDirection = throwDirection.normalized * Vector3.Distance(transform.position, hitInfo.point);
        }

    }

    IEnumerator GravityPullToDestination(Vector3 targetDestination)
    {
        float gravityPullCompletion = 0f;                                                           //for sfx
        float initialDistanceToTarget = Vector3.Distance(transform.position, targetDestination);    //for sfx
        gravityPullSFX.setParameterByName("GravityPullCompletion", gravityPullCompletion);
        gravityPullSFX.start();
        playerController.StartBeingGravityPulled();//Locks input and movement during gravity pull and Disables gravity

        isBeingGravityPulled = true;

        Vector3 lastPosition = transform.position; //Position of player when starting gravity pull
        Vector3 direction;
        while (true)
        {
            // Calculate direction and movement (no gravity applied, only horizontal movement)
            direction = (targetDestination - transform.position).normalized;
            Vector3 move = direction * gravityPullSpeed;

            playerController.CharacterController.Move(move * Time.deltaTime);

            float distanceToTarget = Vector3.Distance(transform.position, targetDestination);

            gravityPullCompletion = 1 - (distanceToTarget / initialDistanceToTarget);                 //for sfx
            gravityPullSFX.setParameterByName("GravityPullCompletion", gravityPullCompletion);  //for sfx


            if (distanceToTarget <= gravityPullAcceptanceRadius)                                                                                                             
            {
                transform.position = targetDestination; //Set position directly to the Star to avoid any small overshoot
                break;
            }

            //if the player is no longer flying towards star, it has gotten stuck and therefore we interupt the gravity pull
            if (Vector3.Distance(transform.position, lastPosition) < 0.01f) //this check COULD cause problems at super high frame rates, might be an option to use WaitForFixedUpdate() if that becomes a problem
            {
                InteruptGravityPullToDestination();
                yield break;
            }

            lastPosition = transform.position; //Update the last position of player

            yield return null;
        }
        playerController.SetPlayerPosition(targetDestination + direction*0.1f);
        StopSuccessfulGravityPullToDestination();
    }

    private void StopSuccessfulGravityPullToDestination() //this method is for ending SUCCFESSFUL gravity pulls
    {
        if (GravityPull_IEnumerator != null)
        {
            StopCoroutine(GravityPull_IEnumerator);
            starActions.Recall();
            playerController.StopBeingGravityPulled(); //Enable gravity again and Re-enable input after the pull
            isBeingGravityPulled = false;
            gravityPullSFX.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void InteruptGravityPullToDestination() //this method is for interupting UNSUCCESSFUL gravity pulls
    {
        if (GravityPull_IEnumerator != null)
        {
            StopCoroutine(GravityPull_IEnumerator);
            playerController.StopBeingGravityPulled(); //Enable gravity again and Re-enable input after the pull
            isBeingGravityPulled = false;
            gravityPullSFX.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    private void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.positionCount = 2;
    }

    

    void ThrowStar()
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (starActions.IsOnPlayer && isAiming)
        {
            isAiming = false;
            throwTargetDestination = transform.position + throwDirection;
            starActions.Throw(throwTargetDestination, throwDirection.normalized);

            if (strongThrow)
            {
                //this code is never triggered
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StrongThrowAttackSFX);
                Debug.Log("StrongThrow SFX");
            } else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StarThrowAttackSFX);
                Debug.Log("NormalThrow SFX");
            }
        }
    }

    // INPUT RELATED MEHTODS ====================================== //
    public void AllowStarOnPlayer()
    {
        if (recallAllowedAtStart)
        {
            recallAllowed = true;
        }
        pickUpAllowed = true;
    }

    public void DisallowStarOnPlayer()
    {
        recallAllowed = false;
        pickUpAllowed = false;
    }
    void OnCarryStarToggle(InputValue input)
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (isBeingGravityPulled)
        {
            return;
        }

        if (Vector3.Distance(transform.position, starTransform.position) <= starPickupRange && pickUpAllowed)
        {
            starActions.CarryToggle();
            return;
        }

        if (!recallAllowed)
        {
            //SFX for recall attempt when not allowed
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StarRecallFailSFX);
            return;
        }

        if (Vector3.Distance(transform.position, starTransform.position) <= recallRange)
        {
            starActions.Recall();
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StarRecallSuccessSFX);
        }
        else
        {
            //SFX for recall attempt but is too far away
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StarRecallFailSFX);
        }
    }

    void OnLeftMouseDown(InputValue input)
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (starActions.IsOnPlayer)
        {
            isAiming = true;

            mouseDownPosition = Input.mousePosition;
        }
    }

    void OnRightMouseDown(InputValue input)
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (!strongThrowAllowed)
        {
            return;
        }

        if (starActions.IsOnPlayer)
        {
            isAiming = true;
            strongThrow = true;

            mouseDownPosition = Input.mousePosition;
        }
    }

    void OnRightMouseRelease(InputValue input)
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (!strongThrowAllowed)
        {
            return;
        }

        isAiming = false;
        strongThrow = false;

        if (starActions.IsOnPlayer)
        {
            throwTargetDestination = transform.position + throwDirection;

            starActions.Throw(throwTargetDestination, throwDirection.normalized);

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.StrongThrowAttackSFX);
            Debug.Log("StrongThrow SFX");
        }
    }

    void OnGravityPull(InputValue input)
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (!gravityPullAllowed ||starActions.IsOnPlayer || starActions.IsTraveling || isBeingGravityPulled)
        {
            return;
        }

        if (Vector3.Distance(transform.position, starTransform.position) <= gravityPullRange)
        {
            if(GravityPull_IEnumerator != null)
                StopCoroutine(GravityPull_IEnumerator);
            GravityPull_IEnumerator = GravityPullToDestination(starTransform.position);
            StartCoroutine(GravityPull_IEnumerator);
        }
    }
    
    void OnAimInput(InputValue input) //For Controller
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (starActions.IsOnPlayer)
        {
            isAiming = true;
            Vector2 input2d = input.Get<Vector2>();
            aimInput = new Vector3(input2d.x, 0, input2d.y);
            if (strongThrow)
            {
                aimInput *= strongThrowRange;
            }
            else
            {
                aimInput *= normalThrowRange;
            }
          
            if (!smoothAim)
            {
                StartCoroutine(SmoothAim());
            }

        }
    }
    private IEnumerator SmoothAim()
    {
        smoothAim = true;
        while (Vector3.Distance(aimSmooth, aimInput) > 0.05f)
        {
            aimSmooth = Vector3.Lerp(aimSmooth, aimInput, controllerAimSmoothness * Time.deltaTime);
            yield return null;
        }
        aimSmooth = aimInput;
        smoothAim = false;
    }

    void OnAimRelease(InputValue input)
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (Vector3.Distance(aimInput, Vector3.zero) < 0.2f)
        {
            isAiming = false;
            aimInput = Vector3.zero;
            if (smoothAim)
            {
                StopCoroutine(SmoothAim());
            }
           
        }
    }

    void OnThrowRelease()
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        ThrowStar();
    }

    void OnStrongThrow()
    {
        if (playerHealth.IsDead) return; //so player cannot do do this action when dead

        if (!strongThrowAllowed)
        {
            return;
        }
        strongThrow = true;
    }

    void OnStrongThrowRelease()
    {
        strongThrow = false;
    }
}
