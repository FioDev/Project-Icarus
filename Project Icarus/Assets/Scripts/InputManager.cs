using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    CharacterControls characterControls;

    AnimatorManager animatorManager;

    float moveAmmount;

    [SerializeField] Vector2 movementInput;
    float verticalInput;
    float horizontalInput;

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void OnEnable()
    {
        //Start listening to input
        if (characterControls == null)
        {
            characterControls = new CharacterControls();

            //translate a performed movement on the CharacterControls scheme into a value on movementinput
            characterControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

        }

        characterControls.Enable();
    }

    private void OnDisable()
    {
        //if disabled, stop taking inputs
        characterControls.Disable();
    }

    private void handleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
        moveAmmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmmount);
    }

    public void HandleAllInputs()
    {
        handleMovementInput();
        //HandleJumpInput
        //HandleActionInput
    }


    //Getters and setters

    public float GetVerticalInput()
    {
        return verticalInput;
    }

    public float GetHorizontalInput()
    {
        return horizontalInput;
    }

}
