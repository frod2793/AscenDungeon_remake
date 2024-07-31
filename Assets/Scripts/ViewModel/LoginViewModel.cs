using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Assets.Scripts.BackEnd;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.ViewModel
{
    /// <summary>
    /// [설명]: 로그인 화면의 뷰모델 클래스입니다.
    /// </summary>
    public class LoginViewModel
    {
        private readonly IBackEndService m_backEndService;
        
        /// <summary>
        /// [설명]: 로딩 상태를 나타냅니다.
        /// </summary>
        public bool IsLoading { get; private set; }
        
        /// <summary>
        /// [설명]: 에러 메시지를 처리하는 액션입니다.
        /// </summary>
        public Action<string> OnErrorMessage { get; set; }
        
        /// <summary>
        /// [설명]: 로그인 성공 시 호출되는 액션입니다.
        /// </summary>
        public Action OnLoginSuccess { get; set; }

        /// <summary>
        /// [설명]: 생성자입니다.
        /// </summary>
        public LoginViewModel(IBackEndService backEndService)
        {
            m_backEndService = backEndService;
        }

        /// <summary>
        /// [설명]: 게스트 로그인을 시도합니다.
        /// </summary>
        public async UniTask TryGuestLogin()
        {
            IsLoading = true;
            
            try
            {
                var result = await m_backEndService.LoginGuestAsync();
                
                if (result.IsSuccess)
                {
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnErrorMessage?.Invoke("게스트 로그인 실패: " + result.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                OnErrorMessage?.Invoke("게스트 로그인 중 오류 발생: " + e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// [설명]: 토큰 로그인을 시도합니다.
        /// </summary>
        public async UniTask TryTokenLogin()
        {
            IsLoading = true;
            
            try
            {
                var result = await m_backEndService.LoginTokenAsync();
                
                if (result.IsSuccess)
                {
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnErrorMessage?.Invoke("토큰 로그인 실패: " + result.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                OnErrorMessage?.Invoke("토큰 로그인 중 오류 발생: " + e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// [설명]: 구글 로그인을 시도합니다.
        /// </summary>
        public async UniTask TryGoogleLogin()
        {
            IsLoading = true;
            
            try
            {
                var result = await m_backEndService.LoginGoogleAsync();
                
                if (result.IsSuccess)
                {
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnErrorMessage?.Invoke("구글 로그인 실패: " + result.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                OnErrorMessage?.Invoke("구글 로그인 중 오류 발생: " + e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
