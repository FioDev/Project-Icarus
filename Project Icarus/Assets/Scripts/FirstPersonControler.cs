using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonControler : MonoBehaviour
{
    //Toggle for can move, defaults to true
    public bool CanMove { get; private set; } = true;
    //Only true if cansprint and sprint key is down
    private bool isSprinting => canSprint && Input.GetKey(sprintKey);
    private bool shouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    [Header("Aesthetic Options")]
    [SerializeField] private bool canUseHeadbob = true;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canSlide = true;
    [SerializeField] private bool canInteract = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 9.8f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCentre = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCentre = new Vector3(0, 0, 0);

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    [SerializeField, Range(0, 5)] private float bobStrengthMultiplier = 0.5f;
    [SerializeField, Range(0, 5)] private float bobSpeedMultiplier = 1f;
    private float defaultCamYPos = 0;
    private float timer;

    //Sliding params
    private Vector3 hitPointNormal; //Angle of the floor below slide
    private bool isSliding //wether sliding or not
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
                //check angle, if more than slope angle, slide - and return true
            } else
            {
                return false; //if none of those are true, you shouldnt be sliding
            }
        }
    }

    //Interaction perams
    [Header("Interaction Perameters")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    //private reference variables
    private Camera playerCamera;
    private CharacterController characterController;

    //movement
    private Vector3 moveDirection;
    private Vector2 currentInput;

    //crouching
    private bool isCrouching;
    private bool duringCrouchAnimation;

    //To be clamped by the upper / lower look limits
    private float rotationX = 0;


    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        defaultCamYPos = playerCamera.transform.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Guard clause, only update if you can move
        if (!CanMove)
        {
            return;
        }

        //Call movement scripts
        HandleMovementInput();
        HandleMouseLook();

        if (canJump)
        {
            HandleJump();
        }

        if (canCrouch)
        {
            HandleCrouch();
        }

        if (canUseHeadbob)
        {
            HandleHeadbob();
        }

        if (canInteract)
        {
            HandleInteractionCheck();
            HandleInteractionInput();
        }

        //Apply movement
        ApplyFinalMovement();

    }

    private void HandleMovementInput()
    {
        //Checks if is sprinting, if yes, multiply by sprint speed. If no, by walk speed, also crouch speed
        currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
        
        //Cache move direction y, so vertical can be preserved
        float moveDirectionY = moveDirection.y;

        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        //restore cached Y movement
        moveDirection.y = moveDirectionY;
    }

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

    private void HandleJump()
    {
        if (shouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch()
    {
        if (shouldCrouch)
        {
            //If should crouch, begin crouch / stand corotine
            StartCoroutine(CrouchStand());
        }
    }

    private void HandleHeadbob()
    {
        //guard clause - if not grounded, no bobbing
        if (!characterController.isGrounded)
        {
            return;
        }

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            //increment timer by movement type (crouch / sprint / walk)
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed) * bobSpeedMultiplier;

            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultCamYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount) * bobStrengthMultiplier,
                playerCamera.transform.localPosition.z
                );
        }
    }

    //Constantly raycast for interactable objects within range
    private void HandleInteractionCheck()
    {
        //This considers all colliders
        //GAIN FOCUS
        Debug.DrawRay(playerCamera.ViewportPointToRay(interactionRayPoint).origin, playerCamera.ViewportPointToRay(interactionRayPoint).direction, Color.red, interactionDistance);
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            //"if theres an object on layer 8 (an interactable)
            if (hit.collider.gameObject.layer == 8) 
            {
                //...*and* check that either the current interactable is null, *OR* the interactable being hovered over and the one currently selected arent the same
                if (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.gameObject.GetInstanceID())
                {
                    //If that's passed, run the code
                    //Save the interactable from this collider to "current Interactable"
                    hit.collider.TryGetComponent(out currentInteractable);

                    //If we do indeed now have an interactable, focus it (as it is the first frame)
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnFocus();
                    }
                }
            }
        } else if (currentInteractable) //LOSE FOCUS - if we have an interactable and the raycast fails to hit *any* interactable, its lost focus
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    //This will be "interact key" for some form of action
    private void HandleInteractionInput()
    {
        //if we have key down, and interact isnt null, raycast it out from the camera on layer Interactable Layer
        //If all checks are good, do the interact
        if (Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }
    }

    private void ApplyFinalMovement()
    {
        //Do gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= (gravity * Time.deltaTime);
        }

        //Slope code
        if (canSlide && isSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);

    }

    private IEnumerator CrouchStand()
    {
        //Guard clause for ceiling (if there is something above me, do not stand up
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1.0f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        float timeElapsed = 0f;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCentre = isCrouching ? standingCentre : crouchingCentre;
        Vector3 currentCentre = characterController.center;

        //Over the time of "time to crouch", defined above, lerp from whatever current height is, to whatever height should be
        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCentre, targetCentre, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Vibe check height, as time is not precise
        characterController.height = targetHeight;
        characterController.center = targetCentre;

        //toggle crouch state
        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}
