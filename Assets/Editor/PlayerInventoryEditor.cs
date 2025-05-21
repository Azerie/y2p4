using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerInventory))]
public class PlayerInventoryEditor : Editor
{
    private Item[] allItems;
    void OnEnable()
    {
        allItems = (Item[])Resources.FindObjectsOfTypeAll(typeof(Item));
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (PlayerInventory.GetInstance() != null)
        {
            GUILayout.Space(10);
            foreach (var item in allItems)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.name, GUILayout.MaxWidth(200));
                if (GUILayout.Button("Add item", GUILayout.MaxWidth(100)))
                {
                    PlayerInventory.GetInstance().AddItem(item);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
