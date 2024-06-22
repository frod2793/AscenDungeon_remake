using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ItemBox))]
public class ReplaceItem : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Space(15f);

        if (GUILayout.Button("Replace Item", GUILayout.Height(25f)))
        {
            if (Application.isEditor)
            {
                var itemBox = target as ItemBox;

                itemBox.SendMessage("OnEnable");
            }
        }
    }
}
#endif