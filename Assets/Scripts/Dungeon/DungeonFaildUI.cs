using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.Ad;

public class DungeonFaildUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI KillCount;
    [SerializeField] private TMPro.TextMeshProUGUI ClearTime;

    private void Awake()
    {
        int clearSec = Mathf.FloorToInt(GameLoger.Instance.ElapsedTime % 60f);
        int clearMin = Mathf.FloorToInt(GameLoger.Instance.ElapsedTime / 60f);

        ClearTime.text = $"{clearMin:D2} : {clearSec:D2}";
        KillCount.text = $"{GameLoger.Instance.KillCount:D3} 마리";

        SoundManager.Instance.PlaySound(SoundName.DungeonResult);
    }

    public void ReTry()
    {
        gameObject.SetActive(false);
        MainCamera.Instance.Fade(2.25f, FadeType.In, () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    public void BackToTown()
    {
        gameObject.SetActive(false);
        MainCamera.Instance.Fade(2.25f, FadeType.In, () =>
        {
            Inventory.Instance.Clear();
            
            // [안전 장치]: 광고 서비스 미초기화 시 광고 생략 후 바로 씬 전환
            if (AdsManager.Service != null)
            {
                AdsManager.Service.ShowInterstitialAd(() =>
                {
                    SceneManager.LoadScene((int)SceneIndex.Town);
                });
            }
            else
            {
                Debug.LogWarning("[DungeonFaildUI] 광고 서비스가 준비되지 않아 광고 없이 진행합니다.");
                SceneManager.LoadScene((int)SceneIndex.Town);
            }
        });
    }
}
