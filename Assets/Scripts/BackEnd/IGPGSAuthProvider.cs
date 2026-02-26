using Cysharp.Threading.Tasks;

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: GPGS(Google Play Games Services) 인증 및 토큰 획득을 담당하는 인터페이스입니다.
    /// </summary>
    public interface IGPGSAuthProvider
    {
        #region 프로퍼티
        /// <summary>
        /// [설명]: GPGS 인증 여부를 반환합니다.
        /// </summary>
        bool IsAuthenticated { get; }
        #endregion

        #region 공개 API
        /// <summary>
        /// [설명]: GPGS 플랫폼을 초기화합니다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// [설명]: GPGS 로그인을 시도하고 백엔드 서버 검증용 토큰을 반환합니다.
        /// </summary>
        /// <returns>성공 시 인증 토큰(AuthCode/IdToken), 실패 시 null</returns>
        UniTask<string> AuthenticateAndGetTokenAsync();

        /// <summary>
        /// 업적 진행 보고 및 UI 노출 API 확장
        /// </summary>
        void ReportProgress(string achievementId, double progress);

        /// <summary>
        /// 업적 UI 표시
        /// </summary>
        void ShowAchievementsUI();
        #endregion
    }
}
