using UnityEditor;
using UnityEngine;

// READ ME 

// Bonjour, voici quelques explications pour utiliser ce tool. Il sert à fusionner plusieurs meshes ensemble dans le but de réduire le nombre de GameObjects présents dans la scène.
// Pour l'utiliser, il suffit tout d'abord de le placer dans un dossier nommé Editor. Ensuite, le script peut être trouvé en haut dans Tools > GameObject Merger.

// Son fonctionnement est simple : Il récupère tous les objets présents dans votre sélection et les fusionne en un seul mesh.

// Il est facile à prendre en main. Il suffit de remplir les deux emplacements prévus pour : Le premier sert à spécifier le dossier dans lequel les meshes nouvellement créés seront stockés.
// Le deuxième sert à référencer le nom du mesh crée. Après avoir cliqué sur le bouton, tous les game objects séléectionnés seront détruits et un nouvel objet avec un seul mesh combiné
// les remplacera. Ce mesh sera stocké selon le path indiqué et peut donc être réutilisé sur d'autres objets.

// ATTENTION : Le tool est loin d'être parfait et doit être utilisé avec précaution. Il est recommandé de push tous ses changements avant de commencer à utiliser le tool, pour être sûr
// de ne rien perdre, notamment car il supprime des game objects.

// D'un point de vue optimisation et ergonomie, les points suivants sont à prendre en compte : Au moment de fusionner les meshes, le nouveau gameObject créé aura un point de pivot centré
// en (0,0,0). Le tool n'est donc pas conçu pour les éléments qui doivent être déplacés ou tournés, et cela doit être pris en compte pour instancier un gameobject avec un mesh combiné. Les 
// gameobjects sont supprimés, donc tous leurs scripts et components vont disparaître également. Le nouveau mesh ne pourra accueillir qu'un seul material, et le dépliage UV est fait par Unity
// automatiquement. Il faut donc garder le tool pour des objets partageant un même material ou utilisant du triplanaire. Enfin, question optimisation, attention à ne pas fusionner des game
// objects trop éloignés, sinon la camera devra les render continuellement. Mieux vaut packer les merge dans des zones plus restreintes.

// N'hésitez pas à venir me voir en cas de doute, à modifier le script selon vos besoins, c'est complètement libre ! :D 

public class GameObjectMerger : EditorWindow
{
    [MenuItem("Tools/GameObject Merger")]
    public static void ShowWindows()
    {
        GetWindow(typeof(GameObjectMerger));
    }

    private string pathName;
    private string fileName;

    private void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.Label("Number of selected objects : " + Selection.count);

        GUILayout.Space(15);

        GUIStyle italic = new GUIStyle(GUI.skin.label);
        italic.fontStyle = FontStyle.Italic;
        
        GUILayout.Label("Save Path : ");
        GUILayout.Label("Example : Assets/Art/CombinedMeshes", italic);
        pathName = GUILayout.TextField(pathName);
        
        GUILayout.Space(5);
        
        GUILayout.Label("File name : ");
        GUILayout.Label("Example : House1_Walls", italic);
        fileName = GUILayout.TextField(fileName);

        GUILayout.Space(15);

        if (GUILayout.Button("Click here to merge game objects", GUILayout.Height(50f)))
        {
            Merge();
        }
    }

    private void Merge()
    {
        var path = pathName + "/" + fileName + ".asset";

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
            Undo.DestroyObjectImmediate(go);
        }

        AssetDatabase.CreateAsset(combinedMeshFilter.sharedMesh, path);
        AssetDatabase.SaveAssets();
    }
}