using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialBase : MonoBehaviour
{
    [SerializeField] protected PlayerAction[] BeginLockActions;
    [SerializeField] protected PlayerAction[] EndUnlockActions;

    public event Action OnTutorialClearEvent;

    /// <summary>
    /// 튜토리얼을 진행한다~
    /// </summary>
    public virtual void StartTutorial()
    {
        for (int i = 0; i < BeginLockActions.Length; ++i) {
            PlayerActionManager.Instance.SetActionLock(BeginLockActions[i], true);
        }   
    }

    /// <summary>
    /// 튜토리얼 클리어 판정 (튜토리얼 끝!)
    /// </summary>
    public virtual void EndTutorial()
    {
        for (int i = 0; i < EndUnlockActions.Length; ++i) {
            PlayerActionManager.Instance.SetActionLock(EndUnlockActions[i], false);
        }
        OnTutorialClearEvent?.Invoke();
    }
}
