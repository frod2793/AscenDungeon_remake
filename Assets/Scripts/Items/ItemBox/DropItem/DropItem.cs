using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : MonoBehaviour
{
    [SerializeField] private Animator Animator;
    [SerializeField] private SpriteRenderer Renderer;
   
    private bool _HasPlayer;
    private Item mContainItem;

    public void Init(Item containItem)
    {
                          mContainItem = containItem;
        Renderer.sprite = mContainItem?.Sprite;
    }

    public void Catch()
    {
        if (_HasPlayer)
        {
            int animControlKey = Animator.GetParameter(0).nameHash;

            if (!Animator.GetBool(animControlKey))
            {
                Animator.SetBool(animControlKey, true);
            }
        }
    }

    private void AnimationPlayOver()
    {
        gameObject.SetActive(false);

        Inventory.Instance.AddItem(mContainItem);
    }

    private void Reset()
    {
        Debug.Assert(TryGetComponent(out Animator));
        Debug.Assert(TryGetComponent(out Renderer));
    }

    private IEnumerator UpdateRoutine()
    {
        bool ClickCheck()
        {
            bool isClick = false;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    isClick = Input.GetMouseButtonDown(0);
                    break;

                case RuntimePlatform.Android:
                    isClick = Input.touchCount > 0;
                    break;
            }
            return isClick && _HasPlayer;
        }
        while (gameObject.activeSelf)
        {
            if (ClickCheck())
            {
                if (!EventSystem.current.IsPointerInUIObject())
                {
                    var origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var rayHit = Physics2D.RaycastAll(origin, Vector2.zero);

                    for (int i = 0; i < rayHit.Length; i++)
                    {
                        if (rayHit[i].collider.gameObject.Equals(gameObject))
                        {
                            Catch();
                            break;
                        }
                    }
                }
            } yield return null;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateRoutine());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) _HasPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) _HasPlayer = false;
    }
}
