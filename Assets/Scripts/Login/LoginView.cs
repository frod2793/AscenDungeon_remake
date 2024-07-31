using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts.ViewModel;
using Assets.Scripts.BackEnd;

public class LoginView : MonoBehaviour
{
    // 로그인 관련 오브젝트들
    public GameObject loginObject;
    public GameObject touchStart;
    public GameObject errorObject;
    public GameObject nicknameObject;
    public GameObject Scene;
    
    private TMPro.TMP_InputField nicknameField;
    private Text errorText;
    private GameObject loadingObject;
 
    // ViewModel
    private LoginViewModel m_viewModel;

    void Start()
    {
        // ViewModel 초기화
        // Note: In a real DI system, this would be injected
        m_viewModel = new LoginViewModel(new BackEndService());
        m_viewModel.OnErrorMessage = ShowErrorMessage;
        m_viewModel.OnLoginSuccess = OnLoginSuccess;
        
        // Google 로그인 버튼 활성화 (안드로이드 전용)
        // var google = loginObject.transform.GetChild(0).gameObject;

#if UNITY_ANDROID
        //  google.SetActive(true);
#endif
    }

    /// <summary>
    /// [설명]: 로그인 성공 시 호출됩니다.
    /// </summary>
    private void OnLoginSuccess()
    {
        // 로비 씬 로드
        SceneLoader.Instance.SceneLoad(1);
    }

    /// <summary>
    /// [설명]: 에러 메시지를 표시합니다.
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
        }
    }

    /// <summary>
    /// [설명]: 터치 시작 시 호출됩니다.
    /// </summary>
    public void TouchStart()
    {
        // 백엔드 서버에서 토큰 로그인 시도
        m_viewModel.TryTokenLogin().Forget();
    }

    /// <summary>
    /// [설명]: 게스트 로그인을 시도합니다.
    /// </summary>
    public void GuestLogin()
    {
        // 게스트 로그인 시도
        m_viewModel.TryGuestLogin().Forget();
    }

    /// <summary>
    /// [설명]: 구글 연동 로그인을 시도합니다.
    /// </summary>
    public void GoogleFederation()
    {
        // 구글 연동 로그인 시도
        m_viewModel.TryGoogleLogin().Forget();
    }

    // 씬 로드 메서드
    void SceneLoad(int index)
    {
        SceneManager.LoadScene(index);

        if (index == 1)
        {
            // 로비 씬으로 이동 시 인벤토리 초기화
            Inventory.Instance.Clear();
        }
    }
}
