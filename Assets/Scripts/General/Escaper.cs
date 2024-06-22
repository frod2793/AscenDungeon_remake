using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escaper : MonoBehaviour
{
    private const string EscTryMessage = "뒤로가기를 두 번 눌러서 종료합니다";

    private const float EscIntervalTime = 2.0f;
    private float _EscInputLastTime;

    private void Awake()
    {
        if (FindObjectsOfType<Escaper>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            float time = Time.time;
            if (time <= _EscInputLastTime + EscIntervalTime)
            {
                Application.Quit();
            }
            else
            {
                _EscInputLastTime = time;
                ShowToastMessage(EscTryMessage);
            }
        }
    }
    public void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt("HighScore", score);
    }
    public void ShowToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
