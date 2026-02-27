using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: GPGS(Google Play Games Services) 인증 및 토큰 관리를 담당하는 서비스 인터페이스입니다.
    /// </summary>
    public interface IGPGSAuthService
    {
        #region 프로퍼티
        /// <summary>
        /// [설명]: 사용자가 현재 로그인(인증)된 상태인지 여부를 반환합니다.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// [설명]: 로컬 사용자의 플랫폼 고유 ID를 반환합니다.
        /// </summary>
        string LocalUserID { get; }
        #endregion

        #region 공개 API
        /// <summary>
        /// [설명]: GPGS 플랫폼을 초기화합니다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// [설명]: 플랫폼 인증을 시도하고 서버 검증을 위한 인증 코드(AuthCode)를 비동기로 반환합니다.
        /// </summary>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>인증 성공 시 AuthCode, 실패 시 null</returns>
        UniTask<string> AuthenticateAndGetTokenAsync(CancellationToken cancellationToken = default);
        #endregion
    }
}
