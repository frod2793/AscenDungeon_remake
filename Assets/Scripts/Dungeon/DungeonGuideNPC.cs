using UnityEngine;

public class DungeonGuideNPC : MonoBehaviour
{
    [SerializeField] private AttackButtonHider _Hider;
    [SerializeField] private bool IsWaitForEffectDisable;

    [SerializeField] private GameObject DungeonSelectWindow;

    private bool mHasPlayer;

    private void Awake()
    {
        mHasPlayer = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // _Hider.HideOrShow();
            mHasPlayer = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // _Hider.HideOrShow();
            mHasPlayer = false;
        }
    }

    public void Interact()
    {
        if (!mHasPlayer)
        {
            SystemMessage.Instance.ShowMessage("NPC와의 거리가\n너무 멉니다!");
        }
        else if (IsWaitForEffectDisable)
        {
            EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position, () => 
            {
                DungeonSelectWindow.SetActive(!DungeonSelectWindow.activeSelf);
            });
        }
        else
        {
            EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position);

            DungeonSelectWindow.SetActive(!DungeonSelectWindow.activeSelf);
        }
    }
}
