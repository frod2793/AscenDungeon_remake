using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Newtonsoft.Json;
using Google.Apis.Sheets;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using System.Threading;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class DataManager : Singleton<DataManager>
{
    private string JsonDataPath;

    //========= Inspecter Vlew =========//
    [Space(3)][Header("QAuth Info")]

    [SerializeField][TextArea(1, 1)]
    private string ClientName;
    [SerializeField][TextArea(1, 1)]
    private string ClientID;
    [SerializeField][TextArea(1, 1)]
    private string ClientPW;

    [Space(3)][Header("Google Cloud Platform")]

    [SerializeField][TextArea(1, 1)]
    private string ProjectName;
    [SerializeField][TextArea(1, 1)]
    private string GoogleSheetTableKey;
    //========= Inspecter Vlew =========//

    public  DataSet  DataBase
    { 
        get
        {
            if (mDataBase == null) Init();

            return mDataBase;
        }
    }
    private DataSet mDataBase;

    private void Awake()
    {
        if (FindObjectsOfType(typeof(ItemStateSaver)).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    public void Init()
    {
        mDataBase = mDataBase ?? new DataSet("DataBase");

        JsonDataPath = $@"{Application.dataPath}/Resources/JsonData/";

        if (Application.isEditor && !Application.isPlaying)
        {
            MakeSheetDataSet(mDataBase);
        }
        else
        {
            LoadJsonData(mDataBase);
        }
    }

    private void LoadJsonData(DataSet dataSet)
    {
        DirectoryInfo info = new DirectoryInfo(JsonDataPath);

        foreach (FileInfo file in info.GetFiles())
        {
            if (file.Name.EndsWith(".meta")) continue;

            dataSet.Tables.Add(DataUtil.GetDataTable(file));
        }
    }

    private void MakeSheetDataSet(DataSet dataSet)
    {
        var pass = new ClientSecrets();

        pass.ClientId     = ClientID;
        pass.ClientSecret = ClientPW;

        var scopes = new string[] { SheetsService.Scope.SpreadsheetsReadonly };

        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(pass, scopes, ClientName, CancellationToken.None).Result;

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            ApplicationName = ProjectName, HttpClientInitializer = credential
        });

        var request = service.Spreadsheets.Get(GoogleSheetTableKey).Execute();

        foreach (Sheet sheet in request.Sheets)
        {
            dataSet.Tables.Add(SendRequest(service, sheet.Properties.Title));
        }
    }
    private DataTable SendRequest(SheetsService service, string sheetName)
    {
        DataTable result = null;
        try
        {
            // getting A1 ~ M
            var request = service.Spreadsheets.Values.Get(GoogleSheetTableKey, sheetName + "!A1:M");

            var    jsonObject = request.Execute().Values;
            string jsonString = ParseSheetData(jsonObject);

            result = JsonConvert.DeserializeObject<DataTable>(jsonString);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);

            string path = $@"JsonData/{0}{sheetName}";
            result = DataUtil.GetDataTable(path, sheetName);

            Debug.LogError($"시트를 불러올 수 없기 때문에, 로컬 {sheetName}의JsonData를 불러왔습니다.");
        }
        result.TableName = sheetName;

        SaveDataToFile(result);

        return result;
    }

    private void SaveDataToFile(DataTable table)
    {
        string path = JsonDataPath + $"{table.TableName}.json";

        FileInfo info = new FileInfo(path);

        if (info.Exists)
        {
            DataTable checkTable = DataUtil.GetDataTable(info);

            if (IsInBinary(table, checkTable))
            {
                return;
            }
        }
        DataUtil.SetObjectFile(table.TableName, table);
    }

    private bool IsInBinary<T>(T targetTable, T compareTable)
    {
        MemoryStream stream1 = new MemoryStream();
        MemoryStream stream2 = new MemoryStream();

        BinaryFormatter formatter1 = new BinaryFormatter();
        BinaryFormatter formatter2 = new BinaryFormatter();

        formatter1.Serialize(stream1,  targetTable);
        formatter2.Serialize(stream2, compareTable);

        byte[]  targetByte = stream1.ToArray();
        byte[] compareByte = stream2.ToArray();

        if (targetByte.Length != compareByte.Length) 
            return false;

        for (int i = 0; i < targetByte.Length; i++)
        {
            if (targetByte[i] != compareByte[i])
                return false;
        }
        return true;
    }

    private string ParseSheetData(IList<IList<object>> values)
    {
        StringBuilder jsonBuilder = new StringBuilder();

        IList<object> columns = values[0];

        jsonBuilder.Append("[");
        for (int row = 1; row < values.Count; ++row)
        {
            var data = values[row];

            jsonBuilder.Append("{");

            for (int col = 0; col < data.Count; ++col)
            {
                jsonBuilder
                    .Append("\"" + columns[col] + "\"" + ":")
                    .Append("\"" +    data[col] + "\"" + ",");
            }
            jsonBuilder.Append("}");

            if (row != values.Count - 1) 
                jsonBuilder.Append(",");
        }
        jsonBuilder.Append("]");

        return jsonBuilder.ToString();
    }
}