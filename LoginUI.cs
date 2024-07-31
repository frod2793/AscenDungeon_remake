using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.Dispatcher;
using UnityEngine.SceneManagement;

public class LoginUI : Singleton<LoginUI>
{
    private static LoginUI instance;

    // 로그인 관련 오브젝트들
    public GameObject loginObject;
    public GameObject touchStart;
    public GameObject errorObject;
    public GameObject nicknameObject;
    public GameObject Scene;

    // UI 컴포넌트들
    private TMPro.TMP_InputField nicknameField;
    private Text errorText;
    private GameObject loadingObject;
 
    // Fade 애니메이션 관련 (주석 처리됨)
    // private FadeAnimation fadeObject;

    // 상수 정의
    private const byte ID_INDEX = 0;
    private const byte PW_INDEX = 1;
    private const string VERSION_STR = "Ver {0}";

    void Awake()
    {
        instance = this;
    }

    public static LoginUI GetInstance()
    {
        if (instance == null)
        {
            // 인스턴스가 존재하지 않을 때 에러 로그 출력
            Debug.LogError("LoginUI 인스턴스가 존재하지 않습니다.");
            return null;
        }
        return instance;
    }

    void Start()
    {
        // UI 요소 초기화
        if (errorObject != null)
        {
            errorText = errorObject.GetComponent<Text>();
        }
        
        // 로딩 오브젝트 초기화
        if (loadingObject != null)
        {
            loadingObject.SetActive(false);
        }

        // 닉네임 필드 초기화
        if (nicknameObject != null)
        {
            nicknameField = nicknameObject.GetComponent<TMPro.TMP_InputField>();
        }

        // 터치 시작 오브젝트 활성화
        if (touchStart != null)
        {
            touchStart.SetActive(true);
        }

        // 로그인 오브젝트 활성화
        if (loginObject != null)
        {
            loginObject.SetActive(true);
        }

        // 로그인 시도
        TouchStart();
    }

    public void TouchStart()
    {
        // 백엔드 서버에서 토큰 로그인 시도
        BackEndServerManager.GetInstance().BackendTokenLogin((bool result, string error) =>
        {
            Debug.Log("토큰 로그인 시도");

            if (result)
            {
                // 성공적으로 로그인된 경우 로비 씬 로드
                SceneLoader.Instance.SceneLoad(1);
                return;
            }
            else if (!string.IsNullOrEmpty(error))
            {
                // 에러가 있을 경우 에러 텍스트 표시
                if (errorText != null)
                {
                    errorText.text = "유저 정보 불러오기 실패\n\n" + error;
                }
                return;
            }
        });
    }

    public void GuestLogin()
    {
        // 게스트 로그인 시도
        BackEndServerManager.GetInstance().GuestLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    // 로그인 실패 시 에러 텍스트 표시
                    if (errorText != null)
                    {
                        errorText.text = "로그인 에러\n\n" + error;
                    }
                    return;
                }
                // 성공적으로 게스트 로그인된 경우 로비 씬 로드
                SceneLoader.Instance.SceneLoad(1);
            });
        });
    }

    public void GoogleFederation()
    {
        // 구글 연동 로그인 시도
        BackEndServerManager.GetInstance().GPGSLogin((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    // 로그인 실패 시 에러 텍스트 표시
                    if (errorText != null)
                    {
                        errorText.text = "로그인 에러\n\n" + error;
                    }
                    return;
                }
                // 성공적으로 구글 로그인된 경우 로비 씬 로드
                SceneLoader.Instance.SceneLoad(1);
            });
        });
    }

    // 씬 로드 메서드
    public void SceneLoad(int index)
    {
        SceneManager.LoadScene(index);
        if (index == 1)
        {
            // 로비 씬으로 이동 시 인벤토리 초기화
            Inventory.Instance.Clear();
        }
    } 
}
