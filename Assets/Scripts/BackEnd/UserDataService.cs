using System;
using UnityEngine;
using BackEnd;
using Cysharp.Threading.Tasks;
using Assets.Scripts.BackEnd;

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: 사용자 데이터를 관리하는 서비스 클래스입니다.
    /// </summary>
    public class UserDataService
    {
        private readonly IBackEndService m_backEndService;

        public UserDataService(IBackEndService backEndService)
        {
            m_backEndService = backEndService;
        }

        /// <summary>
        /// [설명]: 스테이지 데이터를 가져옵니다.
        /// </summary>
        public async UniTask<bool> LoadStageData()
        {
            try
            {
                var result = await m_backEndService.GetGameDataAsync<StageData>("STAGE");
                
                if (result.IsSuccess && result.Data != null)
                {
                    GameLoger.Instance.SetStageUnlock(result.Data.StageData);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"스테이지 데이터 로드 중 오류 발생: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: 아이템 데이터를 가져옵니다.
        /// </summary>
        public async UniTask<bool> LoadItemData()
        {
            try
            {
                var result = await m_backEndService.GetGameDataAsync<ItemData>("ITem");
                
                if (result.IsSuccess && result.Data != null)
                {
                    ItemStateSaver.Instance.SetUnlockedItem(result.Data.ItemList);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"아이템 데이터 로드 중 오류 발생: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: 옵션 데이터를 가져옵니다.
        /// </summary>
        public async UniTask<bool> LoadOptionData()
        {
            try
            {
                var result = await m_backEndService.GetGameDataAsync<OptionData>("Option");
                
                if (result.IsSuccess && result.Data != null)
                {
                    GameLoger.Instance.ConOffset(result.Data.ControllerOffset);
                    GameLoger.Instance.ConDefScale(result.Data.ControllerDefScale);
                    GameLoger.Instance.ConMaxScale(result.Data.ControllerMaxScale);
                    GameLoger.Instance.ConAlpha(result.Data.ControllerAlpha);
                    GameLoger.Instance.ConPosX(result.Data.ControllerPosX);
                    GameLoger.Instance.ConPosY(result.Data.ControllerPosY);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"옵션 데이터 로드 중 오류 발생: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: IAP 데이터를 가져옵니다.
        /// </summary>
        public async UniTask<bool> LoadIAPData()
        {
            try
            {
                var result = await m_backEndService.GetGameDataAsync<IAPData>("IAP");
                
                if (result.IsSuccess && result.Data != null)
                {
                    IAP.Instance.AiP(result.Data.IAP);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"IAP 데이터 로드 중 오류 발생: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: 사용자 데이터를 저장합니다.
        /// </summary>
        public async UniTask<bool> SaveUserData()
        {
            try
            {
                var data = new PlayerData
                {
                    IAP = IAP.Instance.APP,
                    Gold = MoneyManager.Instance.Money,
                    Kill = GameLoger.Instance.KillCount,
                    Time = GameLoger.Instance.ElapsedTime.ToString()
                };

                var result = await m_backEndService.SaveGameDataAsync("Player", data);
                return result.IsSuccess;
            }
            catch (Exception e)
            {
                Debug.LogError($"사용자 데이터 저장 중 오류 발생: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: IAP 데이터를 저장합니다.
        /// </summary>
        public async UniTask<bool> SaveIAPData()
        {
            try
            {
                var data = new IAPData
                {
                    IAP = IAP.Instance.APP
                };

                var result = await m_backEndService.SaveGameDataAsync("IAP", data);
                return result.IsSuccess;
            }
            catch (Exception e)
            {
                Debug.LogError($"IAP 데이터 저장 중 오류 발생: {e.Message}");
                return false;
            }
        }
    }

    // 데이터 모델 클래스들
    public class StageData
    {
        public int StageData { get; set; }
    }

    public class ItemData
    {
        public List<int> ItemList { get; set; }
    }

    public class OptionData
    {
        public float ControllerOffset { get; set; }
        public float ControllerDefScale { get; set; }
        public float ControllerMaxScale { get; set; }
        public float ControllerAlpha { get; set; }
        public float ControllerPosX { get; set; }
        public float ControllerPosY { get; set; }
    }

    public class IAPData
    {
        public string IAP { get; set; }
    }

    public class PlayerData
    {
        public string IAP { get; set; }
        public int Gold { get; set; }
        public int Kill { get; set; }
        public string Time { get; set; }
    }
}
