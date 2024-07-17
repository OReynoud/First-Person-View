using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

// READ ME 

// Bonjour, voici quelques explications pour utiliser ce tool. Il sert à remplacer des game objects dans la scène par un ou des prefabs de votre choix.
// Pour l'utiliser, il suffit tout d'abord de le placer dans un dossier nommé Editor. Ensuite, le script peut être trouvé en haut dans Tools > Prefab Replacement.

// Son fonctionnement est simple : Il remplace tous les objets sélectionnés par un prefab ou plusieurs prefabs de votre choix (si vous en avez choisi plusieurs, il choisit aléatoirement
// pour chaque objet par quel prefab le remplacer.

// Il est facile à prendre en main. Sous Number of Prefabs, vous pouvez indiquer combien de prefabs vous souhaitez ajouter au remplacement (vous pouvez également utiliser les boutons
// + et -. Une liste se forme alors du nombre d'éléments indiqués. Vous pouvez placer un prefab de vos dossiers dans chaque emplacement (les emplacements vides sont automatiquement
// supprimés, pas de IndexOutOfRange ou NullReferenceException). Il vous suffit de cliquer sur le bouton pour voir tous vos objets sélectionnés être remplacés.

// Par défaut, les prefabs qui remplacent les objets gardent la même orientation que l'objet remplacé. Si vous le souhaitez, vous pouvez cocher la case "Rotate to Camera". Les prefabs
// seront alors automatiquement tournés de 180° si la caméra se trouve dans leur dos. Cela peut par exemple servir à mettre des fenêtres à la place d'un mur dans un kit modulaire, 
// et être sûr que la fenêtre pointe bien vers l'extérieur du bâtiment, si jamais le mur avait été mis à l'envers (forward vers l'intérieur).

// ATTENTION : Il n'y a pas de UNDO. La manipulation est définitive. Cependant, si vous vous êtes trompés, il suffit de reprendre le prefab précédent et de remplacer à nouveau. Dans tous
// les cas, il est conseillé de push ses changements avant pour éviter toute mauvaise surprise. Attention également car les objets de base sont supprimés, donc tous les scripts ou
// components attachés disparaissent également.

// N'hésitez pas à venir me voir en cas de doute, à modifier le script selon vos besoins, c'est complètement libre ! :D 

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

        GUILayout.Space(10);

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
