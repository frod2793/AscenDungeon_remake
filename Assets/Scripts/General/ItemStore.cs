using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemStore : MonoBehaviour
{
    [SerializeField] private RegisteredItem RegisteredItem;
    [SerializeField] private ItemShowBlock[] ItemBlocks;

    private bool _IsAlreadyInit = false;
    private ItemShowBlock _SelectedBlock;
    private IPurchasable  _SelectedPurchaser;

    private void Start()
    {
        if (!_IsAlreadyInit)
        {
            var list = ItemStateSaver.Instance.GetUnlockedItem();

            for (int i = 0; i < 3; i++)
            {
                int index = Random.Range(0, list.Count);

                ItemBlocks[i].SetBlock(list[index]);
                                       list.RemoveAt(index);
            }
            _IsAlreadyInit = true;
        }
    }
    public void BlockSelect(ItemShowBlock block)
    {
        _SelectedBlock = block;
        _SelectedPurchaser = null;
    }
    public void PurchaserSelect(IPurchasable purchaser)
    {
        _SelectedBlock = null;
        _SelectedPurchaser = purchaser;
    }
    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            _SelectedBlock = null;
            _SelectedPurchaser = null;
        }
    }
    public void Purchace()
    {
        if (_SelectedBlock != null)
        {
            if (MoneyManager.Instance.SubtractMoney(_SelectedBlock.ItemCost))
            {
                SoundManager.Instance.PlaySound(SoundName.BuyItem);

                var item = Instantiate(_SelectedBlock.ContainItem);
                    item.transform.position = new Vector2(-10f, 0f);

                Inventory.Instance.AddItem(item);
                item.transform.parent = ItemStateSaver.Instance.transform;

                _SelectedBlock.Disable();
                _SelectedBlock = null;
            }
            else
            {
                SystemMessage.Instance.ShowMessage("보유한 골드가 부족합니다!");
            }
        }
        else if (_SelectedPurchaser != null)
        {
            _SelectedPurchaser.TryPurchase();
        }
        else
        {
            SystemMessage.Instance.ShowMessage("먼저 구매할 항목을\n선택해야 합니다!");
        }
    }
}
