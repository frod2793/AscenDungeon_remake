using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField][Tooltip("좌측 이동 지점입니다")] private Transform LMovePoint; 
    [SerializeField][Tooltip("중심 이동 지점입니다")] private Transform CMovePoint;
    [SerializeField][Tooltip("우측 이동 지점입니다")] private Transform RMovePoint; 

    private Floor mBelongFloor;

    public bool IsClear
    { 
        get
        {
            try
            { return mObjects.Count == 0; }
            catch
            { return false; }
        }
    }

    private Player  mPlayer;
    private MESSAGE mLastMessage;

    public int BelongFloorIndex
    {
        get
        {
            if (mBelongFloor != null)
            {
                return mBelongFloor.FloorIndex;
            }
            return 0;
        }
    }

    private List<IObject> mObjects;

    public void IInit(Floor parentFloor)
    {
        mBelongFloor = parentFloor;

        mObjects = new List<IObject>();

        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject childObject = transform.GetChild(i).gameObject;

            if (childObject.activeSelf) {
                if (childObject.TryGetComponent(out IObject Object))
                {
                    mObjects.Add(Object);

                    Object.IInit();
                }
            }
        }
    }

    public void IUpdate()
    {
        if (mObjects.Count > 0)
        {
            for (int i = 0; i < mObjects.Count; ++i)
            {                
                if (mObjects[i].IsActive())
                {
                    mObjects[i].IUpdate();
                }
                else
                {
                    mObjects.RemoveAt(i);

                    GameLoger.Instance.EnemyDead();
                }
            }
        }
    }

    public void EnterPlayer(MESSAGE message, Player enterPlayer)
    {
        mPlayer = enterPlayer;

        for (int i = 0; i < mObjects.Count; ++i)
        {
            mObjects[i].PlayerEnter(message, mPlayer);
        }
        mLastMessage = message;
    }
    public void ExitPlayer(MESSAGE message)
    {
        mPlayer = null;

        for (int i = 0; i < mObjects.Count; ++i)
        {
            mObjects[i].PlayerExit(message);
        }
        mLastMessage = message;
    }

    #region READ
    /// <summary>
    /// 해당 방에게 종속될 오브젝트를 추가합니다.
    /// <para>주로 방 내에 있는 개체가 새로운 개체를 만들어 냈을 때에 사용됩니다.</para>
    /// </summary>
    /// <param name="object">해당 방에 추가될 오브젝트의 IObject인터페이스를 지정합니다</param>
    #endregion
    public void AddIObject(IObject @object)
    {
        mObjects.Add(@object);

        @object.IInit();

        if (mPlayer == null)
        {
            @object.PlayerExit(mLastMessage);
        }
        else
        {
            @object.PlayerEnter(mLastMessage, mPlayer);
        }
    }

    public List<IObject> GetIObjects()
    {
        return mObjects;
    }

    public bool IsBelongThis(IObject iobject)
    {
        return mObjects.Any(o => o == iobject);
    }

    public Vector2[] GetMovePoints()
    {
        return new Vector2[3] { LMovePoint.position, CMovePoint.position, RMovePoint.position };
    }
}
