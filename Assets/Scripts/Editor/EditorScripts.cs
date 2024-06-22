using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DataManager))]
public class DataManagerInit : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            EditorGUILayout.Space(16f);

            if (GUILayout.Button("Update DataTable", GUILayout.Height(31f)))
            {
                DataManager.Instance.Init();
            }
        }
    }
}

[CustomEditor(typeof(ItemStatObject))]
public class ItemStatObjectControler : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(15f);

        if (GUILayout.Button("Set Data From Json", GUILayout.Height(30f)))
        {
            if (Application.isEditor)
            {
                (target as ItemStatObject).Init();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

[CustomEditor(typeof(CharacterAblityObject))]
public class CharacterAblityObjectControler : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(15f);

        if (GUILayout.Button("Set Data From Json", GUILayout.Height(30f)))
        {
            if (Application.isEditor)
            {
                (target as CharacterAblityObject).Init();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

[CustomEditor(typeof(GameTester))]
public class AddItem : Editor
{
    private Item mAddTarget;

    private string mKillAllEnemyExpectionName = "Scarecrow";

    private float mGameSpeed = 1f;

    private int mJumpFloor = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Application.isPlaying)
        {
            GUILayout.Space(6f);

            GUILayout.Label("Inventory Control", EditorStyles.boldLabel);

            GUILayout.Space(2f);
            GUILayout.Label("Target Item");
            mAddTarget = EditorGUILayout.ObjectField(mAddTarget, typeof(Item), true) as Item;

            if (GUILayout.Button("Set Weapon Item", GUILayout.Height(20f)))
            {
                Inventory.Instance.SetWeaponSlot(mAddTarget);
            }
            if (GUILayout.Button("Add Item", GUILayout.Height(20f)))
            {
                Inventory.Instance.AddItem(mAddTarget);
            }
            if (GUILayout.Button("Clear Inventory", GUILayout.Height(20f)))
            {
                Inventory.Instance.Clear();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //==================================================================

            GUILayout.Space(6f);

            GUILayout.Label("Stage Control", EditorStyles.boldLabel);

            GUILayout.Space(2f);
            GUILayout.Label("Expection Enemy Name");
            mKillAllEnemyExpectionName =
            GUILayout.TextField(mKillAllEnemyExpectionName);

            if (GUILayout.Button("Kill All Enemy", GUILayout.Height(20f)))
            {
                var player = FindObjectOfType(typeof(Player)) as GameObject;

                var enemies = GameObject.FindGameObjectsWithTag("Enemy").Where(o => o.activeSelf).ToList();


                foreach (var enemy in enemies)
                {
                    if (enemy.name.Equals(mKillAllEnemyExpectionName)) continue;

                    if (enemy.TryGetComponent(out ICombatable combatable))
                    {
                        combatable.Damaged(float.MaxValue, player);
                    }
                }
            }
            //==================================================================
            GUILayout.Space(6f);

            GUILayout.Label("Game Control", EditorStyles.boldLabel);
            GUILayout.Label("Game Speed");
            mGameSpeed = GUILayout.HorizontalSlider(mGameSpeed, 0.1f, 2f);
            GUILayout.Space(14f);

            if (GUILayout.Button("Speed Reset", GUILayout.Height(20f)))
            {
                mGameSpeed = 1f;
            }
            Time.timeScale = mGameSpeed;
            GUILayout.Space(6f);

            GUILayout.Label("Floor Jump");
            mJumpFloor = EditorGUILayout.IntField(mJumpFloor);

            if (GUILayout.Button("Jump!", GUILayout.Height(20f)))
            {
                Castle.Instance.SetPlayerFloor(mJumpFloor);
            }
            GUILayout.Space(6f);
        }
    }
}

[CustomEditor(typeof(ItemInfo))]
public class ItemInfoController : Editor
{
    public ItemStatObject _TargetItem;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20f);
        GUILayout.Label("Select ItemStatObject");
        _TargetItem = EditorGUILayout.ObjectField(_TargetItem, typeof(ItemStatObject), true) as ItemStatObject;

        if (GUILayout.Button("Set Data", GUILayout.Height(30f)))
        {
            if (Application.isEditor)
            {
                ItemInfo info = target as ItemInfo;

                info.SetData(_TargetItem.NameKR, _TargetItem.Rating, _TargetItem.AttackPower);

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
