using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : Interactable
{
    private bool goUp = false;

    public override void OnFocus()
    {
        print("LOOKING AT " + gameObject.name);
    }

    public override void OnInteract()
    {
        goUp = !goUp;
    }

    public override void OnLoseFocus()
    {
        print("STOPPED LOOKING AT " + gameObject.name);
    }

    private void Update()
    {
        if (goUp)
        {
            transform.position += new Vector3(0, 0.05f, 0);
        }
    }
}
