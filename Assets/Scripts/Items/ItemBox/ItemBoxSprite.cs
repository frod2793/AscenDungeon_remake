using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemBoxFunc", menuName = "ScriptableObject/ItemBoxFunc")]
public class ItemBoxSprite : ScriptableObject
{
    [SerializeField] private Sprite _ClosedSprite;
    [SerializeField] private Sprite   _OpenSprite;

    public Sprite ClosedSprite 
    { get => _ClosedSprite; }
    public Sprite   OpenSprite 
    { get => _OpenSprite; }
}
