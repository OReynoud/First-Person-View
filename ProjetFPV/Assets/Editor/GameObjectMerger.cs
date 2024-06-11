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

        GUILayout.Label(
            "Instructions : \n\nCe tool permet de merge plusieurs mesh en un seul. \nLors de la fusion, les materials sont perdus. Un seul material \nne peut être assigné au nouveau mesh, " +
            "il ne faut donc fusionner \nque les gameobject qui ont le même material. Avant de merge, il faut \nspécifier un nom pour le mesh qui va ainsi être crée. Si ce nom est déjà pris, une \n" +
            "erreur va apparaître dans la console. Les meshs générés sont stockés dans \nAssets/Art/Meshes/Modulaire/Prefab/CombinedMeshes.");

        GUILayout.Space(30);

        GUILayout.Label("Number of selected objects : " + Selection.count);

        GUILayout.Space(15);

        GUILayout.Label("File name : ");
        fileName = GUILayout.TextField(fileName);

        GUILayout.Space(15);

        if (GUILayout.Button("Click here to merge game objects", GUILayout.Height(50f)))
        {
            Merge();

        }
    }

    private void Merge()
    {
        var path = savePath + "/" + fileName + ".asset";

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("Précisez un nom pour votre fichier");
            return;
        }

        if (System.IO.File.Exists(path))
        {
            Debug.LogError("Ce fichier existe déjà et risque d'être remplacé. Choisissez un nom différent");
            return;
        }

        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("Aucun GameObject sélectionné");
            return;
        }

        CombineInstance[] combineInstances = new CombineInstance[Selection.gameObjects.Length];

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var currentGO = Selection.gameObjects[i];
            var meshFilter = currentGO.GetComponent<MeshFilter>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"Le GameObject {currentGO.name} n'a pas de MeshFilter ou de Mesh associé.");
                continue;
            }

            combineInstances[i].mesh = meshFilter.sharedMesh;
            combineInstances[i].transform = currentGO.transform.localToWorldMatrix;
        }

        // Crée un nouveau GameObject pour contenir le Mesh combiné
        GameObject combinedObject = new GameObject(fileName);
        combinedObject.transform.position = Vector3.zero;

        // Ajoute un MeshFilter et un MeshRenderer au nouveau GameObject
        MeshFilter combinedMeshFilter = combinedObject.AddComponent<MeshFilter>();
        MeshRenderer combinedMeshRenderer = combinedObject.AddComponent<MeshRenderer>();

        // Fusionne les Meshes en un seul Mesh
        combinedMeshFilter.sharedMesh = new Mesh();
        combinedMeshFilter.sharedMesh.CombineMeshes(combineInstances, true, true);
        Unwrapping.GenerateSecondaryUVSet(combinedMeshFilter.sharedMesh);

        // Assigne le matériau du premier objet à la MeshRenderer du nouveau GameObject
        if (Selection.gameObjects.Length > 0)
        {
            var firstRenderer = Selection.gameObjects[0].GetComponent<MeshRenderer>();
            if (firstRenderer != null)
            {
                combinedMeshRenderer.material = firstRenderer.sharedMaterial;
            }
        }

        // Supprime les GameObjects d'origine
        foreach (GameObject go in Selection.gameObjects)
        {
            DestroyImmediate(go);
        }

        AssetDatabase.CreateAsset(combinedMeshFilter.sharedMesh, path);
        AssetDatabase.SaveAssets();
    }
}