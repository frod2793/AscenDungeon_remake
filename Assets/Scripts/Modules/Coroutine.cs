using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Comment
/// <summary>
/// <b>유니티에서 제공하는 코루틴 기능의 제어를 보조합니다</b>
/// <para></para>
/// - 하나의 루틴만이 실행되는 것을 보장합니다.
/// <br></br>
/// - 진행중인 루틴이 종료되었는지의 여부를 확인할 수 있습니다.
/// <br></br>
/// <i>! 진행한 루틴이 종료되었을 때에는, 반드시 Finish()함수를 호출해야 합니다.</i>
/// <br></br>
/// </summary>
#endregion
public class Coroutine
{
    private IEnumerator _Routine;
    private MonoBehaviour _User;

    public Coroutine(MonoBehaviour user)
    {
        _User = user;
    }

    #region Comment
    /// <summary>실행한 루틴이 종료되는 시점에 반드시 호출해야 합니다</summary>
    #endregion
    public void Finish()
    {
        _Routine = null;
    }
    #region Comment
    /// <summary>진행중인 루틴의 여부를 확인합니다</summary>
    /// <returns>현재 진행중인 루틴이 없거나, 루틴이 종료되었는지의 여부를 반환합니다</returns>
    #endregion
    public bool IsFinished()
    {
        return _Routine == null;
    }

    public void StopRoutine()
    {
        if (_Routine != null)
        {
            _User.StopCoroutine(_Routine);
            _Routine = null;
        }
    }
    public void StartRoutine(IEnumerator routine)
    {
        StopRoutine();

        _User.StartCoroutine(_Routine = routine);
    }
}

