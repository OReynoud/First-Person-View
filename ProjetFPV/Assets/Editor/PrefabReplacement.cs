using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class PrefabReplacement : EditorWindow
{
    private GameObject prefab;
    private int numberOfSelectedObjects;
    private GameObject go;
    private GameObject[] toSelect;
    
    [MenuItem("Tools/Prefab Replacement")]
    public static void ShowWindows()
    {
        GetWindow(typeof(PrefabReplacement));
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Prefab", EditorStyles.boldLabel);

        GUILayout.Space(30);

        GUILayout.Label("Number of selected objects : " + Selection.count);

        GUILayout.Space(10);

        prefab = EditorGUILayout.ObjectField("New Prefab", prefab, typeof(GameObject), false) as GameObject;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Replace Game Objects"))
        {
            if (prefab is null)
            {
                Debug.Log("<color=yellow> From : Plant Creator Tool | </color>You must at least select one object and have a prefab.");
                return;
            }
            
            Replace();
        }
    }

    private void Replace()
    {
        toSelect = new GameObject[Selection.count];
        
        for (int i = Selection.count - 1; i >= 0; i--)
        {
            go = (GameObject)Selection.objects[i];
            
            GameObject newGO = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            newGO.transform.position = go.transform.position;
            newGO.transform.rotation = go.transform.rotation;
            newGO.transform.localScale = go.transform.localScale;
            
            DestroyImmediate(go);

            toSelect[i] = newGO;
        }

        Selection.objects = toSelect;
    }
}
