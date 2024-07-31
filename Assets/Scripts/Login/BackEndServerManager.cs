using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BackEnd;
using LitJson;
using static BackEnd.SendQueue;
using UnityEngine.SocialPlatforms;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// [설명]: 뒤끝 서버와의 통신 및 데이터 관리를 담당하는 매니저 클래스입니다.
/// 최신 Backend SDK(5.11.1)의 GameData API를 사용하며, UniTask를 통한 비동기 처리를 지원합니다.
/// </summary>
public class BackEndServerManager : MonoBehaviour
{
    #region 에디터 설정
    [SerializeField, Tooltip("로그인 후 활성화할 씬 오브젝트")]
    private GameObject m_sceneObject;

    [SerializeField, Tooltip("로그인 UI 오브젝트")]
    private GameObject m_loginObject;
    #endregion

    #region 내부 필드
    private static BackEndServerManager s_instance;

    private bool m_isLogin = false;
    private string m_myNickName = string.Empty;
    private string m_myIndate = string.Empty; 
    private bool m_isInitialized = false;
    private bool m_isLoadFinished = false;

    private const string k_BackendErrorFormat = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";
    #endregion

    #region 프로퍼티
    /// <summary>
    /// [설명]: BackEndServerManager의 싱글톤 인스턴스입니다.
    /// </summary>
    public static BackEndServerManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<BackEndServerManager>();
                if (s_instance == null)
                {
                    GameObject obj = new GameObject(typeof(BackEndServerManager).Name);
                    s_instance = obj.AddComponent<BackEndServerManager>();
                }
            }
            return s_instance;
        }
    }

    public bool IsLogin => m_isLogin;
    public string MyNickName => m_myNickName;
    public string MyInDate { get => m_myIndate; set => m_myIndate = value; }
    public bool IsLoadFinished => m_isLoadFinished;
    
    // 기존 코드와의 호환성을 위한 프로퍼티
    public string mIndate { get => m_myIndate; set => m_myIndate = value; }
    public bool load { get => m_isLoadFinished; set => m_isLoadFinished = value; }
    public GameObject Scene { get => m_sceneObject; set => m_sceneObject = value; }
    public GameObject Login { get => m_loginObject; set => m_loginObject = value; }
    #endregion

    #region 유니티 생명주기
    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        InitializeBackend().Forget();
    }

    private void Start()
    {
        if (m_loginObject != null) m_loginObject.SetActive(false);
        if (m_sceneObject != null) m_sceneObject.SetActive(false);

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Update()
    {
        SendQueue.Poll();
    }

    private void OnApplicationPause(bool isPause)
    {
        if (!isPause)
        {
            ResumeSendQueue();
        }
        else
        {
            PauseSendQueue();
        }
    }

    private void OnApplicationQuit()
    {
        SendDataToServerSchema();
        StopSendQueue();
    }
    #endregion

    #region 초기화 및 바인딩 로직
    /// <summary>
    /// [설명]: 뒤끝 SDK를 초기화합니다.
    /// </summary>
    private async UniTaskVoid InitializeBackend()
    {
        try
        {
            // [해결]: 인자 없는 Initialize() 호출로 전환하여 타입 추론 에러(CS1660) 원천 봉쇄
            Backend.Initialize();

            // 초기화 완료까지 대기
            await UniTask.WaitUntil(() => Backend.IsInitialized);
            
            OnBackendInitialize();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Backend] 초기화 중 예외 발생: {e.Message}");
        }
    }

    /// <summary>
    /// [설명]: 뒤끝 초기화 콜백 메서드입니다.
    /// </summary>
    private void OnBackendInitialize()
    {
        if (Backend.IsInitialized)
        {
            m_isInitialized = true;
            SendQueue.StartSendQueue();
            Debug.Log($"[Backend] 초기화 성공. 서버 시간: {Backend.Utils.GetServerTime()}");

            // 초기화 성공 시 토큰 로그인 시도 (자동 로그인)
            BackendTokenLogin((success, error) => 
            {
                if (success)
                {
                    Debug.Log("[Backend] 자동 토큰 로그인 성공");
                }
                else
                {
                    Debug.Log("[Backend] 자동 토큰 로그인 실패, 로그인 UI 활성화");
                    if (m_loginObject != null) m_loginObject.SetActive(true);
                }
            });
        }
        else
        {
            Debug.LogError("[Backend] 초기화 실패");
        }
    }
    #endregion

    #region 공개 API
    /// <summary>
    /// [설명]: 기존 코드 호환을 위한 GetInstance 메서드입니다.
    /// </summary>
    public static BackEndServerManager GetInstance()
    {
        return Instance;
    }

    /// <summary>
    /// [설명]: 게스트 로그인을 시도합니다.
    /// </summary>
    public void GuestLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.GuestLogin, callback =>
        {
            if (callback.IsSuccess())
            {
                OnBackendAuthorized();
                Debug.Log("게스트 로그인 성공");
                
                if (m_sceneObject != null) m_sceneObject.SetActive(true);
                if (m_loginObject != null) m_loginObject.SetActive(false);
                
                func?.Invoke(true, string.Empty);
                return;
            }

            Debug.LogError($"게스트 로그인 실패: {callback}");
            func?.Invoke(false, callback.ToString());
        });
    }

    /// <summary>
    /// [설명]: 뒤끝 토큰으로 로그인을 시도합니다.
    /// </summary>
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
        {
            if (callback.IsSuccess())
            {
                OnBackendAuthorized();
                m_isLoadFinished = true;
                
                if (m_sceneObject != null) m_sceneObject.SetActive(true);
                Debug.Log("토큰 로그인 성공");
                
                func?.Invoke(true, string.Empty);
            }
            else
            {
                Debug.LogWarning($"토큰 로그인 실패: {callback}");
                if (m_loginObject != null) m_loginObject.SetActive(true);
                func?.Invoke(false, callback.ToString());
            }
        });
    }

    /// <summary>
    /// [설명]: GPGS 로그인을 시도합니다. (Android 전용 호환 로직)
    /// </summary>
    public void GPGSLogin(Action<bool, string> func)
    {
#if UNITY_ANDROID
        if (Social.localUser.authenticated)
        {
            string token = GetTokens();
            Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs", callback =>
            {
                if (callback.IsSuccess())
                {
                    OnBackendAuthorized();
                    if (m_sceneObject != null) m_sceneObject.SetActive(true);
                    if (m_loginObject != null) m_loginObject.SetActive(false);
                    func?.Invoke(true, string.Empty);
                }
                else
                {
                    func?.Invoke(false, callback.ToString());
                }
            });
        }
        else
        {
            Social.localUser.Authenticate(success =>
            {
                if (success)
                {
                    string token = GetTokens();
                    Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            OnBackendAuthorized();
                            if (m_sceneObject != null) m_sceneObject.SetActive(true);
                            if (m_loginObject != null) m_loginObject.SetActive(false);
                            func?.Invoke(true, string.Empty);
                        }
                        else
                        {
                            func?.Invoke(false, callback.ToString());
                        }
                    });
                }
                else
                {
                    func?.Invoke(false, "GPGS Authentication Failed");
                }
            });
        }
