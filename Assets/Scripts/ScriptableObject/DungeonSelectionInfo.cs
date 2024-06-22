using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SelectionInfo", menuName = "ScriptableObject/Dungeon/SelectionInfo")]
public class DungeonSelectionInfo : ScriptableObject
{
    [SerializeField] private string _AttachedSceneName;
    public string AttachedSceneName
    { get => _AttachedSceneName; }

    [Header("DungeonLocked Section")]
    [TextArea(1, 1)] 
    [SerializeField] private string _LockedTitle;
    [TextArea(3, 4)] 
    [SerializeField] private string _LockedComment;
    [SerializeField] private Sprite _LockedSprite;
    public string LockedTitle
    { get => _LockedTitle; }
    public string LockedComment
    { get => _LockedComment; }
    public Sprite LockedSprite 
    { get => _LockedSprite; }


    [Header("DungeonUnLocked Section")]
    [TextArea(1, 1)]
    [SerializeField] private string _UnLockedTitle;
    [TextArea(3, 4)]
    [SerializeField] private string _UnLockedComment;
    [SerializeField] private Sprite _UnLockedSprite;
    public string UnLockedTitle
    { get => _UnLockedTitle; }
    public string UnLockedComment
    { get => _UnLockedComment; }
    public Sprite UnLockedSprite
    { get => _UnLockedSprite; }
}
