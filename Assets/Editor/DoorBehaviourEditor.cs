using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorBehaviour))]
public class DoorBehaviourEditor : Editor
{
    private Item[] allItems;
    void OnEnable()
    {
        allItems = (Item[])Resources.FindObjectsOfTypeAll(typeof(Item));
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DoorBehaviour doorBehaviour = (DoorBehaviour)target;
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open", GUILayout.MaxWidth(300)))
        {
            doorBehaviour.Open();
        }
        if (GUILayout.Button("Close", GUILayout.MaxWidth(300)))
        {
            doorBehaviour.Close();
        }
        EditorGUILayout.EndHorizontal();

        foreach (var item in allItems)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(item.name, GUILayout.MaxWidth(200));
            if (GUILayout.Button("Lock", GUILayout.MaxWidth(200)))
            {
                doorBehaviour.CloseAndLock(item);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}