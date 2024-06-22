using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrivingTutorial : TutorialBase
{
    private void Reset()
    {
        if (!TryGetComponent(out BoxCollider2D collider))
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
    }

    public override void StartTutorial()
    {
        base.StartTutorial();
        gameObject.SetActive(true);
    }

    public override void EndTutorial()
    {
        base.EndTutorial();
        gameObject.SetActive(false);

        var player = FindObjectOfType<Player>();
        player.MoveStop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            EndTutorial();
        }
    }
}
