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
        #region 내부 변수
        private readonly IBackEndService m_backEndService;
        private readonly UserDataService m_userDataService;
        private readonly ISceneNavigationService m_navigationService;
        #endregion

        #region 프로퍼티
        /// <summary>
        /// [설명]: 로딩 상태를 나타냅니다.
        /// </summary>
        public bool IsLoading { get; private set; }
        #endregion

        #region 액션/이벤트
        /// <summary>
        /// [설명]: 에러 메시지를 처리하는 액션입니다.
        /// </summary>
        public Action<string> OnErrorMessage { get; set; }
        
        /// <summary>
        /// [설명]: 로그인 성공 시 호출되는 액션입니다.
        /// </summary>
        public Action OnLoginSuccess { get; set; }

        /// <summary>
        /// [설명]: 토큰 로그인 실패 시 호출되는 액션입니다.
        /// </summary>
        public Action OnTokenLoginFailed { get; set; }

        /// <summary>
        /// [설명]: 닉네임 설정이 필요할 때 호출되는 액션입니다.
        /// </summary>
        public Action OnNicknameSetupRequired { get; set; }
        #endregion

        #region 초기화
        /// <summary>
        /// [설명]: 생성자입니다.
        /// </summary>
        public LoginViewModel(IBackEndService backEndService, UserDataService userDataService, ISceneNavigationService navigationService)
        {
            m_backEndService = backEndService;
            m_userDataService = userDataService;
            m_navigationService = navigationService;
        }
        #endregion

        #region 공개 API
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
                    // [정책]: 게스트 로그인은 최초 로그인 가능성을 염두에 두고 닉네임을 체크합니다.
                    await HandleLoginPostProcess(true);
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
                    // [정책]: 토큰 로그인은 기존 사용자로 간주하여 닉네임 체크를 스킵합니다.
                    await ProceedToGame();
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
                if (result.IsSuccess)
                {
                    // [정책]: 구글 로그인도 닉네임 유무를 확인합니다.
                    await HandleLoginPostProcess(true);
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

        /// <summary>
        /// [설명]: 닉네임 생성을 시도합니다.
        /// </summary>
        /// <param name="nickname">사용자가 입력한 닉네임</param>
        public async UniTask TryCreateNickname(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                OnErrorMessage?.Invoke("닉네임을 입력해 주세요.");
                return;
            }

            IsLoading = true;
            try
            {
                bool success = await m_backEndService.CreateNicknameAsync(nickname);
                if (success)
                {
                    // 닉네임 설정 성공 시 환영 업적 해제 시도
                    Debug.Log($"[Login] 닉네임 생성 성공. 환영 업적 해제 시도: {GPGSIds.achievement____}");
                    // [수정]: 전역 네임스페이스의 GPGSIds를 사용합니다.
                    var welcome = await m_backEndService.UnlockAchievement(GPGSIds.achievement____);
                    var hello = await m_backEndService.UnlockAchievement(GPGSIds.achievement____);
                    await ProceedToGame();
                }
                else
                {
                    OnErrorMessage?.Invoke("닉네임 생성 실패 (중복 또는 부적절한 이름)");
                }
            }
            catch (Exception e)
            {
                OnErrorMessage?.Invoke("닉네임 설정 중 오류 발생: " + e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region 내부 로직
        /// <summary>
        /// [설명]: 로그인 성공 후속 처리를 담당합니다.
        /// </summary>
        /// <param name="checkNickname">닉네임 체크 여부</param>
        private async UniTask HandleLoginPostProcess(bool checkNickname)
        {
            if (checkNickname)
            {
                bool hasNick = await m_backEndService.HasNicknameAsync();
                if (!hasNick)
                {
                    OnNicknameSetupRequired?.Invoke();
                    return;
                }
            }

            await ProceedToGame();
        }

        /// <summary>
        /// [설명]: 모든 데이터를 로드하고 인게임 씬으로 이동합니다.
        /// 스테이지 진행도에 따라 튜토리얼 또는 타이틀 씬으로 분기합니다.
        /// </summary>
        private async UniTask ProceedToGame()
        {
            // 1. 서버 데이터 로드
            Debug.Log("[Login] ProceedToGame: 모든 사용자 데이터 로드 시작...");
            await m_userDataService.LoadAllUserDataAsync();
            Debug.Log("[Login] ProceedToGame: 데이터 로드 완료.");
            
            // 2. 환영 업적 해제 시도 (백엔드 서비스 내부에서 중복 체크 등 수행)
            Debug.Log($"[Login] ProceedToGame: 업적 해제 시도 ID = {GPGSIds.achievement____}");
            var result = await m_backEndService.UnlockAchievement(GPGSIds.achievement____);
            Debug.Log($"[Login] ProceedToGame: 업적 해제 시도 결과 = {result.IsSuccess}");

            // 3. 로그인 후 기본 이동 씬 설정 (항상 Town 씬으로 이동)
            // [수정]: 타이틀을 거치지 않고 바로 마을(Town)로 진입하도록 요구사항 반영됨.
            int targetSceneIndex = (int)SceneIndex.Town; // 2

            Debug.Log($"[Login] ProceedToGame - User logged in. Moving to Town scene ({targetSceneIndex}).");

            try
            {
                await m_navigationService.LoadSceneAsync(targetSceneIndex);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Login] NavigationService error: {ex.Message}. Using fallback SceneManager.");
                SceneManager.LoadScene(targetSceneIndex);
            }

            OnLoginSuccess?.Invoke();
        }
        #endregion
    }
}
