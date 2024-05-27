using System;
using Codice.Client.BaseCommands.Merge.Xml;
using UnityEditor;
using UnityEngine;

public class SaveTheProject : EditorWindow
{
    [MenuItem("Tools/Save the Project")]
    public static void ShowWindows()
    {
        GetWindow(typeof(SaveTheProject));
    }

    private void OnGUI()
    {
        GUILayout.Space(30);
        
        GUILayout.Label("Number of selected objects : " + Selection.count);
        
        GUILayout.Space(15);
        
        if (GUILayout.Button("Click here to save the project",  GUILayout.Height(50f)))
        {
            Merge();
        
        }
    }

    private void Merge()
    {
        CombineInstance[] combineInstances = new CombineInstance[Selection.count];
        
        for (int i = 0; i < Selection.count; i++)
        {
            // Crée une nouvelle instance de CombineInstance pour chaque GameObject
            combineInstances[i].mesh = Selection.gameObjects[i].GetComponent<MeshFilter>().sharedMesh;
            combineInstances[i].transform = Matrix4x4.TRS( Selection.gameObjects[i].transform.localPosition,
                Selection.gameObjects[i].transform.localRotation,
                Selection.gameObjects[i].transform.localScale);
        }
        
        // Crée un nouveau GameObject pour contenir le Mesh combiné
        GameObject combinedObject = new GameObject("CombinedMesh");
        combinedObject.transform.position = Selection.gameObjects[0].transform.position;
        
        // Ajoute un MeshFilter et un MeshRenderer au nouveau GameObject
        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();
        
        // Fusionne les Meshes en un seul Mesh
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.CombineMeshes(combineInstances, true, true);
        Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
        
        // Assigne le matériau du premier objet à la MeshRenderer du nouveau GameObject
        meshRenderer.material = Selection.gameObjects[0].GetComponent<MeshRenderer>().sharedMaterial;

        foreach (GameObject go in Selection.gameObjects)
        {
            DestroyImmediate(go);
        }
    }
    
    // METTRE LA POSITION AU BON ENDROIT
    // AJOUTER UNE ZONE STRING
    // VERIFIER QU'UN FICHIER N'EXISTE PAS DEJA
    
}