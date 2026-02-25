using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Assets.Scripts.Ad;

public class DungeonSelection : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private DungeonSelectionInfo _SelectionInfo;

    [Header("Selection Parameter")]
    [SerializeField] private Image _Image;
    [SerializeField] private TMPro.TextMeshProUGUI _Title;
    [SerializeField] private TMPro.TextMeshProUGUI _Comment;

    public event Action<DungeonSelection> SelectedEvent;
    public string Name => _SelectionInfo.AttachedSceneName;

    public void Init(bool isLocked)
    {
        if (isLocked)
        {
            _Image.sprite = _SelectionInfo.LockedSprite;
            _Title.text   = _SelectionInfo.LockedTitle;
            _Comment.text = _SelectionInfo.LockedComment;
        }
        else
        {
            _Image.sprite = _SelectionInfo.UnLockedSprite;
            _Title.text   = _SelectionInfo.UnLockedTitle;
            _Comment.text = _SelectionInfo.UnLockedComment;
        }
    }

    public void DungeonEnter()
    {
        if (Inventory.Instance.IsEquipWeapon())
        {
            MainCamera.Instance.Fade(1.75f, FadeType.In, () =>
            {
                // [안전 장치]: 광고 서비스 미초기화 시 광고 생략 후 바로 씬 전환
                if (AdsManager.Service != null)
                {
                    AdsManager.Service.ShowInterstitialAd(() =>
                    {
                        SceneManager.LoadScene(_SelectionInfo.AttachedSceneName);
                    });
                }
                else
                {
                    Debug.LogWarning("[DungeonSelection] 광고 서비스가 준비되지 않아 광고 없이 진행합니다.");
                    SceneManager.LoadScene(_SelectionInfo.AttachedSceneName);
                }
            });
        }
        else
        {
            SystemMessage.Instance.ShowMessage("무기없이 입장할 수\n없습니다!");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SelectedEvent?.Invoke(this);
    }
}