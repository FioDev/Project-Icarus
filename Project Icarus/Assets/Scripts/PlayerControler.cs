using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FlightTypes
{
    NoClip,
    CreativeMode,
    Aerodynamic,
    GlideOnly
}

public enum SpeedType
{
    Sprint,
    Run,
    Walk,
    Crouch,
    Swim
}

public class PlayerControler : MonoBehaviour
{
    //Variables
    #region variables


    //component references
    #region component references
    [Header("Component References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController characterController;

    
    #endregion

    //Master behaviour toggles
    #region behaviour toggles
    //Aesthetic options
    //Things that change the aesthetics of the player controler but not the functionality

    [Header("--Aesthetic Options--")]
    [SerializeField] private bool doHeadBob = false;

    //functional options
    //Things that change the way the game plays

    [Header("--Movement Toggles--")]
    //Jump variables
    [SerializeField] private bool canJump;
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool canFly;

    [Space(10)]
    //movement variables
    [SerializeField] private bool canMove;
    [SerializeField] private bool canWalk;
    [SerializeField] private bool canSprint;
    [SerializeField] private bool canCrouch;
    [SerializeField] private bool canSlide;

    [Space(10)]
    //other movement variables
    [SerializeField] private bool canSwim;
    [SerializeField] private bool canClimb;

    [Header("--Gameplay Toggles--")]
    [SerializeField] private bool canInteract;
    [SerializeField] private bool canUseGravity;
    [SerializeField] private bool canMoveCamera;

    #endregion

    //Behaviour parameters
    #region behaviour parameters
    [Header("--Behaviour Perameters--", order = 0)]
    [Space(10, order = 1)]
    [Header("Jump, Double Jump & flight", order = 2)]
    [Space(10, order = 3)]
    //jump and vertical perams
    [SerializeField] private float jumpForce;
    [SerializeField] private int maxDoubleJumps;
    [SerializeField] private FlightTypes flightType;

    [Header("Physics Perameters")]
    [SerializeField] private float movementSensitivity = 1.0f;
    [SerializeField] private float airMovementAuthority = 0.25f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float groundDrag = 1.1f;
    [SerializeField] private float bounceLimiter = 3;
    [SerializeField] private float liquidBouyancy = 1f;

    [Header("movement speed Perameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private float flySpeed;
    [SerializeField] private float swimSpeed;
    [SerializeField] private float climbSpeed;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    #endregion

    //Key bindings
    #region key bindings
    [Header("Controls")]
    [SerializeField] private bool useAnalogueMovement;
    [SerializeField] private KeyCode movementForward;
    [SerializeField] private KeyCode movementBackward;
    [SerializeField] private KeyCode movementRight;
    [SerializeField] private KeyCode movementLeft;
    [SerializeField] private KeyCode movementJump;
    [SerializeField] private KeyCode movementCrouch;
    [SerializeField] private KeyCode movementWalk;
    [SerializeField] private KeyCode movementSprint;
    [SerializeField] private KeyCode primaryAction;
    [SerializeField] private KeyCode secondaryAction;
    [SerializeField] private KeyCode interaction;

    #endregion

    //Storage variables
    #region storage variables
    private float rotationX = 0; //camera rotation vertical

    private Vector3 currentMovement = Vector3.zero; //The actual current movement of the players character
    private Vector2 oldHorizontalMovement = Vector2.zero; //The "last frame" 2D value stored in "Current movement"
    private Vector2 targetMovement = Vector2.zero; //The ideal target movement of the player's character
    private Vector3 horizontalInput = Vector3.zero; //Horizontal input rotated to be the correct orientation
    private Vector3 rawHorizontalInput = Vector3.zero; // the 2D input of the WASD keys ranging from 1 to -1
    private float currentTargetSpeed = 0f; //The speed to multiply horizontal input by to gain target movement

    private SpeedType speedState = SpeedType.Walk; //Speed type to apply to grounded(and some other) horizontal movement

    private int currentDoubleJumps;

    //bounce variables
    private bool shouldBounce = false;
    private ControllerColliderHit bounceHit;

    private bool movementKeysDown
    {
        get
        {
            //analogue clause
            if (useAnalogueMovement)
            {
                if (rawHorizontalInput == Vector3.zero)
                {
                    return false;
                } else
                {
                    return true;
                }
            }

            if (Input.GetKey(movementLeft))
            {
                return true;
            }

            if (Input.GetKey(movementRight))
            {
                return true;
            }

            if (Input.GetKey(movementForward))
            {
                return true;
            }

            if (Input.GetKey(movementBackward))
            {
                return true;
            }

            return false;
        }
    }

    private bool shouldApplyDrag
    {
        get
        {
            //input check. If input is being given, do not drag.
            if (movementKeysDown)
            {
                return false;
            }

            //else, ground check
            //If on ground, do drag, else do not
            if (characterController.isGrounded)
            {
                return true;
            }

            return false;

        }
    }

    private bool shouldJump
    {
        get
        {
            //guard clause - If no double jump, assume case is "only on ground"
            if (!canDoubleJump)
            {
                if (characterController.isGrounded)
                {
                    return true;
                }
                else return false;
            }

            //else, check double jump charges
            if (currentDoubleJumps != 0)
            {
                return true;
            }

            return false;
        }
    }

    //water / swim variables
    private bool isSwimming = false;
    private bool touchingWater = false; //Holds wether water is being touched
    private Collider waterTouched; //Holds the collider of the water being touched
    private float surfaceYValue;
    private float sinkRate;
    private bool shouldSwim
    {
        get
        {
            //Out of water clause, do not swim
            if (!touchingWater)
            {
                return false;
            }

            //Water not deep enough, return false and do not swim
            if (Vector3.Distance(waterTouched.ClosestPoint(transform.position), transform.position) > 0.1f)
            {
                return false;
            }

            //Else, swim
            return true;
        }
    }


    #endregion

    #endregion

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        //Camera Look
        if (canMoveCamera)
        {
            HandleMouseLook();
        }

        //bounce check
        if (shouldBounce)
        {
            HandleAirBounce(bounceHit);
        }

        //Handle Swim Checks
        if (canSwim)
        {
            HandleSwim();
        }

        //Handle Movement
        if (canMove)
        {
            GetHorizontalInput();
            HandleMovement();
        }

        if (canUseGravity)
        {
            HandleGravity();
        }

        //Handle Jump
        if (canJump)
        {
            HandleJump();
        }


        ApplyFinalMovement();

    }

    #region Initial Movement Input Helpers
    private void HandleMouseLook()
    {
        //up and down camera
        //mouse y rotation controls x rotation - i promise im not mixing them up here
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); //Clamps look rotation
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        //left and right
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void GetHorizontalInput()
    {
        //Analogue / Keyboard movement check / movemement gathering
        if (useAnalogueMovement)
        {
            rawHorizontalInput = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        } else
        {
            //Check which movement keys are applied
            float x = 0;
            float z = 0;

            if (Input.GetKey(movementForward))
            {
                x = 1;
            }

            if (Input.GetKey(movementBackward))
            {
                x = -1;
            }

            if (Input.GetKey(movementRight))
            {
                z = 1;
            }

            if (Input.GetKey(movementLeft))
            {
                z = -1;
            }

            rawHorizontalInput.x = x;
            rawHorizontalInput.y = z;
        }

        horizontalInput = (transform.TransformDirection(Vector3.forward) * rawHorizontalInput.x) + (transform.TransformDirection(Vector3.right) * rawHorizontalInput.y);
        //horizontalInput = horizontalInput.normalized;
        horizontalInput.y = 0f;
    }

    #endregion

    //If the character is in the air, bounce on all but the Y axis, and divide the speed by the bounce limiter
    private void HandleAirBounce(ControllerColliderHit hit)
    {
        if (!characterController.isGrounded)
        {
            float yCache = currentMovement.y;
            currentMovement = Vector3.Reflect(currentMovement, hit.normal) / bounceLimiter;
            currentMovement.y = yCache;
        }

        //Then, reset the shouldBounce so that it doesnt double bounce on the next frame without due cause (the hit collider triggering again)
        shouldBounce = false;
    }

    private void HandleSwim()
    {
        if (shouldSwim)
        {
            Debug.Log("swimming - TO DO - just imagine you are swimming rn");
            isSwimming = true;

        } else
        {
            isSwimming = false;
        }
    }

    private void HandleJump()
    {
        //No jump if swimming
        if (isSwimming)
        {
            return;
        }

        if (Input.GetKeyDown(movementJump) && shouldJump)
        {
            currentMovement.y = jumpForce;

            //if using the double jump moddel, and not grounded, start reducing double jumps
            if (canDoubleJump && !characterController.isGrounded)
            {
                currentDoubleJumps--;
            }
        }

        if (characterController.isGrounded)
        {
            currentDoubleJumps = maxDoubleJumps;
        }
    }

    private void HandleMovement()
    {
        //
        DetermineSpeedState();

        //Handles individual states of horizontal movement
        HandleHorizontalMovement();
    }

    private void HandleHorizontalMovement()
    {
        switch (speedState)
        {
            case SpeedType.Sprint:
                currentTargetSpeed = sprintSpeed;
                HandleSprint();
                break;

            case SpeedType.Walk:
                currentTargetSpeed = walkSpeed;
                HandleWalk();
                break;

            case SpeedType.Run:
                currentTargetSpeed = moveSpeed;
                HandleRun();
                break;

            case SpeedType.Crouch:
                currentTargetSpeed = crouchSpeed;
                HandleCrouch();
                break;


        }

        //Apply drag under circumstance
        if (shouldApplyDrag)
        {
            HandleDrag();
        }
    }

    #region horizontal movement states 
    private void HandleSprint()
    {

        //Sprint can: go faster
        //Sprint cannot: interact, be stealthy

        UpdateTargetMovement(currentTargetSpeed);

    }

    private void HandleWalk()
    {

        //Walk can: more detail, interact, etc
        //Walk cannot: go fast

        UpdateTargetMovement(currentTargetSpeed);
        HandleInteraction();

    }

    private void HandleRun()
    {

        //Run can: do pretty much everything to a decent level
        //Run can't: be stealthy

        UpdateTargetMovement(currentTargetSpeed);
        HandleInteraction();

    }

    private void HandleCrouch()
    {

        //Crouch can: be stealthy, see detail, interact
        //crouch cant: go fast

        UpdateTargetMovement(currentTargetSpeed);
        HandleInteraction();

    }

    #endregion

    private void HandleDrag()
    {
        targetMovement.x /= groundDrag;
        targetMovement.y /= groundDrag;
    }
    private void HandleGravity()
    {
        //dont even TRY gravity if we're swimming rn
        if (isSwimming)
        {
            return;
        }

        if (!characterController.isGrounded)
        {
            currentMovement.y += gravity * Time.deltaTime;
        } else
        {
            //Sets to near-zero value, rather than zero
            currentMovement.y = gravity * Time.deltaTime;
        }
    }

    private void HandleInteraction()
    {
        
    }

    private void UpdateTargetMovement(float speed)
    {
        targetMovement.x = horizontalInput.x * speed;
        targetMovement.y = horizontalInput.z * speed;
    }

    private void DetermineSpeedState()
    {
        if (canSprint)
        {
            if (Input.GetKey(movementSprint))
            {
                speedState = SpeedType.Sprint;
                return;
            }
        }

        if (canCrouch)
        {
            if (Input.GetKey(movementCrouch))
            {
                speedState = SpeedType.Crouch;
                return;
            }
        }

        if (canWalk)
        {
            if (Input.GetKey(movementWalk))
            {
                speedState = SpeedType.Walk;
                return;
            }
        }

        if (shouldSwim)
        {
            speedState = SpeedType.Swim;
            return;
        }

        //TODO add cases for flight, swimming, etc

        speedState = SpeedType.Run;
    }

    private void ApplyFinalMovement()
    {
        //Cache current movement in old horizontal movement
        oldHorizontalMovement.x = currentMovement.x;
        oldHorizontalMovement.y = currentMovement.z;

        //Lerp between "real" horizontal motion and "target" horizontal motion, by deltatime * sensitivity
        //Also multiplies movement sensitivity by the air movement authority *if* in the air, else, just multiplies by 1
        oldHorizontalMovement = Vector2.Lerp(oldHorizontalMovement, targetMovement, (movementSensitivity * (characterController.isGrounded ? 1 : airMovementAuthority)) * Time.deltaTime);

        //apply to currentmovement, ignoring Y component
        currentMovement.x = oldHorizontalMovement.x;
        currentMovement.z = oldHorizontalMovement.y;

        //Send to character controler
        characterController.Move(currentMovement * Time.deltaTime);

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        shouldBounce = true;
        bounceHit = hit;
    }

    private void OnTriggerEnter(Collider other)
    {
        //check for water layer - water is layer 4
        if (other.gameObject.layer == 4)
        {
            Debug.Log("entered water");
            touchingWater = true;
            waterTouched = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //4 = water layer
        if (other.gameObject.layer == 4)
        {
            touchingWater = false;
            Debug.Log("Exited the water");
        }
    }
}
