using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomLibrary : Singleton<RoomLibrary>
{
    [SerializeField]
    private List<Room> mRooms;

    #region READ
    /// <summary>
    /// RoomLibrary에 등재된 무작위 방을 반환합니다. 반환값은 중복될 수 있습니다.
    /// </summary>
    #endregion
    public Room Random()
    {
        int randomIndex = UnityEngine.Random.Range(0, mRooms.Count);

        return mRooms[randomIndex];
    }
}
