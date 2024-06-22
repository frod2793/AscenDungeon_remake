using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Castle : Singleton<Castle>
{
    public Floor PlayerFloor => mPlayerFloor;

    [SerializeField]
    private bool DisableStageEvent;

    [HideInInspector] public bool CanPlayerFloorInit   = true;
    [HideInInspector] public bool CanPlayerFloorNotify = true;

    [SerializeField][Range(0.1f, 2f)]
    private float CameraMoveAccel;

    private bool mIsCastClearEvent;
    private bool mIsActivation = true;
    private bool mIsPause   = false;

    private Player mPlayer;

    private IEnumerator _EUpdate;
    private UnitizedPosV mLastPlayerPOS = UnitizedPosV.NONE;

    [Header("Floor")]
    [SerializeField] private   Floor[] mFloors;
    [SerializeField] private   Floor   mPlayerFloor;
                     private Vector2[] mMovePoints;

    [SerializeField] private GameObject _DungeonClearUI;

    #region READ
    /// <summary>
    /// 현재 층안에 존재하는 이동 지점의 위치를 반한합니다.
    /// </summary>
    #endregion 
    public Vector2 GetMovePoint(UnitizedPos direction)
    {
        return mMovePoints[(int)direction];
    }
    public float GetMovePointY(UnitizedPosV posV)
    {
        switch (posV)
        {
            case UnitizedPosV.TOP: return mMovePoints[0].y;
            case UnitizedPosV.MID: return mMovePoints[3].y;
            case UnitizedPosV.BOT: return mMovePoints[6].y;
            
            default: return 0f;
        }
    }

    #region READ
    /// <summary>
    /// 다음 층으로 이동할 수 있는지의 여부를 반환합니다.
    /// </summary>
    #endregion 
    public bool CanNextPoint()
    {
        return (!IsIndexOutFloor(mPlayerFloor.FloorIndex)) && mPlayerFloor.IsClear;
    }

    #region READ
    /// <summary>
    /// 다음 층으로 이동할 수 있는지의 여부를 반환합니다.
    /// </summary>
    /// <param name="point">다음층의 이동 지점을 저장할 변수를 사용합니다</param>
    /// <returns></returns>
    #endregion 
    public bool CanNextPoint(out Vector2 point)
    {
        point = Vector2.zero;

        if (CanNextPoint())
        {
            mPlayerFloor = mFloors[mPlayerFloor.FloorIndex];

            float playerX = mPlayer.transform.position.x;

            point = new Vector2(playerX, mPlayerFloor.GetMovePoints(UnitizedPosV.BOT)[0].y);

            RenewPlayerFloor();
            return true;
        }
        return false;
    }
    // === Cheat ===
    public void SetPlayerFloor(int floor)
    {
        mPlayerFloor.Disable();
        mPlayerFloor = mFloors[floor - 1];

        mPlayer.transform.position = 
            mPlayerFloor.GetMovePoints(mPlayer.GetUnitizedPosV())[(int)mPlayer.GetTPOSITION3()];

        RenewPlayerFloor();

        MainCamera.Instance.Move(mPlayerFloor.transform.position, CameraMoveAccel);
    }
    // === Cheat ===

    public bool CanPrevPoint()
    {
        return (!IsIndexOutFloor(mPlayerFloor.FloorIndex - 2)) && mPlayerFloor.IsClear;
    }
    public bool CanPrevPoint(out Vector2 point)
    {
        point = Vector2.zero;

        if (CanPrevPoint())
        {
            mPlayerFloor = mFloors[mPlayerFloor.FloorIndex - 2];

            float playerX = mPlayer.transform.position.x;

            point = new Vector2(playerX, mPlayerFloor.GetMovePoints(UnitizedPosV.TOP)[0].y);

            RenewPlayerFloor();
            return true;
        }
        return false;
    }

    public void PauseEnable() => mIsPause = true;

    public void PauseDisable() => mIsPause = false;

    public Room[] GetFloorRooms()
    {
        return mPlayerFloor.GetRooms();
    }

    public Room GetPlayerRoom()
    {
        return mPlayerFloor.GetRooms()[(int)mPlayer.GetUnitizedPosV()];
    }

    public IObject GetLongestIObject(Vector2 comparePos)
    {
        float Distance(IObject o)
        {
            return Vector2.Distance(o.ThisObject().transform.position, comparePos);
        }
        float longestDistance = 0f;

        IObject selected = null;

        for (int i = 0; i < 3; i++)
        {
            var list = mPlayerFloor.GetRooms()[i].GetIObjects();

            if (list.Count > 0) {

                var select = list.OrderBy(o => -Distance(o)).FirstOrDefault();

                if (select != null)
                {
                    float selectDistance 
                        = Distance(select);

                    if (selectDistance > longestDistance)
                    {
                        selected = select;

                        longestDistance = selectDistance;
                    }
                }
            }
        }
        return selected;
    }

    // 입력한 월드좌표를 현재 활성화된 층 상의 좌표로 변환한 값을 반환합니다
    public Vector2 PointToFloorPoint(Vector2 point)
    {
        return (Vector2)mPlayerFloor.transform.position + point;
    }

    #region _MEMBER
    /// <summary>
    /// 멤버함수 : 지정한 인덱스의 층이 존재하는지의 여부를 반환합니다.
    /// </summary>
    #endregion 
    private bool IsIndexOutFloor(int floorNumber)
    {
        return (mFloors.Length <= floorNumber || floorNumber < 0);
    }

    private void BuildCastle()
    {
        for (int i = 0; i < mFloors.Length; ++i)
        {
            mFloors[i].BuildRoom();
        }
    }

    #region _MEMBER
    /// <summary>
    /// 플레이어가 존재하는 층인 mPlayerFloor를 가동시키고,
    /// <para>멤버변수들의 정보를 mPlayerFloor에 대한 정보로 갱신합니다.</para>
    /// </summary>
    #endregion 
    private void RenewPlayerFloor()
    {
        if (CanPlayerFloorInit)
        {
            PlayerFloorInit();
        }
        if (CanPlayerFloorNotify)
        {
            PlayerFloorNotify();
        }
    }
    public void PlayerFloorInit()
    {
        mPlayerFloor.IInit();
        Vector2[] topMovePoint = mPlayerFloor.GetMovePoints(UnitizedPosV.TOP);
        Vector2[] midMovePoint = mPlayerFloor.GetMovePoints(UnitizedPosV.MID);
        Vector2[] botMovePoint = mPlayerFloor.GetMovePoints(UnitizedPosV.BOT);

        mMovePoints = new Vector2[(int)UnitizedPos.END]
        {
            topMovePoint[0], topMovePoint[1], topMovePoint[2],
            midMovePoint[0], midMovePoint[1], midMovePoint[2],
            botMovePoint[0], botMovePoint[1], botMovePoint[2]
        };
    }
    public void PlayerFloorNotify()
    {
        mPlayerFloor.EnterPlayer(mPlayer);
        MainCamera.Instance.Move(mPlayerFloor.transform.position, CameraMoveAccel);

        if (mPlayer)
        {
            RenewPlayerPOS();
        }
        if (!DisableStageEvent)
        {
            StageEventLibrary.Instance?.NotifyEvent(NotifyMessage.StageEnter);
        }
        Inventory.Instance.PlayerEnterFloor();
        mIsCastClearEvent = false;
    }

    #region _MEMBER
    /// <summary>
    /// 현재 플레이어가 있는 층을 가리키는 mPlayerFloor를 가동시키고,
    /// <para>멤버변수들의 정보를 mPlayerFloor에 대한 정보로 갱신합니다.</para>
    /// </summary>
    #endregion
    private void RenewPlayerPOS()
    {
        if (mLastPlayerPOS != mPlayer.GetUnitizedPosV())
        {
            if (mLastPlayerPOS != UnitizedPosV.NONE)
            {
                mPlayerFloor.ExitPlayer(MESSAGE.THIS_ROOM, mLastPlayerPOS);
            }
            mPlayerFloor.EnterPlayer(mPlayer, mLastPlayerPOS = mPlayer.GetUnitizedPosV());
        }
    }

    private IEnumerator EUpdate()
    {
        while (mIsActivation)
        {
            if (!mIsPause)
            {
                if (mPlayer)
                {
                    RenewPlayerPOS();
                }
                if (mPlayerFloor)
                {
                    mPlayerFloor.IUpdate();
                }
                if (mPlayerFloor.IsClear && !mIsCastClearEvent)
                {
                    mIsCastClearEvent = true;

                    if (!DisableStageEvent) 
                    {
                        StageEventLibrary.Instance.NotifyEvent(NotifyMessage.StageClear);
                    }
                }
                if (!CanNextPoint() && mPlayerFloor.IsClear && !DisableStageEvent)
                {
                    _DungeonClearUI?.SetActive(true);
                }
            }
            yield return null;
        }
        yield break;
    }
    public void ForceStopUpdate()
    {
        if (_EUpdate != null) {
            StopCoroutine(_EUpdate);
        }       
        _EUpdate = null;
    }
    public void ReStartUpdate()
    {
        StartCoroutine(_EUpdate = EUpdate());
    }
    private void Start()
    {
        mIsCastClearEvent = false;

        BuildCastle();

        mPlayer = FindObjectOfType(typeof(Player)) as Player;

        RenewPlayerFloor();

        StartCoroutine(_EUpdate = EUpdate());
    }
}
