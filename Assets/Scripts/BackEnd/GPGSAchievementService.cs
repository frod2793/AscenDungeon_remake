using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
#endif

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: GPGS 업적 서비스 구현체입니다. 인증 상태를 확인하고 업적 작업을 수행합니다.
    /// </summary>
    public class GPGSAchievementService : IGPGSAchievementService
    {
        #region 내부 변수
        private readonly IGPGSAuthService m_authService;
        #endregion

        #region 초기화
        /// <summary>
        /// [설명]: 생성자입니다. 인증 상태 확인을 위해 인증 서비스를 주입받습니다.
        /// </summary>
        /// <param name="authService">GPGS 인증 서비스</param>
        public GPGSAchievementService(IGPGSAuthService authService)
        {
            m_authService = authService;
        }
        #endregion

        #region 공개 API
        /// <summary>
        /// [설명]: 특정 업적을 해제합니다.
        /// </summary>
        public async UniTask<BackEndResult> UnlockAchievementAsync(string achievementId)
        {
            Debug.Log($"[GPGS] UnlockAchievementAsync 시작: {achievementId}");

#if UNITY_EDITOR
            // 에디터용 시뮬레이션 로직
            Debug.Log($"[GPGS] 에디터 시뮬레이션: 업적 해제 시도 (ID: {achievementId})");
            await UniTask.Delay(100); // 가상의 지연
            Debug.Log($"[GPGS] 에디터 시뮬레이션 성공: {achievementId}");
            return new BackEndResult(true);
#elif UNITY_ANDROID
            if (m_authService == null || !m_authService.IsAuthenticated)
            {
                Debug.LogWarning("[GPGS] 인증되지 않음. 업적 해제 불가.");
                return new BackEndResult(false, "GPGS not authenticated");
            }

            Debug.Log($"[GPGS] PlayGamesPlatform.Instance.UnlockAchievement 호출 시도: {achievementId}");
            // [사용자 요구사항]: PlayGamesPlatform.Instance.UnlockAchievement 사용 필수
            PlayGamesPlatform.Instance.UnlockAchievement(achievementId, success => 
            {
                Debug.Log($"[GPGS] UnlockAchievement 콜백 수신 - 성공 여부: {success}, ID: {achievementId}");
                if (success) Debug.Log($"[GPGS] Achievement Unlocked: {achievementId}");
                else Debug.LogError($"[GPGS] Failed to unlock achievement: {achievementId}");
            });

            // 인터페이스 호환성을 위해 UniTask 결과 반환
            return await UniTask.FromResult(new BackEndResult(true));
#else
            Debug.LogWarning("[GPGS] 업적 기능은 Android에서만 동작합니다.");
            return await UniTask.FromResult(new BackEndResult(false, "Unsupported platform"));
#endif
        }

        /// <summary>
        /// [설명]: 업적의 진척도를 보고합니다. (누적 방식)
        /// </summary>
        public async UniTask<BackEndResult> ReportProgressAsync(string achievementId, double progress)
        {
#if UNITY_ANDROID
            if (m_authService == null || !m_authService.IsAuthenticated)
            {
                return new BackEndResult(false, "GPGS not authenticated");
            }

            var utcs = new UniTaskCompletionSource<bool>();
            // [교체완료]: PlayGamesPlatform.Instance.ReportProgress 사용
            PlayGamesPlatform.Instance.ReportProgress(achievementId, progress, success => utcs.TrySetResult(success));

            bool isSuccess = await utcs.Task;
            return new BackEndResult(isSuccess);
#else
            return await UniTask.FromResult(new BackEndResult(false, "Unsupported platform"));
#endif
        }

        /// <summary>
        /// [설명]: 업적 리스트 UI를 호출합니다.
        /// </summary>
        public void ShowAchievementUI()
        {
#if UNITY_ANDROID
            if (m_authService != null && m_authService.IsAuthenticated)
            {
                // [교체완료]: PlayGamesPlatform.Instance.ShowAchievementsUI 사용
                PlayGamesPlatform.Instance.ShowAchievementsUI();
            }
            else
            {
                Debug.LogWarning("[GPGS] 인증되지 않아 업적 UI를 열 수 없습니다.");
            }
#endif
        }
        #endregion
    }
}
