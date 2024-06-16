using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindLitBitches : EditorWindow
{
    private List<GameObject> gameObjectsToDestroy;
    private string materialName;
    
    [MenuItem("Tools/Find Lit Bitches")]
    public static void ShowWindows()
    {
        GetWindow(typeof(FindLitBitches));
    }

    private void OnGUI()
    {
        GUILayout.Space(30);

        GUILayout.Label(
            "Attentions : \n\n Ce tool supprime tous les game objects qui ont le Material LIT.\nToute suppression est définitive.\nA utiliser avec précaution.\nNow, find the lit bitches.");

        GUILayout.Space(30);
        
        GUILayout.Label("Material to destroy : ");
        materialName = GUILayout.TextField(materialName);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Click here to destroy lit bitches", GUILayout.Height(50f)))
        {
            if (Selection.count <= 0 || materialName == "")
            {
                Debug.LogError("Sélectionnez au moins un objet et référencez un nom de material");
            }
            
            DestroyLit();
        }
    }

    void DestroyLit()
    {
        GameObject[] allObjects = GetAllChildren(Selection.gameObjects);

        foreach (var obj in allObjects)
        {
            if (obj.TryGetComponent<Renderer>(out Renderer rend))
            {
                if (rend.sharedMaterial.name == materialName)
                {
                    DestroyImmediate(obj);
                    Debug.Log("objet détruit");
                }
            }
        }
    }
    
    private static GameObject[] GetAllChildren(GameObject[] selection)
    {
        List<Transform> t = new();

        foreach (GameObject o in selection)
        {
            t.AddRange(o.GetComponentsInChildren<Transform>(true));
        }

        return t.Distinct().Select(x => x.gameObject).ToArray();
    }
}
