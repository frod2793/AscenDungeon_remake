using Newtonsoft.Json;
using System.Data;
using System.IO;
using UnityEngine;

public static class DataUtil
{
    public static DataTable GetDataTable(string fileName, string tableName)
    {
        var obj = Resources.Load(fileName);

        string value = (obj as TextAsset).ToString();

        DataTable data = JsonConvert.DeserializeObject<DataTable>(value);
        data.TableName = tableName;

        return data;
    }
    public static DataTable GetDataTable(FileInfo info)
    {
        string fileName = Path.GetFileNameWithoutExtension(info.Name);

        string path  = $@"JsonData/{fileName}";
        string value = string.Empty;
        try
        {
            value = (Resources.Load(path) as TextAsset).ToString();
        }
        catch
        {
            Debug.LogError($"Can not load dataTable : {info.Name}");
        }
        DataTable data = JsonConvert.DeserializeObject<DataTable>(value);
        data.TableName = fileName;

        return data;
    }

    public static string GetDataValue(string tableName, string primary, string value, string column)
    {
        DataRow[] rows = DataManager.Instance.DataBase.Tables[tableName].Select($@"{primary} = '{value}'");

        return rows[0][column].ToString();
    }
    public static void SetObjectFile<T>(string key, T data)
    {
        string value = JsonConvert.SerializeObject(data);

        File.WriteAllText($@"{Application.dataPath}/Resources/JsonData/{key}.json", value);
    }
}