#endif
    }

    public string GetTokens()
    {
        // GPGS 토큰 획득 로직 (기존 코드 참고)
        return null; 
    }

    /// <summary>
    /// [설명]: 서버에 플레이어 데이터를 저장합니다.
    /// </summary>
    public void SendDataToServerSchema()
    {
        Param param = new Param();
        param.Add("IAP", IAP.Instance.APP);
        param.Add("Gold", MoneyManager.Instance.Money);
        param.Add("Kill", GameLoger.Instance.KillCount);
        param.Add("Time", GameLoger.Instance.ElapsedTime.ToString());

        Backend.GameData.GetMyData("Player", new Where(), 1, callback =>
        {
            if (callback.IsSuccess() && callback.Rows().Count > 0)
            {
                string rowIndate = callback.Rows()[0]["inDate"]["S"].ToString();
                m_myIndate = rowIndate;
                Backend.GameData.UpdateV2("Player", rowIndate, Backend.UserInDate, param);
            }
            else
            {
                Backend.GameData.Insert("Player", param, insertCallback =>
                {
                    if (insertCallback.IsSuccess()) m_myIndate = insertCallback.GetInDate();
                });
            }
        });
    }

    /// <summary>
    /// [설명]: IAP 구매 정보를 저장합니다. (기존 IAP.cs 호환)
    /// </summary>
    public void IAPSAVE()
    {
        Param param = new Param();
        param.Add("IAP", IAP.Instance.APP);

        Backend.GameData.GetMyData("IAP", new Where(), 1, callback =>
        {
            if (callback.IsSuccess() && callback.Rows().Count > 0)
            {
                string rowIndate = callback.Rows()[0]["inDate"]["S"].ToString();
                Backend.GameData.UpdateV2("IAP", rowIndate, Backend.UserInDate, param);
            }
            else
            {
                Backend.GameData.Insert("IAP", param);
            }
        });
    }
    #endregion

    #region 비즈니스 로직
    private void OnBackendAuthorized()
    {
        m_isLogin = true;
        // Note: Data loading is now handled by UserDataService
        // FetchAllUserData().Forget();
    }

    // Note: This method is now removed as data loading is handled by UserDataService
    // private async UniTaskVoid FetchAllUserData()
    // {
    //     await UniTask.Yield();
    //
    //     IAPCOME();
    //     OnItem();
    //     OnStage();
    //     OnOption();
    //     InitializeTables();
    // }

    private void InitializeTables()
    {
        CheckAndInsert("Player");
        CheckAndInsert("ITem");
        CheckAndInsert("STAGE");
        CheckAndInsert("Option");
        CheckAndInsert("IAP");
    }

    private void CheckAndInsert(string tableName)
    {
        Backend.GameData.GetMyData(tableName, new Where(), 1, callback =>
        {
            if (callback.IsSuccess() && callback.Rows().Count == 0)
            {
                Backend.GameData.Insert(tableName, new Param(), insertCb =>
                {
                    if (tableName == "Player" && insertCb.IsSuccess()) m_myIndate = insertCb.GetInDate();
                });
            }
            else if (tableName == "Player" && callback.IsSuccess() && callback.Rows().Count > 0)
            {
                m_myIndate = callback.Rows()[0]["inDate"]["S"].ToString();
            }
        });
    }

    // Note: These methods are now removed as they're handled by UserDataService
    // public void OnStage()
    // {
    //     Backend.GameData.GetMyData("STAGE", new Where(), callback =>
    //     {
    //         if (callback.IsSuccess() && callback.Rows().Count > 0)
    //         {
    //             var row = callback.Rows()[0];
    //             if (row.ContainsKey("stagedata"))
    //             {
    //                 GameLoger.Instance.SetStageUnlock(Convert.ToInt32(row["stagedata"]["N"].ToString()));
    //             }
    //             m_isLoadFinished = true;
    //         }
    //     });
    // }
    //
    // public void OnItem()
    // {
    //     Backend.GameData.GetMyData("ITem", new Where(), callback =>
    //     {
    //         if (callback.IsSuccess() && callback.Rows().Count > 0)
    //         {
    //             List<int> nowlist = new List<int>();
    //             var row = callback.Rows()[0];
    //             if (row.ContainsKey("ItemList") && row["ItemList"].ContainsKey("L"))
    //             {
    //                 var list = row["ItemList"]["L"];
    //                 for (int i = 0; i < list.Count; i++)
    //                 {
    //                     nowlist.Add(Convert.ToInt32(list[i]["N"].ToString()));
    //                 }
    //             }
    //             ItemStateSaver.Instance.SetUnlockedItem(nowlist);
    //         }
    //     });
    // }
    //
    // public void OnOption()
    // {
    //     Backend.GameData.GetMyData("Option", new Where(), 1, callback =>
    //     {
    //         if (callback.IsSuccess() && callback.Rows().Count > 0)
    //         {
    //             var row = callback.Rows()[0];
    //             if (row.ContainsKey("ControllerOffset")) GameLoger.Instance.ConOffset(float.Parse(row["ControllerOffset"]["N"].ToString()));
    //             if (row.ContainsKey("ControllerDefScale")) GameLoger.Instance.ConDefScale(float.Parse(row["ControllerDefScale"]["N"].ToString()));
    //             if (row.ContainsKey("ControllerMaxScale")) GameLoger.Instance.ConMaxScale(float.Parse(row["ControllerMaxScale"]["N"].ToString()));
    //             if (row.ContainsKey("ControllerAlpha")) GameLoger.Instance.ConAlpha(float.Parse(row["ControllerAlpha"]["N"].ToString()));
    //             if (row.ContainsKey("ControllerPosX")) GameLoger.Instance.ConPosX(float.Parse(row["ControllerPosX"]["N"].ToString()));
    //             if (row.ContainsKey("ControllerPosY")) GameLoger.Instance.ConPosY(float.Parse(row["ControllerPosY"]["N"].ToString()));
    //         }
    //     });
    // }
    //
    // public void IAPCOME()
    // {
    //     Backend.GameData.GetMyData("IAP", new Where(), 1, callback =>
    //     {
    //         if (callback.IsSuccess() && callback.Rows().Count > 0)
    //         {
    //             var row = callback.Rows()[0];
    //             if (row.ContainsKey("IAP"))
    //             {
    //                 IAP.Instance.AiP(bool.Parse(row["IAP"]["BOOL"].ToString()));
    //             }
    //         }
    //     });
    // }

    public void Tplay() => CheckAndInsert("Player");
    public void Titem() => CheckAndInsert("ITem");
    public void Tstage() => CheckAndInsert("STAGE");
    public void Toption() => CheckAndInsert("Option");
    public void Tiap() => CheckAndInsert("IAP");
    #endregion

    #region 내부 로직
    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.buildIndex == (int)SceneIndex.Title)
        {
            var userInfo = Backend.BMember.GetUserInfo();
            if (userInfo.IsSuccess())
            {
                m_myIndate = userInfo.GetReturnValuetoJSON()["row"]["inDate"].ToString();
                
                Backend.GameData.GetMyData("Player", new Where(), 10, callback =>
                {
                    if (callback.IsSuccess() && callback.Rows().Count > 0)
                    {
                        var row = callback.Rows()[0];
                        if (row.ContainsKey("Gold"))
                        {
                            GameLoger.Instance.RecordMoney(int.Parse(row["Gold"]["N"].ToString()));
                        }
                        m_myIndate = row["inDate"]["S"].ToString();
                    }
                });
            }
        }
    }
    #endregion
}
