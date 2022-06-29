using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FirstPersonControler : MonoBehaviour
{
    private enum DoubleJumpRecoveryMode
    {
        OnGround,
        OverTime,
        UsingStamina
    }

    private enum DoubleJumpWindow
    {
        Ascending,
        Descending,
        HeightAboveGround
    }

    //Toggle for can move, defaults to true
    public bool CanMove { get; private set; } = true;


    //Only true if cansprint and sprint key is down
    private bool isSprinting => canSprint && Input.GetKey(sprintKey);
    private bool shouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private bool shouldDoubleJump
    {
        get
        {
            //Basic Checks
            if (Input.GetKeyDown(jumpKey) && !characterController.isGrounded && currentExtraJumps > 0)
            {
                switch (doubleJumpWindow)
                {
                    case DoubleJumpWindow.Ascending:
                        if (moveDirection.y > 0)
                        {
                            return true;
                        } else
                        {
                            return false;
                        }
                    
                    case DoubleJumpWindow.Descending:
                        if (moveDirection.y <= 0)
                        {
                            return true;
                        } else 
                        { 
                            return false; 
                        }

                    case DoubleJumpWindow.HeightAboveGround:
                        if (Physics.Raycast(transform.position, Vector3.down, characterController.height + doubleJumpMinHeightOffGround))
                        {
                            return true;
                        } else
                        {
                            return false;
                        }
                }
            }
            return false;
        }
    }

    [Header("Aesthetic Options")]
    [SerializeField] private bool canUseHeadbob = true;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canSlide = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool canFly = true;
    [SerializeField] private bool canSwim = true;
    [SerializeField] private bool canDoubleJump = true;

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
    private bool groundedLastFrame = true;

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

    [Header("Double Jump Parameters")]
    [SerializeField] private bool doubleJumpAsDash = false;
    [SerializeField] private int maxExtraJumps = 1;
    [SerializeField] private DoubleJumpRecoveryMode doubleJumpRecoveryMode = DoubleJumpRecoveryMode.OnGround; //When double jump will return
    [SerializeField] private float doubleJumpStaminaCost = 1f;
    [SerializeField] private float doubleJumpTimeToRecover = 1f;
    [SerializeField] private DoubleJumpWindow doubleJumpWindow = DoubleJumpWindow.Descending;
    [SerializeField] private float doubleJumpMinHeightOffGround = 1f;
    [SerializeField, Range(0.5f, 3)] private float doubleJumpStrengthMultipliar = 1f;
    private int currentExtraJumps = 1;

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

        if (canDoubleJump)
        {
            HandleDoubleJump();
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

    #region movement methods

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

    private void HandleDoubleJump()
    {
        if (shouldDoubleJump)
        {
            moveDirection.y = jumpForce * doubleJumpStrengthMultipliar;
            currentExtraJumps--;
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
            currentInteractable.OnInteract(this);
        }
    }

    private void HandleGravity()
    {
        //Do gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= (gravity * Time.deltaTime);
            groundedLastFrame = false;
        }
        //Kill Y movement when you hit the floor, only on the next frame.
        //Update: killing Y movement entirely makes unity forget the chracter controler is touching the ground
        //Solution: Make it dt * gravity, so that there is a *tiny* bit of Y movement, but SET it to that, not add it as you do in "not grounded"
        //That way, the ground is "within" the skin width of the character controler
        //REASON THIS IS NEEDED: If you dont set it to this, and have the groundedLastFrame bool trigger between this, you get an acumulating amount of negative y momentum
        //This is because the "target" y movement does not reset when you touch the ground.
        //Adendum: i hate floating point inaccuracy
        if (characterController.isGrounded && !groundedLastFrame)
        {
            moveDirection.y = -(gravity * Time.deltaTime);
            groundedLastFrame = true;
        }
    }

    private void RecoverDoubleJumps()
    {
        switch (doubleJumpRecoveryMode)
        {
            case DoubleJumpRecoveryMode.OverTime:
                StartCoroutine(RecoverDoubleJumpOverTime());
                break;

            case DoubleJumpRecoveryMode.UsingStamina:
                //TODO: IMPLEMENT STAMINA SYSTEM.
                break;

            case DoubleJumpRecoveryMode.OnGround:
                if (characterController.isGrounded)
                {
                    currentExtraJumps = maxExtraJumps;
                }
                break;
        }
    }

    #endregion

    private void ApplyFinalMovement()
    {
        HandleGravity();

        RecoverDoubleJumps();

        //Slope code
        if (canSlide && isSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);

    }

    #region coroutines

    private IEnumerator RecoverDoubleJumpOverTime()
    {
        float timeElapsed = 0f;
        //Wait set time
        while (timeElapsed < doubleJumpTimeToRecover)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        //Add a jump
        currentExtraJumps++;
        
        //If not full yet, recurse.
        if (currentExtraJumps < maxExtraJumps)
        {
            StartCoroutine(RecoverDoubleJumpOverTime());
        }

        yield break;
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
        while (timeElapsed < timeToCrouch)
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

    #endregion


    #region public methods
    public void UpdatePosition(Vector3 newPosition)
    {
        //Flashes cc on, then off, and in the in between, changes the position
        characterController.enabled = false;
        transform.position = newPosition;
        characterController.enabled = true;
    }
    #endregion
}
