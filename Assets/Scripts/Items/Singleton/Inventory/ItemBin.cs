using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBin : MonoBehaviour
{
    [SerializeField] private SubscribableButton _BinButton;

    private void Awake()
    {
        _BinButton.ButtonAction += BinButton_ButtonAction;
    }
    private void BinButton_ButtonAction(ButtonState btnState)
    {
        switch (btnState)
        {
            case ButtonState.Down:
                {
                    if (Finger.Instance.CarryItem == null)
                        return;

                    SystemMessage.Instance.ShowCheckMessage("아이템 버??림???", result => {
                        if (result)
                        {
                            Finger.Instance.CarryItem = null;
                        }
                    });
                }
                break;
        }
    }
}
