using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAnim
{
    Idle, Move, Jump, Dash, Death, Damaged
}

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] 
    private Animator Animator;

    private int mControlKey;

    public void Init()
    {
        mControlKey = Animator.GetParameter(0).nameHash;
    }

    public void ChangeState(PlayerAnim anim)
    {
        Animator.SetInteger(mControlKey, (int)anim);
    }
}
