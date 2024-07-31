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
        /// [설명]: 게스트 로그인을 시도합니다.
        /// </summary>
        UniTask<BackEndResult> LoginGoogleAsync();
        
        /// <summary>
        /// [설명]: 특정 테이블의 데이터를 가져옵니다.
        /// </summary>
        UniTask<BackEndResult<T>> GetGameDataAsync<T>(string tableName) where T : class;
        
        /// <summary>
        /// [설명]: 데이터를 서버에 저장합니다.
        /// </summary>
        UniTask<BackEndResult> SaveGameDataAsync(string tableName, object data);
    }

    /// <summary>
    /// [설명]: 뒤끝 서버 결과를 나타내는 클래스입니다.
    /// </summary>
    public class BackEndResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        
        public BackEndResult(bool isSuccess, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// [설명]: 뒤끝 서버 결과를 나타내는 클래스 (제네릭).
    /// </summary>
    public class BackEndResult<T> : BackEndResult
    {
        public T Data { get; set; }
        
        public BackEndResult(bool isSuccess, T data = null, string errorMessage = null) 
            : base(isSuccess, errorMessage)
        {
            Data = data;
        }
    }
}
