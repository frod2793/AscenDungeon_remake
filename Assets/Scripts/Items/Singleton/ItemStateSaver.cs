using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemStateSaver : Singleton<ItemStateSaver>
{
    [SerializeField] private RegisteredItem RegisteredItem;

    private List<Item> _UnlockedItemList = null;
    private List<Item>   _LockedItemList = null;

    private ItemID[] _AccessoryIDArray;
    private ItemID[] _ContainerIDArray;

    private ItemID _WeaponItemID;

    private bool _IsAlreadyInit = false;

    private void Awake()
    {
        Init();
    }
        private void Init()
        {
            if (!_IsAlreadyInit)
            {
                _IsAlreadyInit = true;
                ItemListInit();
                ItemSlotArrayInit();

                // [개선]: Inspector 설정에 의존하지 않고 Resources 폴더에서 자동으로 로드 시도
                if (RegisteredItem == null)
                {
                    RegisteredItem = Resources.Load<RegisteredItem>("RegisteredItem");
                    
                    if (RegisteredItem != null)
                    {
                        Debug.Log("[ItemStateSaver] RegisteredItem를 Resources에서 로드했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning("[ItemStateSaver] Resources에서 RegisteredItem를 찾을 수 없습니다. 씬 검색을 시도합니다.");
                        var found = FindObjectOfType<RegisteredItem>();
                        if (found != null)
                        {
                            RegisteredItem = found;
                            Debug.Log("[ItemStateSaver] RegisteredItem를 씬에서 찾아 연결했습니다.");
                        }
                    }
                }

                // [안전 장치]: RegisteredItem이 최종적으로 null인 경우에도 시스템이 정지하지 않도록 처리
                if (RegisteredItem != null)
                {
                    List<int> list = new List<int>();
                    var allIDs = RegisteredItem.GetAllID();
                    if (allIDs != null)
                    {
                        for (int i = 0; i < allIDs.Count; i++)
                        {
                            list.Add((int)allIDs[i]);
                        }
                    }
                    SetUnlockedItem(list);
                }
                else
                {
                    Debug.LogError("[ItemStateSaver] RegisteredItem 로드 실패! 아이템 목록이 비어있는 상태로 초기화됩니다.");
                    // 빈 리스트로 초기화하여 최소한의 NullReferenceException 방지
                    SetUnlockedItem(new List<int>());
                }
            // ====== ====== Test ====== ====== //
            // SetUnlockedItem(new List<int>());

            // ====== ====== 기본아이템 지급 ====== ====== //
            EquipWeaponItem();

            SceneManager.sceneUnloaded += o => 
            {
                EquipWeaponItem();
            };
            // ====== ====== 기본아이템 지급 ====== ====== // 

            if (FindObjectsOfType(typeof(ItemStateSaver)).Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    public void SetUnlockedItem(List<int> idList)
    {
        if (RegisteredItem == null)
        {
            Debug.LogError("[ItemStateSaver] SetUnlockedItem failed: RegisteredItem is null!");
            return;
        }

        ItemListInit();

        _UnlockedItemList.Clear();
          _LockedItemList.Clear();

        int registerCount = RegisteredItem.Count();
        for (int i = 1; i < registerCount + 1; i++)
        {
            Item instance = RegisteredItem.GetItemInstance((ItemID)i);

            if (IsDefaultUnlockItemCheck(i) || (idList != null && idList.Contains(i)))
            {
                _UnlockedItemList.Add(instance);
            }
            else
            {
                _LockedItemList.Add(instance);
            }
        }
    }
    public void SetInventoryItem(List<int> idList)
    {
        Debug.Log(idList.Count());

        _WeaponItemID = (ItemID)idList[0];
        idList.RemoveAt(0);

        int container = Inventory.ContainerSlotCount;
        int accessory = Inventory.AccessorySlotCount;

        int invokeCount = container + accessory;

        for (int i = 0; i < invokeCount; i++)
        {
            if (i < accessory)
            {
                _AccessoryIDArray[i] = (ItemID)idList[i];
            }
            if (i < container)
            {
                _ContainerIDArray[i] = (ItemID)idList[i + accessory];
            }
        }
    }


    public List<Item> GetUnlockedItem()
    {
        Init();
        return new List<Item>(_UnlockedItemList);
    }
    public List<Item> GetLockedItem()
    {
        Init();
        return new List<Item>(_LockedItemList);
    }

    public List<int> GetInventoryItem()
    {
        List<int> list = new List<int>();
        list.Add((int)_WeaponItemID);

        int invokeCount = 
            Inventory.AccessorySlotCount + 
            Inventory.ContainerSlotCount;

        for (int i = 0; i < invokeCount; i++)
        {
            if (i < Inventory.AccessorySlotCount)
            {
                list.Add((int)_AccessoryIDArray[i]);
            }
            else
            {
                list.Add((int)_ContainerIDArray[i]);
            }
        }
        return list;
    }
    public void ItemUnlock(params ItemID[] ids)
    {
        ItemListInit();

        for (int i = 0; i < ids.Length; i++)
        {
            for (int j = 0; j < _LockedItemList.Count; j++)
            {
                if (_LockedItemList[j].ID == ids[i])
                {
                    _UnlockedItemList.Add(_LockedItemList[j]);
                  
                    _LockedItemList.RemoveAt(j);
                }
            }
        }
        SoundManager.Instance.PlaySound(SoundName.UnlockItem);
    }

    public void SaveSlotItem(SlotType slotType, Item item, int index)
    {
        ItemSlotArrayInit();

        switch (slotType)
        {
            case SlotType.Weapon:
                {
                    if (item == null)
                    {
                        _WeaponItemID = ItemID.None;
                    }
                    else 
                        _WeaponItemID = item.ID;
                }
                break;

            case SlotType.Container:
                {
                    if (item == null)
                    {
                        _ContainerIDArray[index] = ItemID.None;
                    }
                    else
                        _ContainerIDArray[index] = item.ID;
                }
                break;

            case SlotType.Accessory:
                {
                    if (item == null)
                    {
                        _AccessoryIDArray[index] = ItemID.None;
                    }
                    else
                        _AccessoryIDArray[index] = item.ID;
                }
                break;
        }
    }
    public Item LoadSlotItem(SlotType slotType, int index)
    {
        ItemSlotArrayInit();
        ItemID loadID = ItemID.None;

        switch (slotType)
        {
            case SlotType.Weapon:
                loadID = _WeaponItemID;
                break;

            case SlotType.Container:
                loadID = _ContainerIDArray[index];
                break;

            case SlotType.Accessory:
                loadID = _AccessoryIDArray[index];
                break;
        }
        return RegisteredItem.GetItemInstance(loadID);
    }
    public void EquipWeaponItem()
    {
        if (_WeaponItemID == ItemID.None)
        {
            for (int i = 0; i < Inventory.ContainerSlotCount; ++i)
            {
                if (_ContainerIDArray[i] != ItemID.None)
                {
                    _WeaponItemID = _ContainerIDArray[i];
                    _ContainerIDArray[i] = ItemID.None;
                    break;
                }
            }
            if (_WeaponItemID == ItemID.None)
            {
                for (int i = 0; i < Inventory.AccessorySlotCount; ++i)
                {
                    if (_AccessoryIDArray[i] != ItemID.None)
                    {
                        _WeaponItemID = _AccessoryIDArray[i];
                        _AccessoryIDArray[i] = ItemID.None;
                        break;
                    }
                }
                if (_WeaponItemID == ItemID.None)
                {
                    _WeaponItemID = ItemID.LongSword;
                }
            }
        }
    }

    private void ItemSlotArrayInit()
    {
        _ContainerIDArray = _ContainerIDArray ?? new ItemID[Inventory.ContainerSlotCount];
        _AccessoryIDArray = _AccessoryIDArray ?? new ItemID[Inventory.AccessorySlotCount];
    }
    private void ItemListInit()
    {
        _UnlockedItemList = _UnlockedItemList ?? new List<Item>();
          _LockedItemList =   _LockedItemList ?? new List<Item>();
    }
    private bool IsDefaultUnlockItemCheck(int id)
    {
        return id == (int)ItemID.LongSword
            || id == (int)ItemID.OrdinaryBow
            || id == (int)ItemID.Shuriken
            || id == (int)ItemID.MysteriousMace
            || id == (int)ItemID.IronShield;
    }
}
