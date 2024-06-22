using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPopup : Singleton<ItemInfoPopup>
{
    [SerializeField] private ItemKeywordContainer _KeywordContainer;
    [Space()]
    [SerializeField] private Image _ItemImage;
    [SerializeField] private GameObject    _PopupObject;
    [SerializeField] private RectTransform _RectTransform;

    [Header("ItemData Section")]
    [SerializeField] private TMPro.TextMeshProUGUI _NameText;
    [SerializeField] private TMPro.TextMeshProUGUI _RatingText;
    [SerializeField] private TMPro.TextMeshProUGUI _ATKText;
    [SerializeField] private TMPro.TextMeshProUGUI _DPSText;

    [Header("WeaponData Section")]
    [SerializeField] private TMPro.TextMeshProUGUI _WeaponInfoText;
    [SerializeField] private TMPro.TextMeshProUGUI _WeaponAblityText;

    [Header("AccessoryData Section")]
    [SerializeField] private TMPro.TextMeshProUGUI _AccessoryInfoText;
    [SerializeField] private TMPro.TextMeshProUGUI _AccessoryAblityText;

    private ItemInfo _ItemInfo;
    private Coroutine _CloseInputCheck;

    public void SetPopup(ItemInfo info)
    {
        _ItemInfo = info;

        _ItemImage.sprite = info.ItemSprite;

        _NameText.text = info.ItemName;
        _NameText.color = info.TextColor;
        _RatingText.text = info.ItemRatingText;
        _RatingText.color = info.TextColor;

        _ATKText.text = $"ATK : {info.ATK}";
        _DPSText.text = $"DPS : {info.DPS}";

        _WeaponInfoText.text = info.WeaponText;
        _WeaponInfoText.fontSize = info.WeaponTextSize;
        _WeaponAblityText.text = info.WeaponAblity;

        _AccessoryInfoText.text = info.AccessoryText;
        _AccessoryInfoText.fontSize = info.AccessoryTextSize;
        _AccessoryAblityText.text = info.AccessoryAblity;
    }

    public void ShowPopup(UnitizedPos pivot, Vector2 position)
    {
        UnitizedPosH keywordPosition = UnitizedPosH.LEFT;

        switch (pivot)
        {
            case UnitizedPos.TOP_LEFT:
                keywordPosition = UnitizedPosH.LEFT;
                _RectTransform.pivot = new Vector2(0.0f, 1.0f);
                break;
            case UnitizedPos.TOP:
                keywordPosition = UnitizedPosH.MID;
                _RectTransform.pivot = new Vector2(0.5f, 1.0f);
                break;
            case UnitizedPos.TOP_RIGHT:
                keywordPosition = UnitizedPosH.RIGHT;
                _RectTransform.pivot = new Vector2(1.0f, 1.0f);
                break;
            case UnitizedPos.MID_LEFT:
                keywordPosition = UnitizedPosH.LEFT;
                _RectTransform.pivot = new Vector2(0.0f, 0.5f);
                break;
            case UnitizedPos.MID:
                keywordPosition = UnitizedPosH.MID;
                _RectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            case UnitizedPos.MID_RIGHT:
                keywordPosition = UnitizedPosH.RIGHT;
                _RectTransform.pivot = new Vector2(1.0f, 0.5f);
                break;
            case UnitizedPos.BOT_LEFT:
                keywordPosition = UnitizedPosH.LEFT;
                _RectTransform.pivot = new Vector2(0.0f, 0.0f);
                break;
            case UnitizedPos.BOT:
                keywordPosition = UnitizedPosH.MID;
                _RectTransform.pivot = new Vector2(0.5f, 0.0f);
                break;
            case UnitizedPos.BOT_RIGHT:
                keywordPosition = UnitizedPosH.RIGHT;
                _RectTransform.pivot = new Vector2(1.0f, 0.0f);
                break;
            default:
                break;
        }
        _PopupObject.SetActive(true);
        _KeywordContainer.ShowKeyword(keywordPosition, _ItemInfo.UsageKeywords);

        _RectTransform.position = position;

        _CloseInputCheck.StartRoutine(CloseInputCheck());
    }

    public void ClosePopup()
    {
        _PopupObject.SetActive(false);
        _KeywordContainer.gameObject.SetActive(false);
    }

    private void Start()
    {
        _CloseInputCheck = new Coroutine(this);
    }

    private IEnumerator CloseInputCheck()
    {
        do
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ClosePopup();
                    }
                    break;
                case RuntimePlatform.Android:
                    if (Input.touchCount > 0)
                    {
                        if (Input.GetTouch(0).phase == TouchPhase.Began)
                        {

                            ClosePopup();
                        }
                    }
                    break;
            }
            yield return null;

        } while (_PopupObject.activeSelf);

        _CloseInputCheck.Finish();
    }
}
