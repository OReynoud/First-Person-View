using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(CameraShake))]
public class CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CameraShake cs = (CameraShake)target;

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Try a preset", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Try Camera Shake (F)"))
        {
            cs.ShakeOneShot(cs.index);
        }

        if (GUILayout.Button("Start Infinite Camera Shake (G)"))
        {
            cs.StartInfiniteShake(cs.index);
        }
        
        if (GUILayout.Button("Stop Infinite Camera Shake (H)"))
        {
            cs.StopInfiniteShake();
        }

    }
}

#endif
