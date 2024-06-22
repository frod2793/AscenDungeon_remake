using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogData))]
public class DialogDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Space(5f);
        string fileName = serializedObject.FindProperty("_FileName").stringValue;
        serializedObject.FindProperty("_FileName").stringValue = GUILayout.TextField(fileName);

        if (GUILayout.Button("Get Data", GUILayout.Height(25f)))
        {
            DialogData dialog = target as DialogData;
            dialog.Data.Clear();

            for (int i = 0; ; ++i)
            {
                try
                {
                    GetData(fileName, i, "Index");
                }
                catch
                {
                    break;
                }
                var indexData = new DialogIndex()
                {
                    Index = int.Parse(GetData(fileName, i, "Index")),
                    GroupCode = int.Parse(GetData(fileName, i, "Group")),
                    Sprite = (Texture)Resources.Load(string.Concat("ImageData/", GetData(fileName, i, "ImageName"))),
                    Name = GetData(fileName, i, "Name"),
                    Text = GetData(fileName, i, "Text")
                };
                dialog.Data.Add(indexData);
            }
            EditorUtility.SetDirty(target);
        }
        DrawProperty("Data");
        serializedObject.ApplyModifiedProperties();
    }

    private string GetData(string tableName, int index, string column)
    {
        return DataUtil.GetDataValue(tableName, "ID", index.ToString(), column);
    }

    private void DrawProperty(string name)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty(name), true);
    }
}
