using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private ItemBoxSprite ItemBoxSprite;

    [Space()]
    [SerializeField] private DropItem DropItem;
    [SerializeField] private Animator Animator;
    [SerializeField] private SpriteRenderer Renderer;

    public void Init(Item containItem, ItemBoxSprite boxSprite)
    {
        ItemBoxSprite = boxSprite;
        Renderer.sprite = boxSprite.ClosedSprite;

        DropItem.Init(containItem);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {

            Animator.enabled = true;
        }
    }

    private void ChestOpen()
    {
        Renderer.sprite = ItemBoxSprite.OpenSprite;

        Vector2 twinklePoint = transform.position + new Vector3(0f, 0.4f, 0f);
        EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, twinklePoint);

        SoundManager.Instance.PlaySound(SoundName.ItemBoxOpen);
    }
}
