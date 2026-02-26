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
            
            // [변경]: 던전 실패 시 광고를 제거하고 바로 마을로 씬을 전환합니다.
            SceneManager.LoadScene((int)SceneIndex.Town);
        });
    }
}
