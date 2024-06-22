using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

public enum RankEnum
{
    Floor, Kill, Damage, Speedrun
}

public class Rank : MonoBehaviour
{
    [SerializeField] Transform RankChart;

    private RankEnum mRankType;

    private void Start()
    {
        mRankType = RankEnum.Floor;

        InsertChart(10, 10, 10, 10.5f);

        RankUpdate();
    }

    private void InsertChart(int floor, int kill, int damage, float speedrun)
    {
        Param param = new Param();

        param.Add("TopFloor", floor);
        param.Add("TopKill", kill);
        param.Add("TopDamage", damage);
        param.Add("TopSpeedrun", speedrun);

        Backend.GameSchemaInfo.Insert("Rank", param, callback =>
        {
            Debug.Log($"Backend.GameInfo.Insert : {callback.GetMessage()}");
        });
    }

    private void RankUpdate()
    {
        string column = "";

        switch (mRankType)
        {
            case RankEnum.Floor:
                column = "TopDamage";
                break;
            case RankEnum.Kill:
                column = "TopKill";
                break;
            case RankEnum.Damage:
                column = "TopDamage";
                break;
            case RankEnum.Speedrun:
                column = "TopSpeedrun";
                break;
        }


        BackendReturnObject Result;
    }
}
