using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// Include Backend
using BackEnd;
using LitJson;
using static BackEnd.SendQueue;
//  Include GPGS namespace
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;


public class BackEndServerManager : Singleton<BackEndServerManager>
{
    private static BackEndServerManager instance;   // 인스턴스
    public bool isLogin { get; private set; }   // 로그인 여부

    private string tempNickName;                        // 설정할 닉네임 (id와 동일)
    public string myNickName { get; private set; } = string.Empty;  // 로그인한 계정의 닉네임
    public string myIndate { get;  set; } = string.Empty; 
    public string mIndate { get;  set; } = string.Empty;
    public string InDate { get;  set; } = string.Empty;// 로그인한 계정의 inDate
    private Action<bool, string> loginSuccessFunc = null;

    public bool load = false;
 

    float x = 0;
    float Y = 0;
    bool IsInitialized = false;
    BackendReturnObject bro;
     public GameObject Scene;
     public GameObject Login;
    private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";
    // Start is called before the first frame update
    
    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
        // 모든 씬에서 유지
        DontDestroyOnLoad(this.gameObject);
    }

    public void Optionsaver()
    {
        Where where = new Where();
        Debug.Log("옵션값23");

        Param _Param = new Param();
        Debug.Log(_Param);

        where.Contains("gamerIndate", mIndate);
        _Param.Add("ControllerOffset", GameLoger.Instance.ControllerOffset);
        _Param.Add("ControllerDefScale", GameLoger.Instance.ControllerDefScale);
        _Param.Add("ControllerMaxScale", GameLoger.Instance.ControllerMaxScale);
        _Param.Add("ControllerAlpha", GameLoger.Instance.ControllerAlpha);
        _Param.Add("ControllerPosX", GameLoger.Instance.ControllerPos.x);
        _Param.Add("ControllerPosY", GameLoger.Instance.ControllerPos.y);

        Enqueue(Backend.GameSchemaInfo.Get, "Option", where, 1, (BackendReturnObject Getbro) =>
        {

            Debug.Log("옵션값");

            if (Getbro.IsSuccess())
            {
                Enqueue(Backend.GameSchemaInfo.Update, "Option", Getbro.GetReturnValuetoJSON()["rows"][0]["inDate"]["S"].ToString(), _Param, (BackendReturnObject Updatebro) =>
                {
                    if (Updatebro.IsSuccess())
                        Debug.Log("Update");
                    else
                        Debug.Log("falil update" + Updatebro);
                });
            }
            else
            {
                Enqueue(Backend.GameSchemaInfo.Insert, "Option", _Param, (BackendReturnObject Insertbro) =>
                {
                    if (Insertbro.IsSuccess())
                        Debug.Log("insert");
                    else
                        Debug.Log("falil insert" + Insertbro);
                });
            }
        });
    }

    public void SendDataToServerSchema()
    {
        Where where = new Where();
        Debug.Log("쿠르르");
       
        Param _Param = new Param();
        Debug.Log(_Param);

        where.Contains("gamerIndate", mIndate);
        _Param.Add("IAP", IAP.Instance.APP);
        _Param.Add("Gold", MoneyManager.Instance.Money);
        _Param.Add("Kill", GameLoger.Instance.KillCount);
        _Param.Add("Time", GameLoger.Instance.ElapsedTime.ToString());
       
        Enqueue(Backend.GameSchemaInfo.Get, "Player", where ,10, (BackendReturnObject Getbro) =>
        {
            
              Debug.Log("커넥트");

            if (Getbro.IsSuccess())
            {            
                Enqueue(Backend.GameSchemaInfo.Update, "Player", Getbro.GetReturnValuetoJSON()["rows"][0]["inDate"]["S"].ToString(), _Param, (BackendReturnObject Updatebro) =>
             {
              if(Updatebro.IsSuccess())
                 Debug.Log("Update");
                 else
                     Debug.Log("falil update"+ Updatebro);
             });
            }
            else
            {
               Enqueue(Backend.GameSchemaInfo.Insert, "Player", _Param, (BackendReturnObject Insertbro) =>
                {
                  if(Insertbro.IsSuccess())
                    Debug.Log("insert");
                  else
                     Debug.Log("falil insert"+ Insertbro);
                });
            }
        });
    }

    public void IAPSAVE()
    {
        Where where = new Where();
        Debug.Log("iap");

        Param _Param = new Param();
        Debug.Log(_Param);

        where.Contains("gamerIndate", mIndate);
        _Param.Add("IAP", IAP.Instance.APP);
       

        Enqueue(Backend.GameSchemaInfo.Get, "IAP", where, 1, (BackendReturnObject Getbro) =>
        {

            Debug.Log("iap커넥트");

            if (Getbro.IsSuccess())
            {
                Enqueue(Backend.GameSchemaInfo.Update, "IAP", Getbro.GetReturnValuetoJSON()["rows"][0]["inDate"]["BOOL"].ToString(), _Param, (BackendReturnObject Updatebro) =>
                {
                    if (Updatebro.IsSuccess())
                        Debug.Log("Update");
                    else
                        Debug.Log("falil update" + Updatebro);
                });
            }
            else
            {
                Enqueue(Backend.GameSchemaInfo.Insert, "IAP", _Param, (BackendReturnObject Insertbro) =>
                {
                    if (Insertbro.IsSuccess())
                        Debug.Log("insert");
                    else
                        Debug.Log("falil insert" + Insertbro);
                });
            }
        });
    }
    public static BackEndServerManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("BackEndServerManager 인스턴스가 존재하지 않습니다.");
            return null;
        }

        return instance;
    }

    void OnEnable()
    {

#if UNITY_ANDROID
        // ----- GPGS -----
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestEmail()
            .RequestIdToken()
            .Build();

        //커스텀된 정보로 GPGS 초기화
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        //GPGS 시작.
        PlayGamesPlatform.Activate();
#endif
        isLogin = false;
   
        // ----- 뒤끝 -----

        
        try
        {

            Backend.Initialize(() => // 비동기 
            {
                if (Backend.IsInitialized)
                {
                    IsInitialized = true;
                    SendQueue.StartSendQueue();
                   

                    // 구글 해시키 획득 
                    if (!string.IsNullOrEmpty(Backend.Utils.GetGoogleHash()))
                        Debug.Log(Backend.Utils.GetGoogleHash());

                    // 서버시간 획득
                   Debug.Log(Backend.Utils.GetServerTime());
                    // Application 버전 확인
                   

                }
                else
                {
                    // 초기화 실패한 경우 실행

                    
                    Debug.Log("초기화 실패 - ");
                }
            });

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        };



        

    }
    void Start()
    {
        Login.SetActive(false);
        Scene.SetActive(false);

        SceneManager.sceneUnloaded += scene =>
        {
            if (scene.buildIndex == (int)SceneIndex.Title)
            {
                bro = Backend.BMember.GetUserInfo();
                mIndate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
                if (bro.IsSuccess())
                {
                    Where param = new Where();
                    param.Contains("gamerIndate", mIndate);
                    Backend.GameSchemaInfo.Get("Player", param, 10, callback1 =>
                    {
                        if (callback1.IsSuccess())
                        {
                            Debug.Log(callback1.GetReturnValuetoJSON().ToJson());
                            Debug.Log(callback1.GetReturnValuetoJSON().ToJson());

                            GameLoger.Instance.RecordMoney(int.Parse(callback1.Rows()[0]["Gold"]["N"].ToString()));
                            Debug.Log("정보 불러오기 성공" + callback1);
                        }
                        else
                        {
                            Debug.Log("정보 불러오기 실패" + callback1);
                        }
                    });
                }
                else
                    Debug.Log(bro + "812");
            }
        };
    }
    void backendCallback(BackendReturnObject BRO)
    {
        //프로세싱 팝업 끄기
        

        // 초기화 성공한 경우 실행
        if (BRO.IsSuccess())
        {
            // 구글 해시키 획득 
            if (!string.IsNullOrEmpty(Backend.Utils.GetGoogleHash()))
                Debug.Log(Backend.Utils.GetGoogleHash());

            // 서버시간 획득
            Debug.Log(Backend.Utils.GetServerTime());
            // Application 버전 확인
          
        }
        // 초기화 실패한 경우 실행
        else
        {
          
            Debug.Log("초기화 실패 - " + BRO);
        }
    }
    void OnApplicationQuit()
    {


        Param param = new Param();
        param.Add("Gold", MoneyManager.Instance.Money);

        SendDataToServerSchema();
        Debug.Log("OnApplicationQuit");
        StopSendQueue();

    }

    void OnApplicationPause(bool isPause)
    {
        Debug.Log("OnApplicationPause : " + isPause);
        if (isPause == false)
        {
            ResumeSendQueue();
        }
        else
        {
            PauseSendQueue();
        }
    }

    
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Debug.Log("전");
    Enqueue(Backend.BMember.LoginWithTheBackendToken ,callback =>
        {
            if (callback.IsSuccess())
            {
                OnBackendAuthorized();
                //indate();
                loginSuccessFunc = func;
                Debug.Log("토큰 로그인 성공");
                load = true;

                Scene.SetActive(true);


            }
            else
            {
                Debug.Log("토큰 로그인 실패\n" + callback.ToString());
                func(false, string.Empty);


                Login.SetActive(true);
                //SceneLoader.Instance.SceneLoad(0);
            }
        });
    }


    public void OnStage()
    {
        bro = Backend.BMember.GetUserInfo();
        mIndate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        if (bro.IsSuccess())
        {
            Where param = new Where();
            param.Contains("gamerIndate", mIndate);
            Backend.GameInfo.GetPrivateContents("STAGE", (callback1) =>
            {
                if (callback1.IsSuccess())
                {

                    if (callback1.IsSuccess())
                    {
                      GameLoger.Instance.SetStageUnlock(Convert.ToInt32(callback1.Rows()[0]["stagedata"]["N"].ToString()));




                        Debug.Log("정보 불러오기 성공" + callback1);

                        load = true;

                    }
                    else
                    {

                        Debug.Log("정보 불러오기 실패" + callback1);
                       // SceneLoader.Instance.SceneLoad(1);
                    }
                }    
            });
        }
        else
            Debug.Log(bro + "812");
    }


    public void OnItem()
    {
        bro = Backend.BMember.GetUserInfo();
        mIndate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        if (bro.IsSuccess())
        {
            Where param = new Where();
            param.Contains("gamerIndate", mIndate);
            Backend.GameInfo.GetPrivateContents("ITem", (callback1) =>
            {
                if (callback1.IsSuccess())
                {
                    List<int> nowlist = new List<int>();

                    for (int i = 0; i < callback1.Rows()[0]["ItemList"]["L"].Count; i++)
                    {
                        nowlist.Add(Convert.ToInt32(callback1.Rows()[0]["ItemList"]["L"][i]["N"].ToString()));
                    }


                   ItemStateSaver.Instance.SetUnlockedItem(nowlist); 

                    Debug.Log("정보 불러오기 성공" + callback1);
              

                }
                else
                { 

                    Debug.Log("정보 불러오기 실패"+callback1);
                    //SceneLoader.Instance.SceneLoad(1);
                }
            });
        }
        else
            Debug.Log(bro + "812");
    }

    public void OnOption()
    {
        bro = Backend.BMember.GetUserInfo();
        mIndate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        if (bro.IsSuccess())
        {
            Where param = new Where();
            param.Contains("gamerIndate", mIndate);
            Backend.GameSchemaInfo.Get("Option", param,1, callback1 =>
            {
                if (callback1.IsSuccess())
                {
                    Debug.Log(callback1.GetReturnValuetoJSON().ToJson());
                    Debug.Log(callback1.GetReturnValuetoJSON().ToJson());

                    GameLoger.Instance.ConOffset(float.Parse(callback1.Rows()[0]["ControllerOffset"]["N"].ToString()));
                    GameLoger.Instance.ConDefScale(float.Parse(callback1.Rows()[0]["ControllerDefScale"]["N"].ToString()));
                    GameLoger.Instance.ConMaxScale(float.Parse(callback1.Rows()[0]["ControllerMaxScale"]["N"].ToString()));
                    GameLoger.Instance.ConAlpha(float.Parse(callback1.Rows()[0]["ControllerAlpha"]["N"].ToString()));
                    GameLoger.Instance.ConPosX(float.Parse(callback1.Rows()[0]["ControllerPosX"]["N"].ToString()));
                    GameLoger.Instance.ConPosY(float.Parse(callback1.Rows()[0]["ControllerPosY"]["N"].ToString()));
                  


                    Debug.Log("정보 불러오기 성공" + callback1);
                  
                    
                }
                else
                {

                    Debug.Log("정보 불러오기 실패" + callback1);
                   // SceneLoader.Instance.SceneLoad(1);
                }
            });
        }
        else
            Debug.Log(bro + "812");
    }
    public void IAPCOME()
    {
        bro = Backend.BMember.GetUserInfo();
        mIndate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        if (bro.IsSuccess())
        {
            Where param = new Where();
            param.Contains("gamerIndate", mIndate);
            Backend.GameSchemaInfo.Get("IAP", param, 1, callback1 =>
            {
                if (callback1.IsSuccess())
                {
                    Debug.Log(callback1.GetReturnValuetoJSON().ToJson());
                    Debug.Log(callback1.GetReturnValuetoJSON().ToJson());

                   
                    IAP.Instance.AiP(bool.Parse(callback1.Rows()[0]["IAP"]["BOOL"].ToString()));


                    Debug.Log("정보 불러오기 성공" + callback1);
           

                }
                else
                {

                    Debug.Log("정보 불러오기 실패" + callback1);
                 //   SceneLoader.Instance.SceneLoad(1);
                }
            });
        }
        else
            Debug.Log(bro + "812");
    }
    [System.Obsolete]
    public void OnLogined()
    {
        bro = Backend.BMember.GetUserInfo();
        mIndate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        if (bro.IsSuccess())
        {
            Where param = new Where();
            param.Contains("gamerIndate", mIndate);
            Backend.GameSchemaInfo.Get("Player", param, 10, callback1 =>
            {
                if (callback1.IsSuccess())
                {
                    Debug.Log(callback1.GetReturnValuetoJSON().ToJson());
                    Debug.Log(callback1.GetReturnValuetoJSON().ToJson());

                    GameLoger.Instance.RecordMoney(10000/*int.Parse(callback1.Rows()[0]["Gold"]["N"].ToString())*/);
                 


                    Debug.Log("정보 불러오기 성공"+callback1);

                    
                }
                else
                {
                   
                    Debug.Log("정보 불러오기 실패" + callback1);
                    //SceneLoader.Instance.SceneLoad(1);
                }
            });
        }
        else
            Debug.Log(bro + "812");
    }
    // 커스텀 로그인

