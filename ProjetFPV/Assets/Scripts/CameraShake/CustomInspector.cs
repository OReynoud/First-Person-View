using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraShake))]
public class CustomInspector : Editor
{
    int index;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CameraShake cs = (CameraShake)target;

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Try a preset", EditorStyles.boldLabel);
        
        index = EditorGUILayout.IntField("Index", index);
        if (GUILayout.Button("Try Camera Shake (F)"))
        {
            cs.ShakeOneShot(index);
        }

        if (GUILayout.Button("Start Infinite Camera Shake (G)"))
        {
            cs.StartInfiniteShake(index);
        }
        
        if (GUILayout.Button("Stop Infinite Camera Shake (H)"))
        {
            cs.StopInfiniteShake();
        }

    }
}
