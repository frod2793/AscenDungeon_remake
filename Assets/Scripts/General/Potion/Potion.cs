using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PotionName
{
    LHealingPotion,
    MHealingPotion,
    SHealingPotion,
}
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Potion : MonoBehaviour
{
    public PotionName Name => _Name;

    [SerializeField] protected PotionName _Name;
    protected event Action<Collider2D> OnTriggerEnterEvnt;
    protected event Action<Collider2D> OnTriggerExitEvnt;

    protected bool _TriggerEntryPlayer;

    public abstract void UsePotion();
    public abstract void Interaction();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UsePotion();
            _TriggerEntryPlayer = true;
        }
        OnTriggerEnterEvnt?.Invoke(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _TriggerEntryPlayer = false;
        }
        OnTriggerExitEvnt?.Invoke(collision);
    }
    private void OnBecameInvisible()
    {
        PotionPool.Instance.Add(this);
    }
}
