using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Keyword
{
    Strike, Charge, Entry
}
[Serializable]
public struct KeywordBlock
{
    public Keyword Keyword;
    public GameObject KeywordInfoUI;
}

[Serializable]
public struct GridLayoutSetting
{
    public TextAnchor ChildAlignment;
    public Vector2 Spacing;
}

public class ItemKeywordContainer : MonoBehaviour
{
    [SerializeField] private RectTransform   _RectTransform;
    [SerializeField] private GridLayoutGroup _GridLayoutGroup;
    [SerializeField] private KeywordBlock[]  _KeywordBlocks;

    [Header("GridLayoutGroup")]
    [SerializeField] private GridLayoutSetting _HorizontalSetting;
    [SerializeField] private GridLayoutSetting _VerticalSetting;

    [Header("RectTransform")]
    [SerializeField] private Rect _LeftRect;
    [SerializeField] private Rect _RightRect;
    [SerializeField] private Rect _DownRect;

    private Dictionary<Keyword, GameObject> _KeywordBlockCollection;
    private bool _IsAlreadyInit = false;

    private void OnEnable()
    {
        if (!_IsAlreadyInit)
        {
            _KeywordBlockCollection = new Dictionary<Keyword, GameObject>();

            for (int i = 0; i < _KeywordBlocks.Length; ++i)
            {
                _KeywordBlockCollection.Add
                    (_KeywordBlocks[i].Keyword, _KeywordBlocks[i].KeywordInfoUI);
            }
            _IsAlreadyInit = true;
        }
    }

    private void OnDisable()
    {
        foreach (var keywordBlock in _KeywordBlockCollection) {

            keywordBlock.Value.SetActive(false);
        }
    }

    public void ShowKeyword(UnitizedPosH position, params Keyword[] keywords)
    {
        gameObject.SetActive(true);

        Rect targetRect = default;
        GridLayoutSetting targetLayout = default;

        switch (position)
        {
            case UnitizedPosH.LEFT:
                {
                    targetRect   = _LeftRect;
                    targetLayout = _VerticalSetting;
                }
                break;
            case UnitizedPosH.MID:
                {
                    targetRect   = _DownRect;
                    targetLayout = _HorizontalSetting;
                }
                break;
            case UnitizedPosH.RIGHT:
                {
                    targetRect   = _RightRect;
                    targetLayout = _VerticalSetting;
                }
                break;
        }
        _RectTransform.sizeDelta = new Vector2(targetRect.width, targetRect.height);
        _RectTransform.localPosition = new Vector2(targetRect.x, targetRect.y);

        _GridLayoutGroup.spacing = targetLayout.Spacing;
        _GridLayoutGroup.childAlignment = targetLayout.ChildAlignment;
        
        for (int i = 0; i < keywords.Length; ++i)
        {
            _KeywordBlockCollection[keywords[i]].SetActive(true);
        }
    }
}
