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
    Crouch
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

    [Header("Movement Slide Responsivity")]
    [SerializeField] private float movementSensitivity = 1.0f;

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

    //Storage variables
    #region storage variables
    private float rotationX = 0;
    private Vector3 currentMovement = Vector3.zero; //The actual current movement of the players character
    private Vector2 oldHorizontalMovement = Vector2.zero; //The "last frame" 2D value stored in "Current movement"
    private Vector2 targetMovement = Vector2.zero; //The ideal target movement of the player's character
    private Vector3 horizontalInput = Vector3.zero; //Horizontal input rotated to be the correct orientation
    private Vector3 rawHorizontalInput = Vector3.zero; // the 2D input of the WASD keys ranging from 1 to -1
    private float currentTargetSpeed = 0f; //The speed to multiply horizontal input by to gain target movement
    private SpeedType speedState = SpeedType.Walk;


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

        //Handle Movement
        if (canMove)
        {
            GetHorizontalInput();
            HandleMovement();
        }
    }

    #region Initial Movement Helpers
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
        rawHorizontalInput = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));

        horizontalInput = (transform.TransformDirection(Vector3.forward) * rawHorizontalInput.x) + (transform.TransformDirection(Vector3.right) * rawHorizontalInput.y);
        horizontalInput = horizontalInput.normalized;
        horizontalInput.y = 0f;
    }

    #endregion

    private void HandleMovement()
    {
        HandleGravity();

        DetermineSpeedState();

        //Handles individual states of horizontal movement
        HandleHorizontalMovement();

        ApplyFinalMovement();
        
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
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speedState = SpeedType.Sprint;
                return;
            }
        }

        if (canCrouch)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                speedState = SpeedType.Crouch;
                return;
            }
        }

        if (canWalk)
        {
            if (Input.GetKey(KeyCode.CapsLock))
            {
                speedState = SpeedType.Walk;
                return;
            }
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
        oldHorizontalMovement = Vector2.Lerp(oldHorizontalMovement, targetMovement, movementSensitivity * Time.deltaTime);

        //apply to currentmovement, ignoring Y component
        currentMovement.x = oldHorizontalMovement.x;
        currentMovement.z = oldHorizontalMovement.y;

        //Send to character controler
        characterController.Move(currentMovement * Time.deltaTime);

    }
}
