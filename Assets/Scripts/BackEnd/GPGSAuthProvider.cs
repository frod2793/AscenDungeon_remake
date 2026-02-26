using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: GPGS(Google Play Games Services) v2(v11+) 규격에 맞춘 인증 구현체입니다.
    /// </summary>
    public class GPGSAuthProvider : IGPGSAuthProvider
    {
        #region 내부 변수
        private bool m_isInitialized = false;
        #endregion

        #region 프로퍼티
        /// <summary>
        /// [설명]: 현재 로컬 사용자가 인증되었는지 확인합니다.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
#if UNITY_ANDROID
                return PlayGamesPlatform.Instance.IsAuthenticated();
#else
                return false;
#endif
            }
        }
        #endregion

        #region 공개 API
        /// <summary>
        /// [설명]: GPGS 플랫폼을 활성화합니다. (v2부터는 복잡한 Configuration 빌더가 생략됩니다)
        /// </summary>
        public void Initialize()
        {
            if (m_isInitialized) return;

#if UNITY_ANDROID
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            
            Debug.Log("[GPGS] PlayGamesPlatform v2 Activated");
#endif
            m_isInitialized = true;
        }

        /// <summary>
        /// [설명]: GPGS v2 규격에 맞춰 로그인을 시도하고 서버 인증 코드(AuthCode)를 획득합니다.
        /// </summary>
        public async UniTask<string> AuthenticateAndGetTokenAsync()
        {
#if UNITY_ANDROID
            Initialize();

            // 1. 인증 시도
            if (!IsAuthenticated)
            {
                var authCompletionSource = new UniTaskCompletionSource<SignInStatus>();
                
                PlayGamesPlatform.Instance.Authenticate((status) =>
                {
                    authCompletionSource.TrySetResult(status);
                });

                SignInStatus result = await authCompletionSource.Task;
                if (result != SignInStatus.Success)
                {
                    Debug.LogError($"[GPGS] Authentication failed with status: {result}");
                    return null;
                }
            }

            // 2. 서버 인증 코드(AuthCode) 요청 (v2 비동기 방식)
            var tokenCompletionSource = new UniTaskCompletionSource<string>();
            
            // forceRefreshToken을 true로 설정하여 1회용인 AuthCode를 매번 새롭게 발급받도록 강제합니다. (뒤끝 연동 401 에러 방지)
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, (authCode) =>
            {
                tokenCompletionSource.TrySetResult(authCode);
            });

            string finalToken = await tokenCompletionSource.Task;

            if (string.IsNullOrEmpty(finalToken))
            {
                Debug.LogError("[GPGS] Failed to retrieve ServerAuthCode via RequestServerSideAccess");
            }
            else
            {
                Debug.Log("[GPGS] ServerAuthCode retrieved successfully");
            }

            return finalToken;
#else
            Debug.LogWarning("[GPGS] AuthenticateAndGetTokenAsync is only supported on Android");
            return await UniTask.FromResult<string>(null);
#endif
        }
        #endregion
    }
}
