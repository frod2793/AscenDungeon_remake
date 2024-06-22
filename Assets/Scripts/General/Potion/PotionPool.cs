using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PotionPool : Singleton<PotionPool>
{
    [SerializeField] private Potion[] _Potions;

    // ========== Player Property ========== //
    public Player Player => _Player;
    public AbilityTable PlayerAbilityTable => _Player.GetAbility;
    
    private Player _Player;
    private int _PlayerFloor;
    // ========== Player Property ========== //

    private Dictionary<PotionName, Potion> _PotionLib;
    private Dictionary<PotionName, Queue<Potion>> _PotionPool;
    private List<Potion> _EnablePotionList;

    private void Awake()
    {
        _Player = FindObjectOfType<Player>();

        _PotionLib  = new Dictionary<PotionName, Potion>();
        _PotionPool = new Dictionary<PotionName, Queue<Potion>>();
        _EnablePotionList = new List<Potion>();

        for (int i = 0; i < _Potions.Length; ++i)
        {
            Potion instance = _Potions[i];
            PotionName name = _Potions[i].Name;

            _PotionLib.Add(name, instance);
            _PotionPool.Add(name, new Queue<Potion>());

            instance = Instantiate(instance);
            instance.gameObject.SetActive(false);
            _PotionPool[name].Enqueue(instance);
        }
    }
    /*
    private void Update()
    {
        _PlayerFloor = Castle.Instance.PlayerFloor.FloorIndex;

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
            return isClick;
        }

        if (ClickCheck())
        {
            if (!EventSystem.current.IsPointerInUIObject())
            {
                var origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var rayHit = Physics2D.RaycastAll(origin, Vector2.zero);

                for (int i = 0; i < rayHit.Length; i++)
                {
                    for (int index = 0; index < _EnablePotionList.Count; index++)
                    {
                        if (rayHit[i].collider.gameObject.Equals(_EnablePotionList[index].gameObject))
                        {
                            _EnablePotionList[index].Interaction();
                            break;
                        }
                    }
                }
            }
        }
    }
    */
    public Potion Get(PotionName potionName)
    {
        if (_PotionPool[potionName].Count < 1)
        {
            _PotionPool[potionName].Enqueue(Instantiate(_PotionLib[potionName]));
        }
        Potion potion = _PotionPool[potionName].Dequeue();
        potion.gameObject.SetActive(true);
        _EnablePotionList.Add(potion);

        return potion;
    }
    public void Add(Potion potion)
    {
        _PotionPool[potion.Name].Enqueue(potion);
        _EnablePotionList.Remove(potion);

        potion.gameObject.SetActive(false);
    }
}
