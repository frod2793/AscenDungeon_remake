using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogIndex
{
    public int Index;
    public int GroupCode;

    [Space(2.5f)]
    public string Name;
    public Texture Sprite;
    
    [Space(2.5f)]
    [TextArea(2, 4)]
    public string Text;
}

[CreateAssetMenu(fileName = "DialogData_", menuName = "ScriptableObject/DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField] private string _FileName;

    public List<DialogIndex> Data;
}
