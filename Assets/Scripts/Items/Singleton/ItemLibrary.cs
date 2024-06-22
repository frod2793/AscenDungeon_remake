using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ItemRating
{
    Common, Rare, Epic, Legendary
}
public class ItemLibrary : Singleton<ItemLibrary>
{
    public event System.Action<Item> ItemUnlockEvent;

    [Header("Item Probablities")]
    [SerializeField][Range(0f, 1f)] private float Common;
    [SerializeField][Range(0f, 1f)] private float Rare;
    [SerializeField][Range(0f, 1f)] private float Epic;
    [SerializeField][Range(0f, 1f)] private float Legendary;

    [Header("Registered Items")]
    [SerializeField] private RegisteredItem RegisteredItem;

    private Dictionary<ItemRating, List<Item>> _RunTimeLibrary;
    private Dictionary<ItemRating, List<Item>>        _Library;

    private List<Item>   _LockedItemListForTest;
    private List<Item> _UnlockedItemListForTest;

    private float[] mProbabilityArray;
    private bool _IsAlreadyInit;

    [ContextMenu("Probablity Check")]
    private void ProbablityCheck()
    {
        float sum = Common + Rare + Epic + Legendary;

        if (sum == 1)
        {
            Debug.Log($"Probablity Sum : {sum}");
        }
        else
            Debug.LogError($"Probablity Sum : {sum}");
    }

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _IsAlreadyInit = true;

            mProbabilityArray = new float[4]
            {
                Common, Rare, Epic, Legendary
            };
            _Library = new Dictionary<ItemRating, List<Item>>();

            _Library.Add(ItemRating.Common,    new List<Item>());
            _Library.Add(ItemRating.Rare,      new List<Item>());
            _Library.Add(ItemRating.Epic,      new List<Item>());
            _Library.Add(ItemRating.Legendary, new List<Item>());

            var unlockedList = ItemStateSaver.Instance.GetUnlockedItem();
            for (int i = 0; i < unlockedList.Count; ++i)
            {
                ItemID id = unlockedList[i].ID;
                Item instance = RegisteredItem.GetItemInstance(id);

                if (instance != null)
                {
                    var item = Instantiate(instance);
                    item.transform.position = new Vector2(-10f, 0f);

                    _Library[item.Rating].Add(item);
                }
            }
            ItemBoxReset();
        }
    }

    public Item GetItemObject(ItemID id)
    {
        Init();
        Item instance = RegisteredItem.GetItemInstance(id);

        if (instance != null)
        {
            ItemRating rating = RegisteredItem.GetItemInstance(id).Rating;
            return _Library[rating].FirstOrDefault(o => o.ID == id);
        }
        return null;
    }
    public Item GetRandomItem()
    {
        Init();

        float sum = 0f;
        float probability = Random.value;

        // 더 이상 반환할 수 있는 아이템이 없다면, 초기화한다.
        if (_RunTimeLibrary.Values.All(o => o.Count == 0))
        {
            ItemBoxReset();
        }
        for (int invokeCount = 0; invokeCount < 4; invokeCount++)
        {
            ItemRating currentRating = (ItemRating)invokeCount;

            // 현재 등급의 아이템에서 반환할 수 있는 아이템이 없다면
            if (_RunTimeLibrary[currentRating].Count == 0)
            {
                // 반복 중단
                continue;
            }
            sum += mProbabilityArray[invokeCount];

            // 반환할 아이템의 등급이 정해졌다면
            if (probability <= sum)
            {
                int index = Random.Range(0, _RunTimeLibrary[currentRating].Count);

                // 그 등급의 아이템 중 무작위 아이템 반환
                var item = _RunTimeLibrary[currentRating][index];
                           _RunTimeLibrary[currentRating].RemoveAt(index);

                // 반환하기 전에, 리스트가 비어있다면
                if (_RunTimeLibrary[currentRating].Count == 0)
                {
                    RevisionProbablity(mProbabilityArray[invokeCount]);
                }
                return item;
            }
        }
        return null;
    }

    public void ItemBoxReset()
    {
        if (_IsAlreadyInit)
        {
            _RunTimeLibrary = _Library.ToDictionary(o => o.Key, i => i.Value.ToList());

            for (int invokeCount = 0; invokeCount < 4; invokeCount++)
            {
                if (_Library[(ItemRating)invokeCount].Count == 0)
                {
                    RevisionProbablity(mProbabilityArray[invokeCount]);
                }
            }
        }
    }

    public Item GetRandomItem(ItemRating rating)
    {
        Init();
        Item getItem = null;
        
        if (_RunTimeLibrary.Values.All(o => o.Count == 0))
        {
            ItemBoxReset();
        }
        if (_RunTimeLibrary[rating].Count != 0)
        {
            int itemIndex = Random.Range(0, _RunTimeLibrary[rating].Count);

            getItem = _RunTimeLibrary[rating][itemIndex];
                      _RunTimeLibrary[rating].RemoveAt(itemIndex);
        }
        return getItem;
    }

    private void RevisionProbablity(float selectedProbablity)
    {
        int division =
            _RunTimeLibrary.ToList().Count(o => o.Value.Count > 0);

        float additive =
            selectedProbablity / division;

        for (int i = 0; i < 4; i++)
        {
            // 반환할 수 있는 나머지 등급들의 확률을 보정한다
            if (_RunTimeLibrary[(ItemRating)i].Count > 0)
            {
                mProbabilityArray[i] += additive;
            }
        }
    }
}
