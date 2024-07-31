using System;
using UnityEngine;
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
        private bool m_isInitialized = false;

        public BackEndService()
        {
            InitializeBackend().Forget();
        }

        /// <summary>
        /// [설명]: 뒤끝 SDK를 초기화합니다.
        /// </summary>
        private async UniTaskVoid InitializeBackend()
        {
            try
            {
                Backend.Initialize();
                await UniTask.WaitUntil(() => Backend.IsInitialized);
                m_isInitialized = true;
                Debug.Log("[Backend] 초기화 성공");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Backend] 초기화 중 예외 발생: {e.Message}");
            }
        }

        /// <summary>
        /// [설명]: 게스트 로그인을 시도합니다.
        /// </summary>
        public async UniTask<BackEndResult> LoginGuestAsync()
        {
            if (!m_isInitialized)
                return new BackEndResult(false, "Backend not initialized");

            try
            {
                var callback = await Backend.BMember.GuestLogin();
                
                if (callback.IsSuccess())
                {
                    Debug.Log("게스트 로그인 성공");
                    return new BackEndResult(true);
                }
                else
                {
                    Debug.LogError($"게스트 로그인 실패: {callback}");
                    return new BackEndResult(false, callback.ToString());
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
            if (!m_isInitialized)
                return new BackEndResult(false, "Backend not initialized");

            try
            {
                var callback = await Backend.BMember.LoginWithTheBackendToken();
                
                if (callback.IsSuccess())
                {
                    Debug.Log("토큰 로그인 성공");
                    return new BackEndResult(true);
                }
                else
                {
                    Debug.LogWarning($"토큰 로그인 실패: {callback}");
                    return new BackEndResult(false, callback.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"토큰 로그인 중 예외 발생: {e.Message}");
                return new BackEndResult(false, e.Message);
            }
        }

        /// <summary>
        /// [설명]: 구글 연동 로그인을 시도합니다.
        /// </summary>
        public async UniTask<BackEndResult> LoginGoogleAsync()
        {
#if UNITY_ANDROID
            if (!m_isInitialized)
                return new BackEndResult(false, "Backend not initialized");

            try
            {
                if (UnityEngine.Social.localUser.authenticated)
                {
                    string token = GetTokens();
                    var callback = await Backend.BMember.AuthorizeFederation(token, BackEnd.FederationType.Google, "gpgs");
                    
                    if (callback.IsSuccess())
                    {
                        Debug.Log("구글 로그인 성공");
                        return new BackEndResult(true);
                    }
                    else
                    {
                        Debug.LogError($"구글 로그인 실패: {callback}");
                        return new BackEndResult(false, callback.ToString());
                    }
                }
                else
                {
                    // 구글 인증이 필요할 경우
                    return new BackEndResult(false, "Google authentication required");
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
        /// [설명]: 특정 테이블의 데이터를 가져옵니다.
        /// </summary>
        public async UniTask<BackEndResult<T>> GetGameDataAsync<T>(string tableName) where T : class
        {
            if (!m_isInitialized)
                return new BackEndResult<T>(false, null, "Backend not initialized");

            try
            {
                var callback = await Backend.GameData.GetMyData(tableName, new Where(), 1);
                
                if (callback.IsSuccess() && callback.Rows().Count > 0)
                {
                    // 데이터를 T 타입으로 변환
                    var data = ConvertToData<T>(callback.Rows()[0]);
                    return new BackEndResult<T>(true, data);
                }
                else
                {
                    Debug.LogWarning($"데이터 가져오기 실패: {callback}");
                    return new BackEndResult<T>(false, null, callback.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 가져오기 중 예외 발생: {e.Message}");
                return new BackEndResult<T>(false, null, e.Message);
            }
        }

        /// <summary>
        /// [설명]: 데이터를 서버에 저장합니다.
        /// </summary>
        public async UniTask<BackEndResult> SaveGameDataAsync(string tableName, object data)
        {
            if (!m_isInitialized)
                return new BackEndResult(false, "Backend not initialized");

            try
            {
                // 데이터를 Param으로 변환
                var param = ConvertToParam(data);
                
                var callback = await Backend.GameData.Insert(tableName, param);
                
                if (callback.IsSuccess())
                {
                    Debug.Log("데이터 저장 성공");
                    return new BackEndResult(true);
                }
                else
                {
                    Debug.LogError($"데이터 저장 실패: {callback}");
                    return new BackEndResult(false, callback.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 저장 중 예외 발생: {e.Message}");
                return new BackEndResult(false, e.Message);
            }
        }

        private string GetTokens()
        {
            // GPGS 토큰 획득 로직 (기존 코드 참고)
            return null; 
        }

        private T ConvertToData<T>(Dictionary<string, object> row) where T : class
        {
            // 간단한 데이터 변환 로직 구현
            // 실제 구현은 필요에 따라 확장 가능
            return null;
        }

        private Param ConvertToParam(object data)
        {
            // 데이터를 Param으로 변환
            var param = new Param();
            
            // 간단한 구현 - 실제 구현은 필요에 따라 확장 가능
            return param;
        }
    }
}
