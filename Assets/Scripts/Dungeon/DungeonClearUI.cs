using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BackEnd;
using static BackEnd.SendQueue;
using System.Collections.Generic;
public class DungeonClearUI : MonoBehaviour
{
    [Header("UnlockDungeon")]
    [SerializeField] private int _UnlockDungeonIndex;
    [SerializeField] private DungeonSelectionInfo _UnlockDungeonInfo;

    [Header("UnlockDungeon Para")]
    [SerializeField] private Image _DungeonInfoImage;
    [SerializeField] private TMPro.TextMeshProUGUI _DungeonUnlockMessage;
    [SerializeField] private TMPro.TextMeshProUGUI _DungeonTitle;
    [SerializeField] private TMPro.TextMeshProUGUI _DungeonComment;

    [Header("Parameter")]
    [SerializeField] private TMPro.TextMeshProUGUI KillCount;
    [SerializeField] private TMPro.TextMeshProUGUI ClearTime;

    [Header("UnlockItem")]
    [SerializeField] private Item [] UnlockItems;
    [SerializeField] private Image[] ItemBoxes;

    private void Awake()
    {
        for (int i = 0; i < UnlockItems.Length; ++i)
        {
            
            ItemBoxes[i].sprite = UnlockItems[i].Sprite;
            ItemStateSaver.Instance.ItemUnlock(UnlockItems[i].ID);
         

        }

        


        Where where = new Where();
        Debug.Log("인벤");

        Param Param = new Param();
        Debug.Log(Param);

        where.Equal("gamerIndate", BackEndServerManager.Instance.mIndate);

        Param.Add("stagedata", GameLoger.Instance.UnlockDungeonIndex);


        Enqueue(Backend.GameSchemaInfo.Get, "STAGE", where, 1, (BackendReturnObject Getbro) =>
        {

            Debug.Log("스테이지커넥트");

            if (Getbro.IsSuccess())
            {
                Enqueue(Backend.GameInfo.Update, "STAGE", Getbro.GetReturnValuetoJSON()["rows"][0]["inDate"]["S"].ToString(), Param, (BackendReturnObject Updatebro) =>
                {
                    if (Updatebro.IsSuccess())
                        Debug.Log("스테이지 Update");
                    else
                        Debug.Log("falil 스테이지 update" + Updatebro);
                });
            }

            else
            {
                Enqueue(Backend.GameInfo.Insert, "STAGE", Param, (BackendReturnObject Insertbro) =>
                {
                    if (Insertbro.IsSuccess())
                        Debug.Log("스테이지 insert");
                    else
                        Debug.Log("falil 스테이지  insert" + Insertbro);
                });
            }
        });


        var list = ItemStateSaver.Instance.GetUnlockedItem();
        List<int> numlockedList = new List<int>();
        
        for(int i = 0; i<list.Count; i++)
        {
            numlockedList.Add((int)list[i].ID);
        }

       
        Debug.Log("인벤");

        Param _Param = new Param();
        Debug.Log(_Param);

        where.Equal("gamerIndate",BackEndServerManager.Instance.mIndate);

        _Param.Add("ItemList", numlockedList) ; 
       

        Enqueue(Backend.GameSchemaInfo.Get, "ITem", where, 1, (BackendReturnObject Getbro) =>
        {

            Debug.Log("인벤커넥트");

            if (Getbro.IsSuccess())
            {
                Enqueue(Backend.GameInfo.Update, "ITem", Getbro.GetReturnValuetoJSON()["rows"][0]["inDate"]["S"].ToString(), _Param, (BackendReturnObject Updatebro) =>
                {
                    if (Updatebro.IsSuccess())
                        Debug.Log("인벤 Update");
                    else
                        Debug.Log("falil 인벤 update" + Updatebro);
                });
            }
        
            else
        {
            Enqueue(Backend.GameInfo.Insert, "ITem", _Param, (BackendReturnObject Insertbro) =>
            {
                if (Insertbro.IsSuccess())
                    Debug.Log("인벤 insert");
                else
                    Debug.Log("falil 인벤  insert" + Insertbro);
            });
        }
    });



            int clearSec = Mathf.FloorToInt(GameLoger.Instance.ElapsedTime % 60f);
        int clearMin = Mathf.FloorToInt(GameLoger.Instance.ElapsedTime / 60f);

        ClearTime.text = $"{clearMin:D2} : {clearSec:D2}";
        KillCount.text = $"{GameLoger.Instance.KillCount:D3} 마리";


        _DungeonTitle.text       = _UnlockDungeonInfo.UnLockedTitle;
        _DungeonInfoImage.sprite = _UnlockDungeonInfo.UnLockedSprite;
        _DungeonComment.text     = _UnlockDungeonInfo.UnLockedComment;

        // 아직 해금되지 않은 던전일 때에만
        if (_UnlockDungeonIndex > GameLoger.Instance.UnlockDungeonIndex)
        {
            GameLoger.Instance.RecordStageUnlock();

            _DungeonUnlockMessage.text = "해금된 던전";
        }
        else
        {
            _DungeonUnlockMessage.text = "이미 해금됨";
        }
        SoundManager.Instance.PlaySound(SoundName.DungeonResult);
    }
   
    public void Close()
    {
        MainCamera.Instance.Fade(2.25f, FadeType.In, () =>
        {
            Inventory.Instance.Clear();

            Ads.Instance.ShowFrontAd();
            Ads.Instance.ClosedADEvent(() =>
            {
                SceneManager.LoadScene((int)SceneIndex.Town);
            });
        });
    }

}
