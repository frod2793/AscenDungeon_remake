using System;
using System.Collections.Generic;
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

        public async UniTask<bool> LoadStageData()
        {
            try
            {
                var result = await m_backEndService.GetGameDataAsync<StageData>("Stage");
                
                if (result.IsSuccess && result.Data != null)
                {
                    if (GameLoger.Instance != null)
                    {
                        GameLoger.Instance.SetStageUnlock(result.Data.stagedata);
                    }
                    return true;
                }
                
                // 데이터가 없는 경우(200 ok but no rows) 또는 테이블은 있는데 행이 없는 경우 초기 데이터 생성
                Debug.Log("[UserData] Stage 데이터가 없어 초기 생성을 시도합니다.");
                var initialData = new StageData { stagedata = 1 };
                var saveResult = await m_backEndService.SaveGameDataAsync("Stage", initialData);
                
                if (saveResult.IsSuccess)
                {
                    if (GameLoger.Instance != null)
                    {
                        GameLoger.Instance.SetStageUnlock(initialData.stagedata);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"스테이지 데이터 로드 중 오류 발생: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public async UniTask<bool> LoadItemData()
        {
            try
            {
                var result = await m_backEndService.GetGameDataAsync<ItemData>("Item");
                
                // [중요]: 유니티 API(싱글톤 인스턴스 접근 등)는 반드시 메인 스레드에서 실행되어야 함
                await UniTask.SwitchToMainThread();

                if (result.IsSuccess && result.Data != null)
                {
                    // [안전 장치]: ItemStateSaver.Instance 접근 전 가드 로직
                    if (ItemStateSaver.Instance != null)
                    {
                        ItemStateSaver.Instance.SetUnlockedItem(result.Data.ItemList);
                    }
                    else
                    {
                        Debug.LogError("[UserDataService] LoadItemData 실패: ItemStateSaver 인스턴스를 찾을 수 없습니다.");
                    }
                    return true;
                }
                
                Debug.Log("[UserData] Item 데이터가 없어 초기 생성을 시도합니다.");
                var initialData = new ItemData { ItemList = new List<int> { 0 } }; // 기본 아이템 0번 보유
                var saveResult = await m_backEndService.SaveGameDataAsync("Item", initialData);
                
                if (saveResult.IsSuccess)
                {
                    if (ItemStateSaver.Instance != null)
                    {
                        ItemStateSaver.Instance.SetUnlockedItem(initialData.ItemList);
                    }
                    return true;
                }
                return false;
            }
        catch (Exception e)
        {
            Debug.LogError($"아이템 데이터 로드 중 오류 발생: {e.Message}\n{e.StackTrace}");
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
                    ApplyOptionData(result.Data);
                    return true;
                }
                
                Debug.Log("[UserData] Option 데이터가 없어 초기 생성을 시도합니다.");
                var initialData = new OptionData 
                { 
                    ControllerOffset = 0, 
                    ControllerDefScale = 1, 
                    ControllerMaxScale = 1.5f, 
                    ControllerAlpha = 1,
                    ControllerPosX = 0,
                    ControllerPosY = 0
                };
                var saveResult = await m_backEndService.SaveGameDataAsync("Option", initialData);
                
                if (saveResult.IsSuccess)
                {
                    ApplyOptionData(initialData);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"옵션 데이터 로드 중 오류 발생: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private void ApplyOptionData(OptionData data)
        {
            if (GameLoger.Instance == null) return;

            GameLoger.Instance.ConOffset(data.ControllerOffset);
            GameLoger.Instance.ConDefScale(data.ControllerDefScale);
            GameLoger.Instance.ConMaxScale(data.ControllerMaxScale);
            GameLoger.Instance.ConAlpha(data.ControllerAlpha);
            GameLoger.Instance.ConPosX(data.ControllerPosX);
            GameLoger.Instance.ConPosY(data.ControllerPosY);
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
                    if (IAP.Instance != null)
                    {
                        // [안전 장치]: null/빈 문자열 방어 - bool.Parse(null)는 크래시를 유발함
                        bool iapValue = false;
                        if (!string.IsNullOrEmpty(result.Data.IAP))
                        {
                            bool.TryParse(result.Data.IAP, out iapValue);
                        }
                        IAP.Instance.AiP(iapValue);
                    }
                    return true;
                }
                
                Debug.Log("[UserData] IAP 데이터가 없어 초기 생성을 시도합니다.");
                var initialData = new IAPData { IAP = "false" };
                var saveResult = await m_backEndService.SaveGameDataAsync("IAP", initialData);
                
                if (saveResult.IsSuccess)
                {
                    if (IAP.Instance != null)
                    {
                        IAP.Instance.AiP(false);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"IAP 데이터 로드 중 오류 발생: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: 모든 사용자 데이터를 로드합니다.
        /// </summary>
        public async UniTask LoadAllUserDataAsync()
        {
            await UniTask.WhenAll(
                LoadStageData(),
                LoadItemData(),
                LoadOptionData(),
                LoadIAPData()
            );
            Debug.Log("[UserDataService] 모든 사용자 데이터 로드 완료");
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
                    IAP = (IAP.Instance != null) ? IAP.Instance.APP.ToString() : "false",
                    Gold = (MoneyManager.Instance != null) ? MoneyManager.Instance.Money : 0,
                    Kill = (GameLoger.Instance != null) ? GameLoger.Instance.KillCount : 0,
                    Time = (GameLoger.Instance != null) ? GameLoger.Instance.ElapsedTime.ToString() : "0"
                };

                var result = await m_backEndService.SaveGameDataAsync("Player", data);
                return result.IsSuccess;
            }
            catch (Exception e)
            {
                Debug.LogError($"사용자 데이터 저장 중 오류 발생: {e.Message}\n{e.StackTrace}");
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
                    IAP = IAP.Instance.APP.ToString()
                };

                var result = await m_backEndService.SaveGameDataAsync("IAP", data);
                return result.IsSuccess;
            }
            catch (Exception e)
            {
                Debug.LogError($"IAP 데이터 저장 중 오류 발생: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: 옵션 데이터를 저장합니다.
        /// </summary>
        public async UniTask<bool> SaveOptionData()
        {
            try
            {
                var data = new OptionData
                {
                    ControllerOffset = GameLoger.Instance != null ? GameLoger.Instance.ControllerOffset : 0,
                    ControllerDefScale = GameLoger.Instance != null ? GameLoger.Instance.ControllerDefScale : 1,
                    ControllerMaxScale = GameLoger.Instance != null ? GameLoger.Instance.ControllerMaxScale : 1,
                    ControllerAlpha = GameLoger.Instance != null ? GameLoger.Instance.ControllerAlpha : 1,
                    ControllerPosX = GameLoger.Instance != null ? GameLoger.Instance.ControllerPos.x : 0,
                    ControllerPosY = GameLoger.Instance != null ? GameLoger.Instance.ControllerPos.y : 0
                };

                var result = await m_backEndService.SaveGameDataAsync("Option", data);
                return result.IsSuccess;
            }
            catch (Exception e)
            {
                Debug.LogError($"옵션 데이터 저장 중 오류 발생: {e.Message}");
                return false;
            }
        }
    }

    // 데이터 모델 클래스들 (뒤끝 콘솔 컬럼명과 일치해야 함)
    public class StageData
    {
        public int stagedata { get; set; }
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
