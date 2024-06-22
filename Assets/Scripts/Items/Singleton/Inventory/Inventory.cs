using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    public const int AccessorySlotCount = 3;
    public const int ContainerSlotCount = 6;

    #region Item Function Event

    public event Action<UnitizedPosV, Direction> MoveUpDownEvent;

    public event ProjHit ProjectionHitEvent;
    public delegate void ProjHit(GameObject victim, float damage);

    public event Action<Direction> DashBeginEvent;
    public event Action<Direction> DashEndEvent;

    public event Action PlayerEnterFloorEvent;

    #region COMMENT
    /// <summary>
    /// parameter[1] : attacker gameobject
    /// <br></br>
    /// parameter[2] : attack target interface 'ICombatable'
    /// </summary>
    #endregion
    public event Action<GameObject, ICombatable> AttackEvent;

    #region COMMENT
    /// <summary>
    /// parameter[1] : damage
    /// <br></br>
    /// parameter[2] : attacker gameobject
    /// <br></br>
    /// parameter[3] : target gameobject
    /// </summary>
    #endregion
    public event action BeDamagedAction;
    public delegate void action(ref float damage, GameObject attacker, GameObject victim);

    public event Action ChargeBeginAction;
    public event Action<float> ChargeEndAction;

    #endregion

    public event Action<Item> WeaponChangeEvent
    {
        add    => mWeaponSlot.ItemChangeEvent += value;
        remove => mWeaponSlot.ItemChangeEvent -= value;
    }
    public event Action<Item> WeaponEquipEvent
    {
        add    => mWeaponSlot.ItemEquipEvent += value;
        remove => mWeaponSlot.ItemEquipEvent -= value;
    }
    public Item GetWeaponItem
    {
        get => mWeaponSlot.ContainItem;
    }

                     public  GameObject  InventoryWindow => _InventoryWindow;
    [SerializeField] private GameObject _InventoryWindow;

    [SerializeField] private ItemSlot   mWeaponSlot;
    [SerializeField] private ItemSlot[] mAccessorySlot;
    [SerializeField] private ItemSlot[] mContainer;

    private Player mPlayer;

    private void Awake()
    {
        if (mPlayer == null) 
        {
            var @object = 
                GameObject.FindGameObjectWithTag("Player");

            Debug.Assert(@object.TryGetComponent(out mPlayer));
        }

        mWeaponSlot.Init(SlotType.Weapon);

        for (int i = 0; i < ContainerSlotCount; ++i)
        {   
            if (i < AccessorySlotCount)
            {
                var instance = ItemStateSaver.Instance.LoadSlotItem(SlotType.Accessory, i);
                if (instance != null) {
                    instance = ItemLibrary.Instance.GetItemObject(instance.ID);
                }
                mAccessorySlot[i].Init(SlotType.Accessory);
                mAccessorySlot[i].SetItem(instance);
            }
            {
                var instance = ItemStateSaver.Instance.LoadSlotItem(SlotType.Container, i);
                if (instance != null) {
                    instance = ItemLibrary.Instance.GetItemObject(instance.ID);
                }
                mContainer[i].Init(SlotType.Container);
                mContainer[i].SetItem(instance);
            }
        }
    }
    public bool IsEquipWeapon()
    {
        return mWeaponSlot.ContainItem != null;
    }
    public void SetWeaponSlot(Item item)
    {
        mWeaponSlot.SetItem(item);
    }
    public void AddItem(Item item)
    {
        mContainer.Where(o => o.ContainItem == null).First()?.SetItem(item);
    }
    public void Clear()
    {
        ItemLibrary.Instance.ItemBoxReset();

        mWeaponSlot.SetItem(null);

        mAccessorySlot.ToList().ForEach(o => o.SetItem(null));
            mContainer.ToList().ForEach(o => o.SetItem(null));
    }
    // ========== ItemEvent Method ========== //
    public void OnDamaged(ref float damage, GameObject attacker, GameObject victim)
    {
        BeDamagedAction?.Invoke(ref damage, attacker, victim);
    }
    public void BeginOfCharge()
    {
        ChargeBeginAction?.Invoke();
    }
    public void EndOfCharge(float power)
    {
        ChargeEndAction?.Invoke(power);
    }
    public void PlayerBeginDash(Direction direction)
    {
        DashBeginEvent?.Invoke(direction);
    }
    public void PlayerEndDash(Direction direction)
    {
        DashEndEvent?.Invoke(direction);
    }
    public void PlayerEnterFloor()
    {
        PlayerEnterFloorEvent?.Invoke();
    }
    public void PlayerMoveUpDownBegin(UnitizedPosV room, Direction direction)
    {
        MoveUpDownEvent?.Invoke(room, direction);
    }
    public void ProjectionHit(GameObject victim, float damage)
    {
        ProjectionHitEvent?.Invoke(victim, damage);
    }
    public void AttackAction(GameObject attacker, ICombatable targetCombat)
    {
        mWeaponSlot.ContainItem?.AttackAction(attacker, targetCombat);
    }
    public void AttackCancel()
    {
        mWeaponSlot.ContainItem?.AttackCancel();
    }

    public void OnAttackEvent(GameObject attacker, ICombatable targetCombat)
    {
        AttackEvent?.Invoke(attacker, targetCombat);
    }
    // ========== ItemEvent Method ========== //
}