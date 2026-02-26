using System;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.BackEnd
{
    /// <summary>
    /// [설명]: 뒤끝 서버와의 통신을 담당하는 인터페이스입니다.
    /// </summary>
    public interface IBackEndService
    {
        /// <summary>
        /// [설명]: 게스트 로그인을 시도합니다.
        /// </summary>
        UniTask<BackEndResult> LoginGuestAsync();
        
        /// <summary>
        /// [설명]: 토큰 로그인을 시도합니다.
        /// </summary>
        UniTask<BackEndResult> LoginTokenAsync();
        
        /// <summary>
        /// [설명]: 구글 연동 로그인을 시도합니다.
        /// </summary>
        UniTask<BackEndResult> LoginGoogleAsync();
        
        /// <summary>
        /// [설명]: 사용자 닉네임이 설정되어 있는지 확인합니다.
        /// </summary>
        UniTask<bool> HasNicknameAsync();

        /// <summary>
        /// [설명]: 새로운 닉네임을 생성합니다.
        /// </summary>
        UniTask<bool> CreateNicknameAsync(string nickname);

        /// <summary>
        /// [설명]: 특정 테이블의 데이터를 가져옵니다.
        /// </summary>
        UniTask<BackEndResult<T>> GetGameDataAsync<T>(string tableName) where T : class, new();
        
        /// <summary>
        /// [설명]: 데이터를 서버에 저장합니다.
        /// </summary>
        UniTask<BackEndResult> SaveGameDataAsync(string tableName, object data);

        /// <summary>
        /// [설명]: GPGS 업적 해제를 위한 API 래퍼
        /// </summary>
        UniTask<BackEndResult> UnlockAchievement(string achievementId);

    }

    public class BackEndResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }
        
        public BackEndResult(bool isSuccess, string errorMessage = null, int statusCode = 0)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// [설명]: 뒤끝 서버 결과를 나타내는 클래스 (제네릭).
    /// </summary>
    public class BackEndResult<T> : BackEndResult where T : class
    {
        public T Data { get; set; }
        
        public BackEndResult(bool isSuccess, T data = null, string errorMessage = null, int statusCode = 0) 
            : base(isSuccess, errorMessage, statusCode)
        {
            Data = data;
        }
    }
}
