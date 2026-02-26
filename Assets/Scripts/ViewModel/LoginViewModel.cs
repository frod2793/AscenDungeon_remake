using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Assets.Scripts.BackEnd;
using Assets.Scripts.Modules;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.ViewModel
{
    /// <summary>
    /// [설명]: 로그인 화면의 뷰모델 클래스입니다.
    /// </summary>
    public class LoginViewModel
    {
    private readonly IBackEndService m_backEndService;
    private readonly UserDataService m_userDataService;
    private readonly ISceneNavigationService m_navigationService;
    // 닉네임 설정 필요 시 UI를 표시하도록 하는 콜백
    public Action OnNicknameSetupRequired { get; set; }
        
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
        /// [설명]: 토큰 로그인 실패 시 호출되는 액션입니다(토큰 로그인 실패 시 다른 로그인 경로를 활성화하기 위함).
        /// </summary>
        public Action OnTokenLoginFailed { get; set; }

        /// <summary>
        /// [설명]: 생성자입니다.
        /// </summary>
    public LoginViewModel(IBackEndService backEndService, UserDataService userDataService, ISceneNavigationService navigationService)
    {
        m_backEndService = backEndService;
        m_userDataService = userDataService;
        m_navigationService = navigationService;
    }
    
    public async UniTask TryCreateNickname(string nickname)
    {
        IsLoading = true;
        try
        {
            var createRes = await m_backEndService.CreateNicknameAsync(nickname);
            if (createRes)
            {
                bool hasNick = await m_backEndService.HasNicknameAsync();
                if (hasNick)
                {
                    // 닉네임 생성 성공, 게임 진입 흐름 재개
                    try
                    {
                        await m_navigationService.LoadSceneAsync(2);
                    }
                    catch
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(2);
                    }
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnErrorMessage?.Invoke("닉네임 설정이 반영되지 않았습니다.");
                }
            }
            else
            {
                OnErrorMessage?.Invoke("닉네임 생성 실패");
            }
        }
        catch (Exception e)
        {
            OnErrorMessage?.Invoke("닉네임 생성 중 오류 발생: " + e.Message);
        }
        finally
        {
            IsLoading = false;
        }
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
                
                if ( result.IsSuccess )
                {
                    await m_userDataService.LoadAllUserDataAsync();
                    // 닉네임 여부 확인
                    bool hasNick = await m_backEndService.HasNicknameAsync();
                    if (!hasNick)
                    {
                        OnNicknameSetupRequired?.Invoke();
                        IsLoading = false;
                        return;
                    }
                    try
                    {
                        await m_navigationService.LoadSceneAsync(2);
                    }
                    catch
                    {
                        // Fallback: 강제 씬 전환
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(2);
                    }
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
                
                if ( result.IsSuccess )
                {
                    await m_userDataService.LoadAllUserDataAsync();
                    bool hasNick = await m_backEndService.HasNicknameAsync();
                    if (!hasNick)
                    {
                        OnNicknameSetupRequired?.Invoke();
                        IsLoading = false;
                        return;
                    }
                    try
                    {
                        await m_navigationService.LoadSceneAsync(2);
                    }
                    catch
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(2);
                    }
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnErrorMessage?.Invoke("토큰 로그인 실패: " + result.ErrorMessage);
                    OnTokenLoginFailed?.Invoke();
                }
            }
            catch (Exception e)
            {
                OnErrorMessage?.Invoke("토큰 로그인 중 오류 발생: " + e.Message);
                OnTokenLoginFailed?.Invoke();
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
                
                if ( result.IsSuccess )
                {
                    await m_userDataService.LoadAllUserDataAsync();
                    bool hasNick = await m_backEndService.HasNicknameAsync();
                    if (!hasNick)
                    {
                        OnNicknameSetupRequired?.Invoke();
                        IsLoading = false;
                        return;
                    }
                    try
                    {
                        await m_navigationService.LoadSceneAsync(2);
                    }
                    catch
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(2);
                    }
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
