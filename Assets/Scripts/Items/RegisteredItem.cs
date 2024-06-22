using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RegisterItemSet
{
    public ItemID ID;
    public Item Instance;
}
[CreateAssetMenu(fileName = "RegisteredItem", menuName = "ScriptableObject/RegisteredItem")]
public class RegisteredItem : ScriptableObject
{
    [SerializeField] private List<RegisterItemSet> _RegisterItems;

    public int Count()
    {
        return _RegisterItems.Count;
    }
    public List<ItemID> GetAllID()
    {
        List<ItemID> list = new List<ItemID>();
        for (int i = 0; i < _RegisterItems.Count; ++i)
        {
            list.Add(_RegisterItems[i].ID);
        }
        return list;
    }
    public Item GetItemInstance(ItemID id)
    {
        foreach (var item in _RegisterItems)
        {
            if (item.ID == id) return item.Instance;
        }
        return null;
    }
}