using UnityEditor;
using UnityEngine;

public class GameObjectMerger : EditorWindow
{
    [MenuItem("Tools/GameObject Merger")]
    public static void ShowWindows()
    {
        GetWindow(typeof(GameObjectMerger));
    }

    private string fileName;
    private string savePath = "Assets/Art/Meshes/Modulaire/Prefab/CombinedMeshes";
    
    private void OnGUI()
    {
        GUILayout.Space(30);

        GUILayout.Label("Instructions : \n\nCe tool permet de merge plusieurs mesh en un seul. \nLors de la fusion, les materials sont perdus. Un seul material \nne peut être assigné au nouveau mesh, " +
                        "il ne faut donc fusionner \nque les gameobject qui ont le même material. Avant de merge, il faut \nspécifier un nom pour le mesh qui va ainsi être crée. Si ce nom est déjà pris, une \n" +
                        "erreur va apparaître dans la console. Les meshs générés sont stockés dans \nAssets/Art/Meshes/Modulaire/Prefab/CombinedMeshes.");
        
        GUILayout.Space(30);
        
        GUILayout.Label("Number of selected objects : " + Selection.count);
        
        GUILayout.Space(15);

        GUILayout.Label("File name : ");
        fileName = GUILayout.TextField(fileName);
        
        GUILayout.Space(15);
        
        if (GUILayout.Button("Click here to merge game objects",  GUILayout.Height(50f)))
        {
            Merge();
        
        }
    }

    private void Merge()
    {
        var path = savePath + "/" + fileName + ".asset";

        if (fileName == "")
        {
            Debug.LogError("Précisez un nom pour votre fichier");
            return;
        }
        if (System.IO.File.Exists(path))
        {
            Debug.LogError("Ce fichier existe déjà et risque d'être remplacé. Choisissez un nom différent");
            return;
        }

        var parentGO = new GameObject("CombinedMeshParent");
        parentGO.transform.position = Vector3.zero;
        parentGO.transform.rotation = Quaternion.identity;
        
        CombineInstance[] combineInstances = new CombineInstance[Selection.count];
        
        for (int i = 0; i < Selection.count; i++)
        {
            Selection.gameObjects[i].transform.parent = parentGO.transform;
            // Crée une nouvelle instance de CombineInstance pour chaque GameObject
            combineInstances[i].mesh = Selection.gameObjects[i].GetComponent<MeshFilter>().sharedMesh;
            combineInstances[i].transform = Matrix4x4.TRS( Selection.gameObjects[i].transform.localPosition,
                Selection.gameObjects[i].transform.localRotation,
                Selection.gameObjects[i].transform.localScale);
        }
        
        // Crée un nouveau GameObject pour contenir le Mesh combiné
        GameObject combinedObject = new GameObject("CombinedMesh");
        combinedObject.transform.parent = parentGO.transform;
        combinedObject.transform.position = Vector3.zero;
        
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
        
        AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);
        AssetDatabase.SaveAssets();
    }
    
    // METTRE LA POSITION AU BON ENDROIT
}