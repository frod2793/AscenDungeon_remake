using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    #region Scene
    private const string LOGIN = "Login";
    private const string Town = "Town";
    private const string READY = "2. LoadRoom";
    private const string INGAME = "3. InGame";
    #endregion
    private string asyncSceneName = string.Empty;
    public enum GameState { Login,Town };
    private GameState gameState;
    // Start is called before the first frame update

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("GameManager 인스턴스가 존재하지 않습니다.");
            return null;
        }
        return instance;

        void Awake()
        {
            if (!instance)
            {
               // instance = this;
            }
           
            // 게임중 슬립모드 해제
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

           
        }
    }
        void Start()
    {
        
        gameState = GameState.Login;
       
    }
    public void ChangeState(GameState state, Action<bool> func = null)
    {
        gameState = state;
        switch (gameState)
        {
           
            case GameState.Town:
                MatchLobby(func);
                break;
            default:
                Debug.Log("알수없는 스테이트입니다. 확인해주세요.");
                break;
        }
    }
    private void MatchLobby(Action<bool> func)
    {
        if (func != null)
        {
            ChangeSceneAsync(Town, func);
        }
        else
        {
            ChangeScene(Town);
        }
    }

    private void ChangeSceneAsync(string scene, Action<bool> func)
    {
        asyncSceneName = string.Empty;
        if (scene != LOGIN &&  scene != Town )
        {
            Debug.Log("알수없는 씬 입니다.");
            return;
        }
        asyncSceneName = scene;

        StartCoroutine("LoadScene", func);
    }
    private void ChangeScene(string scene)
    {
        if (scene != LOGIN && scene != Town )
        {
            Debug.Log("알수없는 씬 입니다.");
            return;
        }

        SceneManager.LoadScene(scene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
