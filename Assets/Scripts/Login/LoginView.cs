using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts.ViewModel;
using Assets.Scripts.Modules;
using Assets.Scripts.BackEnd;
using Cysharp.Threading.Tasks;

/// <summary>
/// [설명]: 로그인 화면의 UI 컨트롤 및 데이터 바인딩을 담당하는 뷰 클래스입니다.
/// MVVM 패턴을 따르며, LoginViewModel을 통해 백엔드 로직과 통신합니다.
/// </summary>
public class LoginView : MonoBehaviour
{
    #region 에디터 설정
    [SerializeField, Tooltip("에러 메시지 표시 오브젝트")]
    private GameObject m_errorObject;

    [SerializeField, Tooltip("닉네임 설정 UI 오브젝트")]
    private GameObject m_nicknameObject;

    [SerializeField, Tooltip("로그인 후 활성화할 씬 루트 오브젝트")]
    private GameObject m_sceneObject;

    [Header("로그인 버튼 설정")]
    [SerializeField, Tooltip("GPGS 로그인 버튼")]
    private Button m_gpgsLoginButton;//토큰로그인 실패시 활성화

    [SerializeField, Tooltip("게스트 로그인 버튼")]
    private Button m_guestLoginButton;//토큰로그인 실패시 활성화

    [SerializeField, Tooltip("토큰(자동) 로그인 버튼")]
    private Button m_tokenLoginButton;// 토큰로그인 실패시 비활성화 

    [Header("입력 및 텍스트")]
    [SerializeField, Tooltip("닉네임 입력 필드")]
    private TMPro.TMP_InputField m_nicknameField;

    [SerializeField, Tooltip("에러 메시지 출력 텍스트")]
    private Text m_errorText;

    [SerializeField, Tooltip("로딩 인디케이터 오브젝트")]
    private GameObject m_loadingObject;
    #endregion

    #region 내부 필드
    /// <summary>
    /// [설명]: 로그인 비즈니스 로직을 처리하는 뷰모델입니다.
    /// </summary>
    private LoginViewModel m_viewModel;
    #endregion

    #region 유니티 생명주기
    private void Start()
    {
        InitializeViewModel();
        BindButtonEvents();
        
        // 초기 상태 설정
        if (m_guestLoginButton != null)
        {
            m_guestLoginButton.gameObject.SetActive(false);
        }

        if (m_gpgsLoginButton != null)
        {
            m_gpgsLoginButton.gameObject.SetActive(false);
        }

        // 토큰 로그인 버튼은 처음에 활성화 상태로 둡니다.
        if (m_tokenLoginButton != null)
        {
            m_tokenLoginButton.gameObject.SetActive(true);
            m_tokenLoginButton.interactable = true;
        }

        // 안드로이드 환경에 따른 추가 처리 가능
#if UNITY_ANDROID
        // GPGS 관련 가시성 처리 등
#endif
        // [해결]: 시작 시 이미 자동 로그인이 실패했다면 버튼 활성화 (레이스 컨디션 방지)
        if (BackEndServerManager.Instance != null && BackEndServerManager.Instance.IsAutoLoginFailed)
        {
            EnableGuestAndGpgsLogin();
        }
    }
    #endregion

    #region 초기화 및 바인딩 로직
    /// <summary>
    /// [설명]: 뷰모델을 생성하고 필요한 이벤트를 바인딩합니다.
    /// </summary>
    private void Awake()
    {
        // 로그인 시도 전에 백엔드 자동 로그인 실패 이벤트 구독
        if (BackEndServerManager.Instance != null)
        {
            BackEndServerManager.Instance.OnAutoLoginFailed += EnableGuestAndGpgsLogin;
        }
    }

    private void InitializeViewModel()
    {
        // 의존성 주입
        var gpgsProvider = new GPGSAuthProvider();
#if UNITY_ANDROID
        gpgsProvider.Initialize(); // GPGS SDK 활성화
#endif
        var backEndService = new BackEndService(gpgsProvider);
        var navigationService = new SceneNavigationService();
        m_viewModel = new LoginViewModel(backEndService, new UserDataService(backEndService), navigationService);

        if (m_viewModel != null)
        {
            m_viewModel.OnErrorMessage = ShowErrorMessage;
            m_viewModel.OnLoginSuccess = OnLoginSuccess;
            m_viewModel.OnTokenLoginFailed = EnableGuestAndGpgsLogin;
            // 닉네임 설정 필요 시 UI 노출 핸들러 연결
            m_viewModel.OnNicknameSetupRequired = ShowNicknameUI;
        }
    }

