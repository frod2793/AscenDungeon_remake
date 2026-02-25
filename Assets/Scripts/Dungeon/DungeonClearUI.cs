using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BackEnd;
using static BackEnd.SendQueue;
using System.Collections.Generic;
using Assets.Scripts.Ad;

/// <summary>
/// [설명]: 던전 클리어 시 결과를 표시하고 서버에 데이터를 저장하는 UI 클래스입니다.
/// </summary>
public class DungeonClearUI : MonoBehaviour
{
    #region 에디터 설정
    [Header("UnlockDungeon")]
    [SerializeField, Tooltip("해금할 던전 인덱스")]
    private int m_unlockDungeonIndex;
    [SerializeField, Tooltip("해금할 던전 정보")]
    private DungeonSelectionInfo m_unlockDungeonInfo;

    [Header("UnlockDungeon UI")]
    [SerializeField] private Image m_dungeonInfoImage;
    [SerializeField] private TMPro.TextMeshProUGUI m_dungeonUnlockMessage;
    [SerializeField] private TMPro.TextMeshProUGUI m_dungeonTitle;
    [SerializeField] private TMPro.TextMeshProUGUI m_dungeonComment;

    [Header("Parameter UI")]
    [SerializeField] private TMPro.TextMeshProUGUI m_killCount;
    [SerializeField] private TMPro.TextMeshProUGUI m_clearTime;

    [Header("UnlockItem")]
    [SerializeField] private Item[] m_unlockItems;
    [SerializeField] private Image[] m_itemBoxes;
    #endregion

    #region 유니티 생명주기
    private void Awake()
    {
        if (m_unlockItems != null)
        {
            for (int i = 0; i < m_unlockItems.Length; ++i)
            {
                if (m_itemBoxes != null && i < m_itemBoxes.Length && m_unlockItems[i] != null)
                {
                    m_itemBoxes[i].sprite = m_unlockItems[i].GetItemInfo.ItemSprite;
                    ItemStateSaver.Instance.ItemUnlock(m_unlockItems[i].ID);
                }
            }
        }

        SaveDungeonProgress();

        int clearSec = Mathf.FloorToInt(GameLoger.Instance.ElapsedTime % 60f);
        int clearMin = Mathf.FloorToInt(GameLoger.Instance.ElapsedTime / 60f);

        if (m_clearTime != null) m_clearTime.text = $"{clearMin:D2} : {clearSec:D2}";
        if (m_killCount != null) m_killCount.text = $"{GameLoger.Instance.KillCount:D3} 마리";

        if (m_unlockDungeonInfo != null)
        {
            if (m_dungeonTitle != null) m_dungeonTitle.text = m_unlockDungeonInfo.UnLockedTitle;
            if (m_dungeonInfoImage != null) m_dungeonInfoImage.sprite = m_unlockDungeonInfo.UnLockedSprite;
            if (m_dungeonComment != null) m_dungeonComment.text = m_unlockDungeonInfo.UnLockedComment;
        }

        // 아직 해금되지 않은 던전일 때에만
        if (m_unlockDungeonIndex > GameLoger.Instance.UnlockDungeonIndex)
        {
            GameLoger.Instance.RecordStageUnlock();
            if (m_dungeonUnlockMessage != null) m_dungeonUnlockMessage.text = "해금된 던전";
        }
        else
        {
            if (m_dungeonUnlockMessage != null) m_dungeonUnlockMessage.text = "이미 해금됨";
        }

        SoundManager.Instance.PlaySound(SoundName.DungeonResult);
    }
    #endregion

    #region 내부 로직
    /// <summary>
    /// [설명]: 던전 진행 상황과 획득 아이템을 서버에 저장합니다.
    /// </summary>
    private void SaveDungeonProgress()
    {
        Where where = new Where();
        Param stageParam = new Param();
        string myIndate = BackEndServerManager.Instance.mIndate;

        where.Equal("gamerIndate", myIndate);
        stageParam.Add("stagedata", GameLoger.Instance.UnlockDungeonIndex);

        // GameSchemaInfo 대신 GameData 사용
        Enqueue(Backend.GameData.GetMyData, "STAGE", where, 1, (BackendReturnObject getBro) =>
        {
            if (getBro.IsSuccess())
            {
                // UpdateV2 사용
                string rowInDate = getBro.Rows()[0]["inDate"]["S"].ToString();
                Enqueue(Backend.GameData.UpdateV2, "STAGE", rowInDate, Backend.UserInDate, stageParam, (BackendReturnObject updateBro) =>
                {
                    if (updateBro.IsSuccess()) Debug.Log("스테이지 정보 업데이트 성공");
                });
            }
            else
            {
                Enqueue(Backend.GameData.Insert, "STAGE", stageParam, (BackendReturnObject insertBro) =>
                {
                    if (insertBro.IsSuccess()) Debug.Log("스테이지 정보 삽입 성공");
                });
            }
        });

        // 아이템 정보 저장
        var unlockedList = ItemStateSaver.Instance.GetUnlockedItem();
        List<int> itemIds = new List<int>();
        foreach (var item in unlockedList) itemIds.Add((int)item.ID);

        Param itemParam = new Param();
        itemParam.Add("ItemList", itemIds);

        Enqueue(Backend.GameData.GetMyData, "ITem", where, 1, (BackendReturnObject getBro) =>
        {
            if (getBro.IsSuccess())
            {
                string rowInDate = getBro.Rows()[0]["inDate"]["S"].ToString();
                Enqueue(Backend.GameData.UpdateV2, "ITem", rowInDate, Backend.UserInDate, itemParam, (BackendReturnObject updateBro) =>
                {
                    if (updateBro.IsSuccess()) Debug.Log("아이템 목록 업데이트 성공");
                });
            }
            else
            {
                Enqueue(Backend.GameData.Insert, "ITem", itemParam, (BackendReturnObject insertBro) =>
                {
                    if (insertBro.IsSuccess()) Debug.Log("아이템 목록 삽입 성공");
                });
            }
        });
    }
    #endregion

    #region 공개 API
    /// <summary>
    /// [설명]: 클리어 UI를 닫고 마을 씬으로 이동합니다.
    /// </summary>
    public void Close()
    {
        MainCamera.Instance.Fade(2.25f, FadeType.In, () =>
        {
            Inventory.Instance.Clear();
            
            // [안전 장치]: 광고 서비스 미초기화 시 광고 생략 후 바로 씬 전환
            if (AdsManager.Service != null)
            {
                AdsManager.Service.ShowInterstitialAd(() =>
                {
                    SceneManager.LoadScene((int)SceneIndex.Town);
                });
            }
            else
            {
                Debug.LogWarning("[DungeonClearUI] 광고 서비스가 준비되지 않아 광고 없이 진행합니다.");
                SceneManager.LoadScene((int)SceneIndex.Town);
            }
        });
    }
    #endregion
}
