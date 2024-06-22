using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeddlerNPC : MonoBehaviour
{
    [SerializeField] private AttackButtonHider _Hider;

    [Header("Item Section")]
    [Range(1f, 2f)]
    [SerializeField] private float ItemCostScalling;
    [SerializeField] private SpriteRenderer ShowItemRenderer;
    [SerializeField] private Animator ShowItemAnimator;
    [SerializeField] private GameObject InteractionUI;

    [Header("UI Section")]
    [SerializeField] private GraphicRaycaster CanvasRaycaster;
    [SerializeField] private TMPro.TextMeshProUGUI ItemCostText;
    [SerializeField] private TMPro.TextMeshProUGUI ItemNameText;
    [SerializeField] private TMPro.TextMeshProUGUI  MessageText;

    private bool _IsBoughtItem;

    private int _DropItemControlKey;
    private int _ItemCost;
    private Item _ContainItem;

    private void OnEnable()
    {
        _IsBoughtItem = false;
        var list = ItemStateSaver.Instance.GetUnlockedItem();

        _ContainItem = list[Random.Range(0, list.Count)];

        ShowItemRenderer.sprite = _ContainItem.Sprite;
        _ItemCost = (int)System.Math.Ceiling(_ContainItem.Cost * ItemCostScalling);

        ItemCostText.gameObject.SetActive(true);
        ItemNameText.gameObject.SetActive(true);

        ItemCostText.text = _ItemCost.ToString();
        ItemNameText.text = _ContainItem.NameKR;

        if (_DropItemControlKey == default)
        {
            _DropItemControlKey = ShowItemAnimator.GetParameter(0).nameHash;
        }
        MessageText.text = "구매확인";
        InteractionUI.SetActive(CanvasRaycaster.enabled = false);
    }
    private void OnDisable()
    {
        ShowItemAnimator.SetBool(_DropItemControlKey, false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CanvasRaycaster.enabled = true;
            InteractionUI.SetActive(true);

            // _Hider.HideOrShow();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CanvasRaycaster.enabled = false;
            InteractionUI.SetActive(false);

            // _Hider.HideOrShow();
        }
    }

    public void Purchace()
    {
        if (!_IsBoughtItem)
        {
            if (MoneyManager.Instance.SubtractMoney(_ItemCost))
            {
                var item = Instantiate(_ContainItem);
                item.transform.position = new Vector2(-10f, 0f);

                Inventory.Instance.AddItem(item);
                item.transform.parent = ItemStateSaver.Instance.transform;

                ShowItemAnimator.SetBool(_DropItemControlKey, true);

                ItemCostText.gameObject.SetActive(false);
                ItemNameText.gameObject.SetActive(false);

                Vector2 point = (Vector2)transform.position + new Vector2(0, -1.5f);
                EffectLibrary.Instance.UsingEffect(EffectKind.Coin, point);
                MessageText.text = "구매완료!";

                _IsBoughtItem = true;
            }
            else
            {
                SystemMessage.Instance.ShowMessage("보유한 골드가 부족합니다!");
            }
        }
        else
        {
            SystemMessage.Instance.ShowMessage("이미 구매한 물건입니다!");
        }
    }
}
