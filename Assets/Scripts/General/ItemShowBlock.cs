using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShowBlock : MonoBehaviour
{
    [SerializeField] private ItemStore Store;
    [SerializeField] private ItemInfoShower ItemShower;

    [SerializeField] private Button _Button;

    [Header("TextSection")]
    [SerializeField] private TMPro.TextMeshProUGUI ItemNameText;
    [SerializeField] private TMPro.TextMeshProUGUI ItemCostText;

    [Header("ImageSection")]
    [SerializeField] private Image ItemImage;
    [SerializeField] private Image ContainerImage;

    public int ItemCost
    {
        get;
        private set;
    }
    public Item ContainItem
    {
        get;
        private set;
    }

    public void Selected()
    {
        Store.BlockSelect(this);
    }
    public void Disable()
    {
        _Button.enabled = false;

        ItemNameText.color = new Color(0.35f, 0.35f, 0.35f);
        ItemCostText.color = new Color(0.25f, 0.25f, 0.25f);

        ItemImage.color      = new Color(0.5f, 0.5f, 0.5f);
        ContainerImage.color = new Color(0.5f, 0.5f, 0.5f);
    }
    public void SetBlock(Item blockItem)
    {
        ContainItem = blockItem;
        ItemCost = blockItem.Cost;

        ItemNameText.text = blockItem.NameKR;
        ItemCostText.text = ItemCost.ToString();

        ItemImage.sprite = blockItem.Sprite;
        ItemShower.OnPopupEvent = () =>
        {
            ItemInfoPopup.Instance.SetPopup(blockItem.GetItemInfo);
        };
    }
}
