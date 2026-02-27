using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: GPGS(Google Play Games Services) v2 규격에 맞춘 인증 서비스 구현체입니다.
    /// </summary>
    public class GPGSAuthService : IGPGSAuthService
    {
        #region 내부 변수
        private bool m_isInitialized = false;
        private bool m_isAuthenticating = false;

        #region 디버그 설정
#if UNITY_EDITOR
        /// <summary>
        /// [설명]: 에디터에서 GPGS 로직 도달 여부를 테스트하기 위해 인증 상태를 강제로 true로 리턴하게 합니다.
        /// </summary>
        private static bool s_debugForceAuth = false;

        public static void SetDebugForceAuth(bool value)
        {
            s_debugForceAuth = value;
            Debug.Log($"[GPGS] Debug Force Auth Set to: {value}");
        }
#endif
        #endregion
        #endregion

        #region 프로퍼티
        /// <summary>
        /// [설명]: 현재 로컬 사용자가 인증되었는지 확인합니다.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
#if UNITY_EDITOR
                if (s_debugForceAuth) return true;
#endif
#if UNITY_ANDROID
                return PlayGamesPlatform.Instance.IsAuthenticated();
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// [설명]: 로컬 사용자의 고유 ID를 가져옵니다.
        /// </summary>
        public string LocalUserID
        {
            get
            {
#if UNITY_ANDROID
                return PlayGamesPlatform.Instance.GetUserId();
#else
                return string.Empty;
#endif
            }
        }
        #endregion

        #region 공개 API
        /// <summary>
        /// [설명]: GPGS 플랫폼을 활성화합니다.
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
        /// [설명]: GPGS v2 인증을 시도하고 서버 인증 코드(AuthCode)를 획득합니다.
        /// </summary>
        public async UniTask<string> AuthenticateAndGetTokenAsync(CancellationToken cancellationToken = default)
        {
#if UNITY_ANDROID
            Initialize();

            // 중복 인증 요청 방지
            while (m_isAuthenticating)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            m_isAuthenticating = true;

            try
            {
                // 1. 플랫폼 인증 시도
                if (!IsAuthenticated)
                {
                    var authCompletionSource = new UniTaskCompletionSource<SignInStatus>();
                    PlayGamesPlatform.Instance.Authenticate((status) =>
                    {
                        authCompletionSource.TrySetResult(status);
                    });

                    SignInStatus result = await authCompletionSource.Task.AttachExternalCancellation(cancellationToken);
                    if (result != SignInStatus.Success)
                    {
                        Debug.LogError($"[GPGS] Authentication failed: {result}");
                        return null;
                    }
                }

                // 2. 서버 인증 코드(AuthCode) 요청
                var tokenCompletionSource = new UniTaskCompletionSource<string>();
                
                // true로 설정하여 최신 인증 코드를 강제 발급받습니다.
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, (authCode) =>
                {
                    tokenCompletionSource.TrySetResult(authCode);
                });

                string finalToken = await tokenCompletionSource.Task.AttachExternalCancellation(cancellationToken);

                if (string.IsNullOrEmpty(finalToken))
                {
                    Debug.LogError("[GPGS] Failed to retrieve ServerAuthCode");
                }
                
                return finalToken;
            }
            catch (System.OperationCanceledException)
            {
                Debug.LogWarning("[GPGS] Authentication cancelled");
                return null;
            }
            finally
            {
                m_isAuthenticating = false;
            }
#else
            Debug.LogWarning("[GPGS] AuthenticateAndGetTokenAsync는 Android에서만 지원됩니다.");
            return await UniTask.FromResult<string>(null);
#endif
        }
        #endregion
    }
}
