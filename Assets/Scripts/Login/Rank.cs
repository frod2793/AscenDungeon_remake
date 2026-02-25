using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

/// <summary>
/// [설명]: 게임의 랭킹 시스템을 관리하고 서버와 연동하는 클래스입니다.
/// </summary>
public enum RankEnum
{
    Floor, Kill, Damage, Speedrun
}

/// <summary>
/// [설명]: 랭킹 데이터를 처리하고 차트에 삽입하는 기능을 제공합니다.
/// </summary>
public class Rank : MonoBehaviour
{
    #region 에디터 설정
    [SerializeField, Tooltip("랭킹 차트가 표시될 트랜스폼")]
    private Transform m_rankChart;
    #endregion

    #region 내부 필드
    private RankEnum m_rankType;
    #endregion

    #region 유니티 생명주기
    private void Start()
    {
        m_rankType = RankEnum.Floor;

        InsertChart(10, 10, 10, 10.5f);
        RankUpdate();
    }
    #endregion

    #region 내부 로직
    /// <summary>
    /// [설명]: 랭킹 차트에 새로운 데이터를 삽입합니다.
    /// </summary>
    /// <param name="floor">도달 층수</param>
    /// <param name="kill">처치 수</param>
    /// <param name="damage">누적 데미지</param>
    /// <param name="speedrun">스피드런 기록</param>
    private void InsertChart(int floor, int kill, int damage, float speedrun)
    {
        Param param = new Param();

        param.Add("TopFloor", floor);
        param.Add("TopKill", kill);
        param.Add("TopDamage", damage);
        param.Add("TopSpeedrun", speedrun);

        // GameSchemaInfo 대신 GameData 사용
        Backend.GameData.Insert("Rank", param, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log($"[Rank] 데이터 삽입 성공: {callback.GetMessage()}");
            }
            else
            {
                Debug.LogError($"[Rank] 데이터 삽입 실패: {callback}");
            }
        });
    }

    /// <summary>
    /// [설명]: 현재 설정된 랭킹 타입에 따라 랭킹 정보를 갱신합니다.
    /// </summary>
    private void RankUpdate()
    {
        string column = string.Empty;

        switch (m_rankType)
        {
            case RankEnum.Floor:
                column = "TopDamage"; // 기존 로직 유지 (Floor인데 Damage인 점 주의)
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

        // GameSchemaInfo 대신 GameData 사용
        Backend.GameData.GetMyData("Rank", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log($"[Rank] 데이터 로드 성공: {callback.GetMessage()}");
            }
            else
            {
                Debug.LogError($"[Rank] 데이터 로드 실패: {callback}");
            }
        });
    }
    #endregion
}
