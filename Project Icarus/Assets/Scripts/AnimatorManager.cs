using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    Animator animator;
    int horizontal;
    int vertical;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement)
    {

        //Animation Snapping
        float snappedHorizontal;
        float snappedVertical;

        #region snapped horizontal
        if (horizontalMovement > 0 && horizontalMovement < 0.55f)
        {
            snappedHorizontal = 0.5f;
        } else if (verticalMovement > 0.55f)
        {
            snappedHorizontal = 1;
        } else if (horizontalMovement < 0 && horizontalMovement > -.55f)
        {
            snappedHorizontal = -.5f;
        } else if (horizontalMovement < -.5f)
        {
            snappedHorizontal = -1f;
        } else
        {
            snappedHorizontal = 0;
        }

        #endregion

        #region Snapped Vertical

        if (verticalMovement > 0 && verticalMovement < 0.55f)
        {
            snappedVertical = 0.5f;
        }
        else if (verticalMovement > 0.55f)
        {
            snappedVertical = 1;
        }
        else if (verticalMovement < 0 && verticalMovement > -.55f)
        {
            snappedVertical = -.5f;
        }
        else if (verticalMovement < -.5f)
        {
            snappedVertical = -1f;
        }
        else
        {
            snappedVertical = 0;
        }

        #endregion

        animator.SetFloat(horizontal, snappedHorizontal, 0.1f,  Time.deltaTime);
        animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }
}
