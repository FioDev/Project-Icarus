using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPad : Interactable
{
    [Header("Teleporter Settings")]
    [SerializeField] private bool useRelative;
    [SerializeField] private Vector3 coordinates;
    
    

    public override void OnFocus()
    {
        
    }

    public override void OnInteract(FirstPersonControler interactor)
    {
        Vector3 destination = useRelative ? coordinates + interactor.transform.position : coordinates;

        interactor.UpdatePosition(destination);
    }

    public override void OnLoseFocus()
    {
        
    }
}
