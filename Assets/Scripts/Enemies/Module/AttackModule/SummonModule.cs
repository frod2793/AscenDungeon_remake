using UnityEngine;

public class SummonModule : AttackModule
{
    [SerializeField] private Area _Range;

    [Header("SummonSection")]
    [SerializeField] private GameObject _SummonTarget;
    [SerializeField] private Vector2    _SummonOffset;

    [Space(15f)] 
    [SerializeField] private GameObject[] _AdditionalSummonTarget;

    private Room _BelongRoom;

    public bool RangeHasAny()
    {
        return _Range.HasAny();
    }
    public void RunningDrive()
    {
        _Range.SetScale(_AbilityTable.Range);


        var rooms = Castle.Instance.GetFloorRooms();

        if (_User.TryGetComponent(out IObject _user))
        {
            for (int i = 0; i < rooms.Length; i++) {

                if (rooms[i].IsBelongThis(_user))
                {
                    
                    _BelongRoom = rooms[i]; break;
                }
            }
        }
    }
    public void SummonTargetedObject(Vector2 summonPosition)
    {
        Summon(_SummonTarget, summonPosition, _BelongRoom);
    }
    public void SummonAdditionObject(Vector2 summonPosition)
    {
        int index = Random.Range(0, _AdditionalSummonTarget.Length);

        Summon(_AdditionalSummonTarget[index], summonPosition, _BelongRoom);
    }

    private void Summon(GameObject summonTarget, Vector2 summonPosition, Room room)
    {
        summonPosition += _SummonOffset;

        var summoned = Instantiate(summonTarget, room.transform, false);
            summoned.transform.localPosition = summonPosition;

        if (summoned.TryGetComponent(out IObject _object))
        {
            room.AddIObject(_object);
        }
    }
}
