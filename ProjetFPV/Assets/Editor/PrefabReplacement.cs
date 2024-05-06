using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PrefabReplacement : EditorWindow
{
    private List<GameObject> prefab = new List<GameObject>();
    private int numberOfSelectedObjects;
    private GameObject go;
    private GameObject[] toSelect;
    private bool rotateToCamera;
    
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

        rotateToCamera = GUILayout.Toggle(rotateToCamera, "Rotate to Camera");
        
        GUILayout.Space(10);

        int size = Mathf.Max(0, EditorGUILayout.IntField("Number of prefabs", prefab.Count));

        for (int i = 0; i < prefab.Count; i++)
        {
            prefab[i] = EditorGUILayout.ObjectField("Prefab " + i, prefab[i], typeof(GameObject), false) as GameObject;
        }
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("+", GUILayout.Width(40f), GUILayout.Height(25f)))
        {
            size++;
        }
        if (GUILayout.Button("-", GUILayout.Width(40f), GUILayout.Height(25f)))
        {
            if (size > 0)
            {
                size--;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        while (size > prefab.Count)
        {
            prefab.Add(null);
        }
        
        while (size < prefab.Count)
        {
            prefab.RemoveAt(prefab.Count - 1);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Replace Game Objects", GUILayout.Height(25f)))
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
        for (int i = prefab.Count - 1; i >= 0; i--)
        {
            if (prefab[i] is null)
            {
                prefab.RemoveAt(i);
            }
        }
        
        toSelect = new GameObject[Selection.count];
        
        for (int i = Selection.count - 1; i >= 0; i--)
        {
            go = (GameObject)Selection.objects[i];
            
            GameObject newGO = PrefabUtility.InstantiatePrefab(prefab[Random.Range(0, prefab.Count)]) as GameObject;
            newGO.transform.position = go.transform.position;
            newGO.transform.rotation = go.transform.rotation;
            //newGO.transform.localScale = go.transform.localScale;

            if (go.GetComponent<Renderer>() != null && rotateToCamera)
            {
                SetRotation(newGO);
            }

            if (go.transform.parent is not null)
            {
                newGO.transform.parent = go.transform.parent;
            }
            
            DestroyImmediate(go);

            toSelect[i] = newGO;
        }

        Selection.objects = toSelect;
    }

    private void SetRotation(GameObject newObj)
    {
        var camPos = SceneView.lastActiveSceneView.camera.transform.position;
        var objPos = newObj.transform.GetComponent<Renderer>().bounds.center;

        var angle = Vector3.Angle(newObj.transform.up, camPos - objPos);
        
        if (angle < 90)
        {
            newObj.transform.RotateAround(newObj.transform.GetComponent<Renderer>().bounds.center, Vector3.up, 180f);
        }
    }
}
