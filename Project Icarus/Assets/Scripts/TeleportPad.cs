using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPad : Interactable
{
    [Header("Teleporter Settings")]
    [SerializeField] private Vector3 coordinates;
    
    

    public override void OnFocus()
    {
        
    }

    public override void OnInteract(FirstPersonControler interactor)
    {
        interactor.UpdatePosition(coordinates);
    }

    public override void OnLoseFocus()
    {
        
    }
}
