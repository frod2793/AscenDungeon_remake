using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    MoveHorizontal,
    MoveVertical,
    Dash
}

public class PlayerActionManager : Singleton<PlayerActionManager>
{
    private Dictionary<PlayerAction, bool> _ActionLocker = new Dictionary<PlayerAction, bool>();

    /// <summary>
    /// 플레이어 조작키의 활성화 여부.
    /// </summary>
    public bool EnableController
    {
        get => _EnableController;
    }
    private bool _EnableController = true;

    /// <summary>
    /// 해당 동작의 잠김 여부를 반환한다.
    /// </summary>
    public bool this[PlayerAction action]
    {
        get
        {
            bool value = false;

            if (!_ActionLocker.TryGetValue(action, out value))
            {
                _ActionLocker.Add(action, false);
                return false;
            }
            return value;
        }
    }

    /// <summary>
    /// 해당 동작의 잠김 여부를 설정한다.
    /// </summary>
    public void SetActionLock(PlayerAction action, bool isLock)
    {
        Debug.Log($"SetActionLock :: {action} : {isLock}");
        if (_ActionLocker.ContainsKey(action))
        {
            _ActionLocker[action] = isLock;
        }
        else
        {
            _ActionLocker.Add(action, isLock);
        }
    }

    /// <summary>
    /// 이동 동작의 잠김 여부를 설정한다.
    /// </summary>
    public void SetMoveLock(bool isLock)
    {
        SetActionLock(PlayerAction.MoveHorizontal, isLock);
        SetActionLock(PlayerAction.MoveVertical, isLock);
        SetActionLock(PlayerAction.Dash, isLock);
    }

    /// <summary>
    /// 플레이어 조작키를 활성화/비활성화한다.
    /// </summary>
    public void SetEnableController(bool isEnable)
    {
        _EnableController = isEnable;
    }

    /// <summary>
    /// 입력한 방향으로의 이동이 가능한지 반환한다.
    /// </summary>
    public bool IsLockedAction(Direction moveDir)
    {
        switch (moveDir)
        {
            case Direction.Up:
            case Direction.Down:
                return this[PlayerAction.MoveVertical];

            case Direction.Left:
            case Direction.Right:
                return this[PlayerAction.MoveHorizontal];
        }
        return false;
    }
}
