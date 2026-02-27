using Cysharp.Threading.Tasks;

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: GPGS 업적(Achievement) 및 게임 진척도 관련 기능을 담당하는 서비스 인터페이스입니다.
    /// </summary>
    public interface IGPGSAchievementService
    {
        #region 공개 API
        /// <summary>
        /// [설명]: 특정 업적을 해제(Unlock)합니다.
        /// </summary>
        /// <param name="achievementId">GPGS 콘솔에 정의된 업적 ID</param>
        /// <returns>해제 결과 (성공 여부 및 메시지)</returns>
        UniTask<BackEndResult> UnlockAchievementAsync(string achievementId);

        /// <summary>
        /// [설명]: 업적의 누적 진척도를 보고합니다. (예: 50% 완료)
        /// </summary>
        /// <param name="achievementId">GPGS 콘솔에 정의된 업적 ID</param>
        /// <param name="progress">0.0 ~ 100.0 사이의 완료율</param>
        UniTask<BackEndResult> ReportProgressAsync(string achievementId, double progress);

        /// <summary>
        /// [설명]: GPGS 네이티브 업적 UI를 화면에 호출합니다.
        /// </summary>
        void ShowAchievementUI();
        #endregion
    }
}
