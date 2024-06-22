using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator Animator;

    private int mControlKey;

    private void Reset()
    {
        TryGetComponent(out Animator);
    }

    public void Init()
    {
        mControlKey = Animator.GetParameter(0).nameHash;
    }

    public void ChangeState(AnimState anim)
    {
        Animator.SetInteger(mControlKey, (int)anim);
    }

    public AnimState CurrentState()
    {
        return (AnimState)Animator.GetInteger(mControlKey);
    }
}
