using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField]
    private Room[] mMemberRooms = new Room[3];

    [SerializeField]
    [Tooltip("높이있는 방일수록 가장 작은 인덱스에 저장해 주세요!")]
    private Transform[] mRoomPoints = new Transform[3];

    [SerializeField] private bool mIsSPECIALFloor;

                     public  int  FloorIndex => mFloorIndex;
    [SerializeField] private int mFloorIndex;

    public  bool  IsClear => mIsClear;
    private bool mIsClear;

    // === Cheat ===
    public void Disable()
    {
        mIsClear = true;

        for (int i = 0; i < 3; i++)
        {
            mMemberRooms[i].ExitPlayer(MESSAGE.BELONG_FLOOR);
            mMemberRooms[i].gameObject.SetActive(false);
        }
    }
    // === Cheat ===

    public void IInit()
    {
        mMemberRooms[0].gameObject.SetActive(true);
        mMemberRooms[1].gameObject.SetActive(true);
        mMemberRooms[2].gameObject.SetActive(true);

        mIsClear = false;
    }

    public void IUpdate()
    {
        if (!mIsClear)
        {
            int clearRoomCount = 0;

            for (int i = 0; i < mMemberRooms.Length; ++i)
            {
                mMemberRooms[i].IUpdate();

                if (mMemberRooms[i].IsClear)
                {
                    clearRoomCount++;
                }
            }
            if (clearRoomCount == mMemberRooms.Length)
            {
                mIsClear = true;
            }
        }
    }

    #region READ
    /// <summary>
    /// 해당 층을 구성할 방 객체들을 생성합니다.
    /// </summary>
    #endregion
    public void BuildRoom()
    {
        if (!mIsSPECIALFloor)
        {
            mMemberRooms[0] = Instantiate(RoomLibrary.Instance.Random(), mRoomPoints[0].position, Quaternion.identity);
            mMemberRooms[1] = Instantiate(RoomLibrary.Instance.Random(), mRoomPoints[1].position, Quaternion.identity);
            mMemberRooms[2] = Instantiate(RoomLibrary.Instance.Random(), mRoomPoints[2].position, Quaternion.identity);
        }
        mMemberRooms[0].gameObject.SetActive(false);
        mMemberRooms[1].gameObject.SetActive(false);
        mMemberRooms[2].gameObject.SetActive(false);
    }

    public void EnterPlayer(Player player)
    {
        mMemberRooms[0].IInit(this);
        mMemberRooms[1].IInit(this);
        mMemberRooms[2].IInit(this);

        mMemberRooms[0].gameObject.SetActive(true);
        mMemberRooms[1].gameObject.SetActive(true);
        mMemberRooms[2].gameObject.SetActive(true);

        mMemberRooms[0].EnterPlayer(MESSAGE.BELONG_FLOOR, player);
        mMemberRooms[1].EnterPlayer(MESSAGE.BELONG_FLOOR, player);
        mMemberRooms[2].EnterPlayer(MESSAGE.BELONG_FLOOR, player);
    }
    public void EnterPlayer(Player player, UnitizedPosV position)
    {
        mMemberRooms[(int)position].EnterPlayer(MESSAGE.THIS_ROOM, player);
    }
    public void ExitPlayer(MESSAGE message, UnitizedPosV position)
    {
        switch (message)
        {
            case MESSAGE.THIS_ROOM:
                mMemberRooms[(int)position].ExitPlayer(MESSAGE.THIS_ROOM);
                break;

            case MESSAGE.BELONG_FLOOR:
                mMemberRooms[0].ExitPlayer(MESSAGE.BELONG_FLOOR);
                mMemberRooms[1].ExitPlayer(MESSAGE.BELONG_FLOOR);
                mMemberRooms[2].ExitPlayer(MESSAGE.BELONG_FLOOR);
                break;
        }
    }

    #region READ
    /// <summary>
    /// 지정한 방에 존재하는 이동지점들을 반환합니다.
    /// </summary>
    #endregion
    public Vector2[] GetMovePoints(UnitizedPosV position)
    {
        return mMemberRooms[(int)position].GetMovePoints();
    }

    public Room[] GetRooms()
    {
        return new Room[3] { mMemberRooms[0], mMemberRooms[1], mMemberRooms[2] };
    }
}
