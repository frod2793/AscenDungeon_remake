using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemInfo", menuName = "ScriptableObject/ItemInfo")]
public class ItemInfo : ScriptableObject
{
    public readonly Color LegendaryColor = new Color(1, 0, 0.401f);
    public readonly Color EpicColor = new Color(0.497f, 0.2f, 0.65f);
    public readonly Color RareColor = new Color(0.18f, 0.84f, 0.22f);
    public readonly Color CommonColor = new Color(0.437f, 0.452f, 0.4f);

    public readonly string LegendaryText = "전설 아이템";
    public readonly string EpicText = "서사 아이템";
    public readonly string RareText = "희귀 아이템";
    public readonly string CommonText = "일반 아이템";

    [Header("Item Section")]
    [SerializeField] private string _ItemName;
    [SerializeField] private string _ItemRating;
    [SerializeField] private string _ATK;
    [SerializeField] private string _DPS;
    [SerializeField] private Color _TextColor;
    [SerializeField] private Sprite _ItemSprite;
    [SerializeField] private Keyword[] _UsageKeywords;

    [Header("Weapon Section")]
    [TextArea(2, 4)]
    [SerializeField] private string _WeaponText;
    [SerializeField] private float  _WeaponTextSize = 18f;
    [Space(), TextArea(1, 1)]
    [SerializeField] private string _WeaponAblity;

    [Header("Accessory Section")]
    [TextArea(2, 4)]
    [SerializeField] private string _AccessoryText;
    [SerializeField] private float  _AccessoryTextSize = 18f;
    [Space(), TextArea(1, 1)]
    [SerializeField] private string _AccessoryAblity;

    #region Access Variable
    public Sprite ItemSprite
    { get => _ItemSprite; }
    public string ItemName 
    { get => _ItemName; }
    public string ItemRatingText 
    { get => _ItemRating; }
    public string ATK
    { get => _ATK; }
    public string DPS
    { get => _DPS; }
    public Color TextColor
    { get => _TextColor; }
    public Keyword[] UsageKeywords
    { get => _UsageKeywords; }

    public string WeaponText
    { get => _WeaponText; }
    public string WeaponAblity
    { get => _WeaponAblity; }

    public string AccessoryText
    { get => _AccessoryText; }
    public string AccessoryAblity
    { get => _AccessoryAblity; }

    public float WeaponTextSize => _WeaponTextSize;
    public float AccessoryTextSize => _AccessoryTextSize;
    #endregion

    public void SetData(string nameText, ItemRating rating, float atk)
    {
        _ItemName = nameText;

        _ATK = atk.ToString("f1");

        switch (rating)
        {
            case ItemRating.Common:
                _ItemRating = CommonText;
                _TextColor = CommonColor;
                break;
            case ItemRating.Rare:
                _ItemRating = RareText;
                _TextColor = RareColor;
                break;

            case ItemRating.Epic:
                _ItemRating = EpicText;
                _TextColor = EpicColor;
                break;

            case ItemRating.Legendary:
                _ItemRating = LegendaryText;
                _TextColor = LegendaryColor;
                break;
        }
    }
}
