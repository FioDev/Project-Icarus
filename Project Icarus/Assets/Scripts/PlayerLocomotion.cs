using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{

    InputManager inputManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody characterRigidbody;

    public float movementSpeed = 7f;
    public float rotationSpeed = 15f;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        characterRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        
        //Sets the forward speed to camera forward x the input manager's Vertical
        //Sets the right speed to camera right x the input manager's Horizontal
        //Normalises
        //Then kills Y Velocity

        //Then attaches it to a rigidbody

        moveDirection = cameraObject.forward * inputManager.GetVerticalInput();
        moveDirection = moveDirection + cameraObject.right * inputManager.GetHorizontalInput();
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection = moveDirection * movementSpeed;

        Vector3 movementVelocity = moveDirection;
        characterRigidbody.velocity = movementVelocity;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObject.forward * inputManager.GetVerticalInput();
        targetDirection = targetDirection + cameraObject.right * inputManager.GetHorizontalInput();

        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }
}
