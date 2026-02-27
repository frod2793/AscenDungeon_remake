 using System;
 using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using BackEnd;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using LitJson;

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: 뒤끝 서버와의 통신을 담당하는 서비스 클래스입니다.
    /// </summary>
    public class BackEndService : IBackEndService
    {
        #region 내부 변수
        private readonly IGPGSAuthService m_gpgsAuthService;
        private readonly IGPGSAchievementService m_gpgsAchievementService;
        #endregion

        #region 초기화
        /// <summary>
        /// [설명]: 생성자입니다. GPGS 인증 및 업적 서비스들을 주입받습니다.
        /// DI(Dependency Injection) 컨벤션에 따라 인터페이스를 통해 결합도를 낮춥니다.
        /// </summary>
        /// <param name="gpgsAuthService">GPGS 인증 서비스</param>
        /// <param name="gpgsAchievementService">GPGS 업적 서비스</param>
        public BackEndService(IGPGSAuthService gpgsAuthService, IGPGSAchievementService gpgsAchievementService)
        {
            m_gpgsAuthService = gpgsAuthService;
            m_gpgsAchievementService = gpgsAchievementService;
        }
        #endregion

        /// <summary>
        /// [설명]: 뒤끝 SDK를 초기화합니다.
        /// </summary>

        /// <summary>
        /// [설명]: 게스트 로그인을 시도합니다.
        /// 실패 시 'bad customId' 오류가 발생하면 로컬 정보를 초기화하고 1회 재시도합니다.
        /// </summary>
        public async UniTask<BackEndResult> LoginGuestAsync()
        {
            // [순서 준수]: SDK가 초기화될 때까지 대기
            await UniTask.WaitUntil(() => Backend.IsInitialized);

            try
            {
                // 1차 시도
                var callback = await ExecuteAsync(cb => Backend.BMember.GuestLogin(cb));
                
                // [복구 로직]: 'bad customId' 에러 발생 시 자동 대응
                if (!callback.IsSuccess() && callback.GetMessage().Contains("bad customId"))
                {
                    Debug.LogWarning("[Backend] 손상된 게스트 ID 감지. 로컬 데이터 초기화 후 재시도합니다.");
                    
                    // 로컬 게스트 정보 강제 삭제
                    Backend.BMember.DeleteGuestInfo();
                    
                    // 2차 시도 (재생성 유도)
                    callback = await ExecuteAsync(cb => Backend.BMember.GuestLogin(cb));
                }

                if (callback.IsSuccess())
                {
                    Debug.Log("게스트 로그인 성공");
                    return new BackEndResult(true, null, GetStatusCodeSafe(callback.GetStatusCode()));
                }
                else
                {
                    Debug.LogError($"게스트 로그인 최종 실패: {callback}");
                    return new BackEndResult(false, callback.ToString(), GetStatusCodeSafe(callback.GetStatusCode()));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"게스트 로그인 중 예외 발생: {e.Message}");
                return new BackEndResult(false, e.Message);
            }
        }

        /// <summary>
        /// [설명]: 토큰 로그인을 시도합니다.
        /// </summary>
        public async UniTask<BackEndResult> LoginTokenAsync()
        {
            await UniTask.WaitUntil(() => Backend.IsInitialized);

            try
            {
                var callback = await ExecuteAsync(cb => Backend.BMember.LoginWithTheBackendToken(cb));
                
                if (callback.IsSuccess())
                {
                    Debug.Log("토큰 로그인 성공");
                    return new BackEndResult(true, null, GetStatusCodeSafe(callback.GetStatusCode()));
                }
                else
                {
                    Debug.LogWarning($"토큰 로그인 실패: {callback}");
                    return new BackEndResult(false, callback.ToString(), GetStatusCodeSafe(callback.GetStatusCode()));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"토큰 로그인 중 예외 발생: {e.Message}");
                return new BackEndResult(false, e.Message);
            }
        }

        /// <summary>
        /// [설명]: 구글 연동 로그인을 시도합니다. (GPGS v2 대응)
        /// </summary>
        public async UniTask<BackEndResult> LoginGoogleAsync()
        {
#if UNITY_ANDROID
            await UniTask.WaitUntil(() => Backend.IsInitialized);

            try
            {
                // [개선]: GPGS v2에서는 AuthenticateAndGetTokenAsync()가 AuthCode(일회용 코드)를 반환합니다.
                string authCode = await m_gpgsAuthService.AuthenticateAndGetTokenAsync();

                if (!string.IsNullOrEmpty(authCode))
                {
                    // [핵심]: GPGS v2 인증 코드를 뒤끝 서버를 통해 액세스 토큰으로 교환합니다.
                    // 액세스 토큰 교환은 네트워크 통신을 포함하므로 비동기 처리를 고려하되, 
                    // SDK 사양에 따라 동기 API 호출 후 결과를 확인합니다.
                    BackendReturnObject tokenBro = Backend.BMember.GetGPGS2AccessToken(authCode);
                    
                    if (!tokenBro.IsSuccess())
                    {
                        Debug.LogError($"[GPGS] 액세스 토큰 교환 실패: {tokenBro}");
                        return new BackEndResult(false, tokenBro.ToString(), GetStatusCodeSafe(tokenBro.GetStatusCode()));
                    }

                    // 발급받은 액세스 토큰 추출
                    string accessToken = tokenBro.GetReturnValuetoJSON()["access_token"].ToString();

                    // [수정]: GPGS v2 환경에서는 FederationType.GPGS2를 사용해야 합니다.
                    var callback = await ExecuteAsync(cb => Backend.BMember.AuthorizeFederation(accessToken, global::BackEnd.FederationType.GPGS2, cb));
                    
                    if (callback.IsSuccess())
                    {
                        Debug.Log("구글 로그인(GPGS v2) 성공");
                        return new BackEndResult(true, null, GetStatusCodeSafe(callback.GetStatusCode()));
                    }
                    else
                    {
                        Debug.LogError($"구글 로그인 실패: {callback}");
                        return new BackEndResult(false, callback.ToString(), GetStatusCodeSafe(callback.GetStatusCode()));
                    }
                }
                else
                {
                    // GPGS 인증 실패 또는 코드 획득 실패
                    return new BackEndResult(false, "GPGS authentication failed or AuthCode is missing");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"구글 로그인 중 예외 발생: {e.Message}");
                return new BackEndResult(false, e.Message);
            }
#else
            return new BackEndResult(false, "Google login not supported on this platform");
#endif
        }

        /// <summary>
        /// [설명]: GPGS 업적 해제 요청 래퍼
        /// </summary>
        public async UniTask<BackEndResult> UnlockAchievement(string achievementId)
        {
            Debug.Log($"[Backend] UnlockAchievement 진입: {achievementId}");
            // Ensure backend is initialized
            await UniTask.WaitUntil(() => Backend.IsInitialized);
            Debug.Log("[Backend] UnlockAchievement: Backend 초기화 확인됨.");

            try
            {
                bool isAuthenticated = m_gpgsAuthService != null && m_gpgsAuthService.IsAuthenticated;
                Debug.Log($"[Backend] UnlockAchievement: GPGS 인증 상태 = {isAuthenticated}");

                if (isAuthenticated)
                {
                    // 업적 해제는 Achievement 서비스로 위임
                    Debug.Log("[Backend] UnlockAchievement: GPGSAchievementService로 위임합니다.");
                    return await m_gpgsAchievementService.UnlockAchievementAsync(achievementId);
                }
                else
                {
                    Debug.LogWarning("[Backend] GPGS 인증되지 않음. 업적 해제 불가.");
                    return new BackEndResult(false, "GPGS not authenticated");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Backend] 업적 해제 예외: " + e.Message);
                return new BackEndResult(false, e.Message);
            }
        }

        /// <summary>
        /// [설명]: 사용자 닉네임이 설정되어 있는지 확인합니다.
        /// </summary>
        public async UniTask<bool> HasNicknameAsync()
        {
            await UniTask.WaitUntil(() => Backend.IsInitialized);
            
            var callback = await ExecuteAsync(cb => Backend.BMember.GetUserInfo(cb));
            
            if (callback.IsSuccess())
            {
                var row = callback.GetReturnValuetoJSON()["row"];
                if (row.ContainsKey("nickname") && row["nickname"] != null)
                {
                    string nickname = row["nickname"].ToString();
                    return !string.IsNullOrEmpty(nickname);
                }
            }
            
            return false;
        }

        /// <summary>
        /// [설명]: 새로운 닉네임을 생성합니다.
        /// </summary>
        public async UniTask<bool> CreateNicknameAsync(string nickname)
        {
            await UniTask.WaitUntil(() => Backend.IsInitialized);
            
            var callback = await ExecuteAsync(cb => Backend.BMember.CreateNickname(nickname, cb));
            
            if (callback.IsSuccess())
            {
                Debug.Log($"[Backend] 닉네임 생성 성공: {nickname}");
                return true;
            }
            else
            {
                Debug.LogError($"[Backend] 닉네임 생성 실패: {callback}");
                return false;
            }
        }

        /// <summary>
        /// [설명]: 특정 테이블의 데이터를 가져옵니다.
        /// </summary>
        public async UniTask<BackEndResult<T>> GetGameDataAsync<T>(string tableName) where T : class, new()
        {
            await UniTask.WaitUntil(() => Backend.IsInitialized);

            try
            {
                // [수정]: ExecuteAsync의 타입이 BackendCallback으로 고정되었으므로, 기존 성공 사례인 4개 인자 오버로드 사용 가능
                var callback = await ExecuteAsync(cb => Backend.GameData.GetMyData(tableName, new global::BackEnd.Where(), 1, cb));
                
                if (callback.IsSuccess() && SafeCount(callback.Rows()) > 0)
                {
                    // 데이터를 T 타입으로 변환
                    var data = ConvertToData<T>(callback.Rows()[0]);
                    return new BackEndResult<T>(true, data, null, GetStatusCodeSafe(callback.GetStatusCode()));
                }
                else
                {
                    // 데이터가 없거나(200 ok but rows=0) 테이블이 없는(404) 경우 모두 처리
                    Debug.LogWarning($"[Backend] 데이터 가져오기 결과 - Table: {tableName}, Status: {GetStatusCodeSafe(callback.GetStatusCode())}, Content: {callback}");
                    // 404 혹은 Rows 0인 경우 기본 데이터 자동 생성 시도
                    if (GetStatusCodeSafe(callback.GetStatusCode()) == 404 || SafeCount(callback.Rows()) == 0)
                    {
                        try
                        {
                            var defaultData = new T();
                            var saveResult = await SaveGameDataAsync(tableName, defaultData);
                            if (saveResult.IsSuccess)
                            {
                                // 재시도하여 데이터 조회
                                var retry = await GetGameDataAsync<T>(tableName);
                                if (retry.IsSuccess)
                                {
                                    return retry;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[Backend] 자동 초기 데이터 생성 실패: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                    return new BackEndResult<T>(false, null, callback.ToString(), GetStatusCodeSafe(callback.GetStatusCode()));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 가져오기 중 예외 발생: {e.Message}\n{e.StackTrace}");
                return new BackEndResult<T>(false, null, e.Message);
            }
        }

        /// <summary>
        /// [설명]: 데이터를 서버에 저장합니다.
        /// </summary>
        public async UniTask<BackEndResult> SaveGameDataAsync(string tableName, object data)
        {
            await UniTask.WaitUntil(() => Backend.IsInitialized);

            try
            {
                // 데이터를 Param으로 변환
                var param = ConvertToParam(data);
                
                var callback = await ExecuteAsync(cb => Backend.GameData.Insert(tableName, param, cb));
                
                if (callback.IsSuccess())
                {
                    Debug.Log($"데이터 저장 성공 - Table: {tableName}");
                    return new BackEndResult(true, null, GetStatusCodeSafe(callback.GetStatusCode()));
                }
                else
                {
                    Debug.LogError($"데이터 저장 실패 - Table: {tableName}, Detail: {callback}");
                    return new BackEndResult(false, callback.ToString(), GetStatusCodeSafe(callback.GetStatusCode()));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 저장 중 예외 발생: {e.Message}\n{e.StackTrace}");
                return new BackEndResult(false, e.Message);
            }
        }

        private int SafeCount(JsonData data)
        {
            try
            {
                return data != null ? data.Count : 0;
            }
            catch
            {
                return 0;
            }
        }

        private int GetStatusCodeSafe(string code)
        {
            int v;
            if (int.TryParse(code, out v)) return v;
            return 0;
        }





        /// <summary>
        /// [설명]: 콜백 기반의 뒤끝 API를 UniTask로 변환하는 래퍼 메서드입니다.
        /// SendQueue를 사용하여 호출의 안정성을 보장합니다.
        /// </summary>
        private async UniTask<BackendReturnObject> ExecuteAsync(global::BackEnd.SendQueue.SendQueueDelegate backendAction)
        {
            var utcs = new UniTaskCompletionSource<BackendReturnObject>();
            
            // [핵심 변경]: SendQueue를 사용하여 API 호출을 큐에 삽입
            // BackEndServerManager에서 Poll()을 호출하여 처리합니다.
            global::BackEnd.SendQueue.Enqueue(backendAction, bro => utcs.TrySetResult(bro));
            
            return await utcs.Task;
        }

        /// <summary>
        /// [설명]: 뒤끝의 JsonData를 POCO 객체로 변환합니다.
        /// </summary>
        private T ConvertToData<T>(JsonData row) where T : class, new()
        {
            T instance = new T();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                if (row.ContainsKey(prop.Name))
                {
                    var data = row[prop.Name];
                    string type = string.Empty;
                    
                    // 뒤끝 데이터 타입 추출 (S, N, B, L, M 등)
                    foreach (var key in data.Keys)
                    {
                        type = key;
                        break;
                    }

                    if (string.IsNullOrEmpty(type)) continue;

                    try
                    {
                        object value = null;
                        switch (type)
                        {
                            case "S":
                                value = data[type]?.ToString();
                                break;
                            case "N":
                                if (prop.PropertyType == typeof(int))
                                {
                                    if (int.TryParse(data[type]?.ToString(), out int iv)) value = iv;
                                }
                                else if (prop.PropertyType == typeof(float))
                                {
                                    if (float.TryParse(data[type]?.ToString(), out float fv)) value = fv;
                                }
                                else if (prop.PropertyType == typeof(double))
                                {
                                    if (double.TryParse(data[type]?.ToString(), out double dv)) value = dv;
                                }
                                break;
                            case "BOOL":
                                if (bool.TryParse(data[type]?.ToString(), out bool bv)) value = bv;
                                else if (data[type]?.ToString() == "1") value = true;
                                else if (data[type]?.ToString() == "0") value = false;
                                break;
                            case "L":
                                // 리스트 처리 (단순 int 리스트 가정)
                                if (prop.PropertyType == typeof(List<int>))
                                {
                                    var list = new List<int>();
                                    var listData = data[type];
                                    if (listData != null)
                                    {
                                        for (int i = 0; i < listData.Count; i++)
                                            if (int.TryParse(listData[i]["N"]?.ToString(), out int item)) list.Add(item);
                                    }
                                    value = list;
                                }
                                break;
                        }

                        if (value != null)
                            prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[Backend] 필드 {prop.Name} (Type: {type}) 변환 실패: {e.Message}");
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// [설명]: POCO 객체를 뒤끝 Param 객체로 변환합니다.
        /// </summary>
        private Param ConvertToParam(object data)
        {
            var param = new Param();
            var properties = data.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(data);
                if (value != null)
                {
                    param.Add(prop.Name, value);
                }
            }
            
            return param;
        }
    }
}
