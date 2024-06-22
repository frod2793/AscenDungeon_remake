using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageClearTutorial : TutorialBase
{
    [SerializeField] private ItemBox _ItemBox;
    [SerializeField] private ItemBoxSprite _ItemBoxSprite;
    [SerializeField] private SubscribableButton _ItemBoxInteraction;
    [SerializeField] private ItemID _ContainItem;

    public override void StartTutorial()
    {
        base.StartTutorial();

        var item = ItemLibrary.Instance.GetItemObject(_ContainItem);
        
        _ItemBox.Init(item, _ItemBoxSprite, _ItemBoxInteraction);
        _ItemBox.gameObject.SetActive(true);

        Inventory.Instance.EnterItemSlotEvent += EnterItemSlotEvent;
    }

    public override void EndTutorial()
    {
        base.EndTutorial();
        StageClearNotice.Instance.Show();
        Inventory.Instance.EnterItemSlotEvent -= EnterItemSlotEvent;
    }

    private void EnterItemSlotEvent(Item item, SlotType slotType)
    {
        if (item.ID == _ContainItem && slotType == SlotType.Container)
        {
            EndTutorial();
        }
    }
}
