using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemID
{
    None, GreatSword, FrozenShose, IronShield, 
    MysteriousMace, OrdinaryBow, Shuriken, LongSword, ThronArmor,
    DangerousBottle, RuneFragment, FightersGlove
}
public abstract class Item : MonoBehaviour
{
    public System.Action AttackOverAction;

    [SerializeField] private ItemInfo _ItemInfoTable;
    [SerializeField] protected ItemStatTable StatTable;

    public ItemInfo GetItemInfo => _ItemInfoTable;

    public string NameKR
    {
        get => StatTable.NameKR;
    }
    public int Cost
    {
        get => StatTable.Cost;
    }

    public    Sprite  Sprite
    {
        get
        {
            if (mSprite == null)
            {
                if (TryGetComponent(out SpriteRenderer renderer))
                {
                    mSprite = renderer.sprite;
                }
            }
            return mSprite;
        }
    }
    protected Sprite mSprite;

    // *********************************** //
    public virtual bool IsNeedAttackBtn
    { get => true; }

    public ItemRating Rating
    {
        get => StatTable.Rating;
    }
    public ItemID ID
    {
        get => StatTable.ID;
    }
    public float WeaponRange
    { 
        get => StatTable[ItemStat.Range]; 
    }
    public float Begin_AttackDelay
    {
        get => StatTable[ItemStat.Begin_AttackDelay];
    }
    public float After_AttackDelay
    {
        get => StatTable[ItemStat.After_AttackDelay];
    }

    // *********************************** //

    public abstract void  OnEquipThis(SlotType onSlot);
    public abstract void OffEquipThis(SlotType offSlot);

    public abstract void AttackCancel();

    public virtual void AttackAction(GameObject attacker, ICombatable combatable)
    { }

    // 애니메이션 이벤트 함수
    protected virtual void CameraShake()
    {
        MainCamera.Instance.Shake();
    }
    protected virtual void AttackAnimationPlayOver()
    {
        AttackOverAction?.Invoke();
    }
}