#region 페더레이션 회원 가입 (GPGS)
    public void GPGSLogin(Action<bool, string> func) // 로그인으로 해야 될 경우, 동기 형식이 더 편하다
    {
#if UNITY_ANDROID
        // 이미 로그인 된 경우
        if (Social.localUser.authenticated == true)
        {
            bro = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");
            if (bro.IsSuccess())
            {
                // 성공시
                OnBackendAuthorized();

             
                indate();
                MessagePopManager.instance.ShowPop("GPGS 로그인 성공");
                Scene.SetActive(true);
                load = true;
                Login.SetActive(false);


            }
        }
        else
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    // 로그인 성공 -> 뒤끝 서버에 획득한 구글 토큰으로 가입요청
                    bro = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");

                    if (bro.IsSuccess())
                    {
                        // 성공시
                        OnBackendAuthorized();

                        
                        indate();
                        Debug.Log("GPGS 로그인 성공");
                        Scene.SetActive(true);
                        Login.SetActive(false);
                    }
                }
                else
                {
                    // 로그인 실패
                    Debug.Log("Login failed for some reason");
                    Debug.LogError("GPGS 로그인 실패");
                    MessagePopManager.instance.ShowPop("[X]GPGS 로그인 실패", 10f);
                }
            });
        }
#endif
    }

    // 구글 토큰 받아옴
    public string GetTokens() // 딱히 수정할 필요 없음
    {
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            // 유저 토큰 받기 첫번째 방법
            string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
            // 두번째 방법
            // string _IDtoken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
            return _IDtoken;
        }
        else
        {
            MessagePopManager.instance.ShowPop("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail", 10f);
            Debug.Log("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
        }
#endif
        return null;
    }
     #endregion


  
 

    private void OnBackendAuthorized()
    {
        // OnLogined();
        IAPCOME();
        OnItem();
        OnStage();
        OnOption();
       

    }
    void Update()
    {
        SendQueue.Poll();
        
    }

    public void GuestLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.GuestLogin, callback =>
        {
            if (callback.IsSuccess())
            {
                OnBackendAuthorized();


                indate();
                Debug.Log("게스트 로그인 성공");
                Scene.SetActive(true);
                Login.SetActive(false);
                return;
            }

            Debug.Log("게스트 로그인 실패\n" + callback);
           
        });
    }
   

    public void indate()
    {
        Tplay();
        Titem();
        Tstage();
        Toption();
        Tiap();
    }
    public void Tplay()
    {
        bro = Backend.GameSchemaInfo.Insert("Player");
        Param param1 = new Param();
        param1.Add("gamerIndate", mIndate);
        Backend.GameSchemaInfo.Update("Player", bro.GetInDate(), param1);
    }
     public void Titem()
    {
        bro = Backend.GameSchemaInfo.Insert("ITem");
        Param param1 = new Param();
        param1.Add("gamerIndate", mIndate);
        Backend.GameSchemaInfo.Update("ITem", bro.GetInDate(), param1);
    }

    public void Tstage()
    {
        bro = Backend.GameSchemaInfo.Insert("STAGE");
        Param param1 = new Param();
        param1.Add("gamerIndate", mIndate);
        Backend.GameSchemaInfo.Update("STAGE", bro.GetInDate(), param1);
    }

    public void Toption()
    {
        bro = Backend.GameSchemaInfo.Insert("Option");
        Param param1 = new Param();
        param1.Add("gamerIndate", mIndate);
        Backend.GameSchemaInfo.Update("Option", bro.GetInDate(), param1);
    }
    // Update is called once per frame
    public void Tiap()
    {
        bro = Backend.GameSchemaInfo.Insert("IAP");
        Param param1 = new Param();
        param1.Add("gamerIndate", mIndate);
        Backend.GameSchemaInfo.Update("IAP", bro.GetInDate(), param1);
    }
}