    /// <summary>
    /// [설명]: UI 버튼 이벤트를 런타임에 바인딩합니다.
    /// 중복 바인딩을 방지하기 위해 Persistent Event 개수를 확인합니다.
    /// </summary>
    private void BindButtonEvents()
    {
        if (m_gpgsLoginButton != null && m_gpgsLoginButton.onClick.GetPersistentEventCount() == 0)
        {
            m_gpgsLoginButton.onClick.AddListener(OnGoogleFederationClicked);
        }

        if (m_guestLoginButton != null && m_guestLoginButton.onClick.GetPersistentEventCount() == 0)
        {
            m_guestLoginButton.onClick.AddListener(OnGuestLoginClicked);
        }

        if (m_tokenLoginButton != null && m_tokenLoginButton.onClick.GetPersistentEventCount() == 0)
        {
            m_tokenLoginButton.onClick.AddListener(OnTouchStartClicked);
        }
    }
    #endregion

    #region UI 이벤트 핸들러
    /// <summary>
    /// [설명]: 화면 터치 또는 자동 로그인 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnTouchStartClicked()
    {
        if (m_viewModel != null)
        {
            m_viewModel.TryTokenLogin().Forget();
        }
    }

    /// <summary>
    /// [설명]: 게스트 로그인 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnGuestLoginClicked()
    {
        if (m_viewModel != null)
        {
            m_viewModel.TryGuestLogin().Forget();
        }
    }

    /// <summary>
    /// [설명]: 구글 페더레이션 로그인 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnGoogleFederationClicked()
    {
        if (m_viewModel != null)
        {
            m_viewModel.TryGoogleLogin().Forget();
        }
    }
    #endregion

    #region 내부 로직
    /// <summary>
    /// [설명]: 로그인 성공 시 수행할 동작입니다.
    /// </summary>
    private void OnLoginSuccess()
    {
        // 씬 전환은 뷰모델/서비스로 위임되도록 변경되었으므로, 더 이상 뷰에서 직접 로드하지 않습니다.
        // 필요시 UI 업데이트나 성공 처리만 수행합니다.
    }

    /// <summary>
    /// [설명]: 에러 발생 시 UI에 메시지를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 에러 메시지</param>
    private void ShowErrorMessage(string message)
    {
        if (m_errorText != null)
        {
            m_errorText.text = message;
        }

        if (m_errorObject != null)
        {
            m_errorObject.SetActive(true);
        }
    }

    /// <summary>
    /// [설명]: 토큰 로그인 실패 시 다른 로그인 옵션들을 활성화합니다.
    /// </summary>
    private void EnableGuestAndGpgsLogin()
    {
        if (m_guestLoginButton != null)
        {
            m_guestLoginButton.gameObject.SetActive(true);
            m_guestLoginButton.interactable = true;
        }

        if (m_gpgsLoginButton != null)
        {
            m_gpgsLoginButton.gameObject.SetActive(true);
            m_gpgsLoginButton.interactable = true;
        }

        // 토큰 로그인 버튼은 실패 시 비활성화(숨김) 처리
        if (m_tokenLoginButton != null)
        {
            m_tokenLoginButton.gameObject.SetActive(false);
        }

        // Debug 확인 로그
        Debug.Log("[LoginView] EnableGuestAndGpgsLogin 호출: 게스트/GPGS 오브젝트 활성화, 토큰 로그인 오브젝트 비활성화");
    }

    private void OnDestroy()
    {
        if (BackEndServerManager.Instance != null)
        {
            BackEndServerManager.Instance.OnAutoLoginFailed -= EnableGuestAndGpgsLogin;
        }
    }

    private void ShowNicknameUI()
    {
        if (m_nicknameObject != null)
        {
            m_nicknameObject.SetActive(true);
        }
    }

    // 닉네임 입력 시도 핸들러
    public void OnNicknameSubmit()
    {
        if (m_viewModel != null && m_nicknameField != null)
        {
            string nick = m_nicknameField.text?.Trim();
            if (!string.IsNullOrEmpty(nick))
            {
                m_viewModel.TryCreateNickname(nick).Forget();
            }
        }
    }

    /// <summary>
    /// [설명]: 수동 씬 로드 처리가 필요한 경우 사용합니다.
    /// </summary>
    /// <param name="index">씬 인덱스</param>
    private void SceneLoad(int index)
    {
        SceneManager.LoadScene(index);

        if (index == 1)
        {
            // 로비 씬 진입 시 인벤토리 초기화
            if (Inventory.Instance != null)
            {
                Inventory.Instance.Clear();
            }
        }
    }
    #endregion
}
