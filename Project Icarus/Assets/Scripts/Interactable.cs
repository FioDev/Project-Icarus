using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void awake()
    {
        gameObject.layer = 8; 
        //8 is the index of the "interactable" layer. If this changes, this needs to change too - or everything breaks
    }
    public abstract void OnInteract(FirstPersonControler interactor);
    public abstract void OnFocus();
    public abstract void OnLoseFocus();
}
