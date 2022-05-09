using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{

    //Variables

    CharacterController _cc;
    Animator _animator;
    PlayerInput _playerInput; //Generated from Input System Class

    //Getter Setter parameter IDs
    int _isWalkingHash;
    int _isRunningHash;
    int _isFallingHash;
    int _isJumpingHash;

    //Player Inputs
    Vector2 _currentMovementInput;
    Vector2 _currentMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;
    bool _isRunPressed;

    //Constants
    float _rotationFactorPerFrame = 15.0f;
    float _runMultiplier = 4.0f;
    int _zero = 0;

    //Gravity Variables
    float _gravity = -9.8f;

    //Jumping Variables
    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 4.0f;
    float _maxJumpTime = .75f;
    bool _isJumping = false;
    bool _requireNewJumpPress = false;


    //State Variable
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    //Getters and setters
    public CharacterController CC { get { return _cc; } }
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public Animator Animator { get { return _animator; } }
    public int IsFallingHash { get { return _isFallingHash;} }
    public int IsJumpingHash { get { return _isJumpingHash; } }
    public int IsWalkingHash { get { return _isWalkingHash;} }
    public int IsRunningHash { get { return _isRunningHash; } }
    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; } }
    public bool IsJumping { set { _isJumping = value; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } }
    public bool IsMovementPressed { get { return _isMovementPressed; } }
    public bool IsRunPressed { get { return _isRunPressed; } }
    public float Gravity { get { return _gravity; } }
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public float CurrentMovementX { get { return _currentMovement.x; } set { _currentMovement.x = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }    
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value;} }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
    public float RunMultiplier { get { return _runMultiplier; } set { _runMultiplier = value; } }
    public Vector2 CurrentMovementInput { get { return _currentMovementInput; } set { _currentMovementInput = value; } }
    
    void Awake()
    {
        //Initially set reference variables
        _playerInput = new PlayerInput();
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        //Setup state
        _states = new PlayerStateFactory(this); //State Factory handles switching state
        _currentState = _states.Grounded(); //Sets the current state
        _currentState.EnterState(); //enters it

        //Parameter hash references - makes it easier for the animator to reffer to variables
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");

        //Set player input callbacks - makes sure character inputs function as expected
        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;
        _playerInput.CharacterControls.Run.started += OnRun;
        _playerInput.CharacterControls.Run.canceled += OnRun;
        _playerInput.CharacterControls.Jump.started += OnJump;
        _playerInput.CharacterControls.Jump.canceled += OnJump;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();

        //Invoke current state logic
        _currentState.UpdateStates();

        //Moves the character at the end of applied movement
        _cc.Move(_appliedMovement * Time.deltaTime);
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;
        //Change the position our character points to
        positionToLookAt.x = _currentMovementInput.x;
        positionToLookAt.y = _zero;
        positionToLookAt.z = _currentMovementInput.y;

        //Current Rotation Of Character
        Quaternion currentRotation = transform.rotation;

        //Rotation logic
        if (_isMovementPressed)
        {
            //Create new rotation based on input
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);

            //Lerp from old to new by rotation factor per frame
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }

    //Callback Handler function to set player input values
    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }

    //Callback handler function for jump buttons
    void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }

    //Callback handler function for run buttons
    void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnEnable()
    {
        //Enable character controls Action Map
        _playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        //Disable character controls action map
        _playerInput.CharacterControls.Disable();
    }

}
