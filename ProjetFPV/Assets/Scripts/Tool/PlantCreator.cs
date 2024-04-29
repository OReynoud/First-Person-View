using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlantCreator : MonoBehaviour
{
    private Material material;
    
    float rotationX;
    float rotationY;
    float rotationZ;
    float size;
    float density;
    bool lockX;
    bool lockY;
    bool lockZ;
    private bool[] constraints = new[] { false, false, false, false, false, false};
    int depth;
    LayerMask defaultLayer;
    
    int numberOfGameObjects;
    int numberOfTris;
    private int numberOfCurrentTris;
    List<GameObject> objectsToCombine = new List<GameObject>();

    List<GameObject> lastCreated = new List<GameObject>();
    
    #region Editor
    #if UNITY_EDITOR
    [CustomEditor(typeof(PlantCreator))]
    public class PlantCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PlantCreator plantCreator = (PlantCreator)target;
            
            GeneralParameters(plantCreator);
            
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Material", GUILayout.MaxWidth(80));
            plantCreator.material = (Material)EditorGUILayout.ObjectField(plantCreator.material, typeof(Material), false);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Generate Plant"))
            {
                if (plantCreator.material is null)
                {
                    Debug.Log("<color=yellow> From : Plant Creator Tool | </color>Au moins un material est nécessaire.");
                    return;
                }
                
                GeneratePlant(plantCreator);
            }
            
            if (GUILayout.Button("Undo"))
            {
                Undo(plantCreator);
            }

            EditorGUILayout.Space(20);
            
            GUI.color = Color.green;
            
            if (GUILayout.Button("Merge Plant"))
            {
                CombineMeshes(plantCreator);
            }
            
            EditorGUILayout.Space(20);

            GUI.color = Color.red;
            
            if (GUILayout.Button("Destroy Plant"))
            {
                DestroyPlant(plantCreator);
            }
        }

        private static void GeneralParameters(PlantCreator plantCreator)
        {
            EditorGUILayout.LabelField("Perfs Analyser", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Number of Game Objects : " + plantCreator.numberOfGameObjects);
            EditorGUILayout.LabelField("Number of Tris : " + plantCreator.numberOfTris);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
            
            EditorGUILayout.LabelField("General Parameters", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("        Size", GUILayout.MaxWidth(80));
            plantCreator.size = EditorGUILayout.FloatField(plantCreator.size);

            EditorGUILayout.LabelField("        Density", GUILayout.MaxWidth(80));
            plantCreator.density = EditorGUILayout.FloatField(plantCreator.density);

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("    Depth", GUILayout.MaxWidth(80));
            plantCreator.depth = EditorGUILayout.IntSlider(plantCreator.depth, 1, 5);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(20);
            
            EditorGUILayout.LabelField("Rotations", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X", GUILayout.MaxWidth(30));
            plantCreator.lockX = EditorGUILayout.Toggle(plantCreator.lockX);
            plantCreator.rotationX = EditorGUILayout.FloatField(plantCreator.rotationX);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(30));
            plantCreator.lockY = EditorGUILayout.Toggle(plantCreator.lockY);
            plantCreator.rotationY = EditorGUILayout.FloatField(plantCreator.rotationY);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(30));
            plantCreator.lockZ = EditorGUILayout.Toggle(plantCreator.lockZ);
            plantCreator.rotationZ = EditorGUILayout.FloatField(plantCreator.rotationZ);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Faces", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Front", GUILayout.MaxWidth(30));
            plantCreator.constraints[0] = EditorGUILayout.Toggle(plantCreator.constraints[0]);
            EditorGUILayout.LabelField("Back", GUILayout.MaxWidth(30));
            plantCreator.constraints[1] = EditorGUILayout.Toggle(plantCreator.constraints[1]);
            EditorGUILayout.LabelField("Left", GUILayout.MaxWidth(30));
            plantCreator.constraints[2] = EditorGUILayout.Toggle(plantCreator.constraints[2]);
            EditorGUILayout.LabelField("Right", GUILayout.MaxWidth(30));
            plantCreator.constraints[3] = EditorGUILayout.Toggle(plantCreator.constraints[3]);
            EditorGUILayout.LabelField("Up", GUILayout.MaxWidth(30));
            plantCreator.constraints[4] = EditorGUILayout.Toggle(plantCreator.constraints[4]);
            EditorGUILayout.LabelField("Down", GUILayout.MaxWidth(30));
            plantCreator.constraints[5] = EditorGUILayout.Toggle(plantCreator.constraints[5]);
            EditorGUILayout.EndHorizontal();
            
        }
        
        private static void ListOfTextures(List<Material> list)
        {
            int size;
            size = Mathf.Max(0, list != null ? EditorGUILayout.IntField("Size", list.Count) : EditorGUILayout.IntField("Size", 0));

            while (size > list.Count)
            {
                list.Add(null);
            }

            while (size < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = EditorGUILayout.ObjectField("Element " + i, list[i], typeof(Material), false) as Material;
            }
        }

        private static void GeneratePlant(PlantCreator plantCreator)
        {
            if (plantCreator.numberOfCurrentTris >= 30000)
            {
                Debug.Log("<color=yellow> From : Plant Creator Tool | </color>Nombre de tris maximum atteint (30'000).");
                return;
            }
            
            var host = plantCreator.gameObject;

            // Creates a parent to add a destruction security
            
            GameObject parent = new GameObject("Plants");
            plantCreator.numberOfGameObjects++;
            parent.transform.position = host.transform.position;
            parent.transform.parent = host.transform;
            plantCreator.lastCreated.Add(parent);

            SceneVisibilityManager.instance.DisablePicking(parent, false);
            
            // Change layer for raycast

            plantCreator.defaultLayer = plantCreator.gameObject.layer;
            plantCreator.gameObject.layer = LayerMask.NameToLayer("PlantCreator");
            
            int layerMask = 1 << LayerMask.NameToLayer("PlantCreator");
            
            for (int i = 0; i < plantCreator.density; i++)
            {
                if (plantCreator.numberOfCurrentTris >= 30000)
                {
                    Debug.Log("<color=yellow> From : Plant Creator Tool | </color>Nombre de tris maximum atteint (30'000). Interruption de la génération à l'étape " + i);
                    return;
                }
                
                // Instantiate plane
                GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                plantCreator.numberOfGameObjects++;
                plantCreator.objectsToCombine.Add(newPlane);
                plantCreator.numberOfTris += 2;
                plantCreator.numberOfCurrentTris += 2;
                
                RaycastHit hit;
                var x = plantCreator.constraints[2] ? 1f : plantCreator.constraints[3] ? -1f : Random.Range(-1f, 1f);
                var y = plantCreator.constraints[4] ? 1f : plantCreator.constraints[5] ? -1f : Random.Range(-1f, 1f);
                var z = plantCreator.constraints[0] ? 1f : plantCreator.constraints[1] ? -1f : Random.Range(-1f, 1f);

                var r = new Vector3(x, y, z).normalized;
                var raycastSpawner = plantCreator.gameObject.transform.position + r * 100f;
                
                if (Physics.Raycast(raycastSpawner, plantCreator.gameObject.transform.position - raycastSpawner, out hit, Mathf.Infinity, layerMask))
                {
                    newPlane.transform.position = hit.point + r/100 * plantCreator.depth;
                }
            
                // Set plane size
                newPlane.transform.localScale = Vector3.one * plantCreator.size / 100f;

                // Set plane orientation
                Vector3[] vectors;
                VectorCalculator(hit.normal, out vectors);

                newPlane.transform.up = hit.normal;
                
                if (plantCreator.lockX)
                {
                    newPlane.transform.RotateAround(vectors[0], Random.Range(0f, plantCreator.rotationX));
                }

                if (plantCreator.lockY)
                {
                    newPlane.transform.RotateAround(vectors[1], Random.Range(0f, plantCreator.rotationY));
                }

                if (plantCreator.lockZ)
                {
                    newPlane.transform.RotateAround(vectors[2], Random.Range(0f, plantCreator.rotationZ));
                }

                // Set plane texture
                newPlane.GetComponent<Renderer>().material = plantCreator.material;

                newPlane.transform.parent = parent.transform;
                DestroyImmediate(newPlane.GetComponent<MeshCollider>());
                SceneVisibilityManager.instance.DisablePicking(newPlane, false);
                
                // Set un deuxième quad au même endroit, mais retourné
                GameObject secondPlane = Instantiate(newPlane, newPlane.transform.position, newPlane.transform.rotation, parent.transform);
                plantCreator.numberOfGameObjects++;
                plantCreator.objectsToCombine.Add(secondPlane);
                plantCreator.numberOfTris += 2;
                plantCreator.numberOfCurrentTris += 2;
                secondPlane.transform.localScale = new Vector3(secondPlane.transform.localScale.x, secondPlane.transform.localScale.y, -secondPlane.transform.localScale.z);
            }
            
            // Remet le bon layer

            plantCreator.gameObject.layer = plantCreator.defaultLayer;
        }

        private static void VectorCalculator(Vector3 normal, out Vector3[] vectors)
        {
            vectors = new Vector3[3];

            Vector3 arbitraryVector = normal == Vector3.up ? Vector3.right : Vector3.up;

            vectors[0] = Vector3.Cross(normal, arbitraryVector).normalized;
            vectors[1] = Vector3.Cross(normal, vectors[0]).normalized;
            vectors[2] = normal;
        }
        
        private static void DestroyPlant(PlantCreator plantCreator)
        {
            for (var i = plantCreator.transform.childCount - 1; i >= 0; i--)
            {
                if (plantCreator.transform.GetChild(i).name == "Plants")
                {
                    DestroyImmediate(plantCreator.transform.GetChild(i).gameObject);
                    plantCreator.lastCreated = new List<GameObject>();
                }
                
                else if (plantCreator.transform.GetChild(i).name == "CombinedMesh")
                {
                    DestroyImmediate(plantCreator.transform.GetChild(i).gameObject);
                }
            }

            plantCreator.numberOfGameObjects = 0;
            plantCreator.objectsToCombine = new List<GameObject>();
            plantCreator.numberOfTris = 0;
            plantCreator.numberOfCurrentTris = 0;
        }

        private static void Undo(PlantCreator plantCreator)
        {
            if (plantCreator.lastCreated.Count <= 0) return;
            
            plantCreator.numberOfGameObjects -= plantCreator.lastCreated[^1].transform.childCount + 1;
            plantCreator.numberOfTris -= plantCreator.lastCreated[^1].transform.childCount * 2;
            plantCreator.numberOfCurrentTris -= plantCreator.lastCreated[^1].transform.childCount * 2;

            foreach (Transform child in plantCreator.lastCreated[^1].transform)
            {
                plantCreator.objectsToCombine.Remove(child.gameObject);
            }
            
            DestroyImmediate(plantCreator.lastCreated[^1]);
            plantCreator.lastCreated.RemoveAt(plantCreator.lastCreated.Count - 1);
        }
        
        private static void CombineMeshes(PlantCreator plantCreator)
        {
            if (plantCreator.objectsToCombine.Count <= 0)
            {
                Debug.Log("<color=yellow> From : Plant Creator Tool | </color>Aucune plante à fusionner. Opération annulée.");
                return;
            }
            
            // Liste des CombineInstances pour stocker les informations sur les Meshes à fusionner
            CombineInstance[] combineInstances = new CombineInstance[plantCreator.objectsToCombine.Count];

            for (int i = 0; i < plantCreator.objectsToCombine.Count; i++)
            {
                // Crée une nouvelle instance de CombineInstance pour chaque GameObject
                combineInstances[i].mesh = plantCreator.objectsToCombine[i].GetComponent<MeshFilter>().sharedMesh;
                combineInstances[i].transform = Matrix4x4.TRS(plantCreator.objectsToCombine[i].transform.localPosition,
                    plantCreator.objectsToCombine[i].transform.localRotation,
                    plantCreator.objectsToCombine[i].transform.localScale);
            }

            // Crée un nouveau GameObject pour contenir le Mesh combiné
            GameObject combinedObject = new GameObject("CombinedMesh");
            combinedObject.transform.position = plantCreator.transform.position;
            combinedObject.transform.parent = plantCreator.transform;
        
            // Ajoute un MeshFilter et un MeshRenderer au nouveau GameObject
            MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();

            // Fusionne les Meshes en un seul Mesh
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.CombineMeshes(combineInstances, true, true);
            Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);

            // Assigne le matériau du premier objet à la MeshRenderer du nouveau GameObject
            meshRenderer.material = plantCreator.objectsToCombine[0].GetComponent<MeshRenderer>().sharedMaterial;

            for (var i = plantCreator.transform.childCount - 1; i >= 0; i--)
            {
                if (plantCreator.transform.GetChild(i).name == "Plants")
                {
                    DestroyImmediate(plantCreator.transform.GetChild(i).gameObject);
                }
            }

            plantCreator.objectsToCombine = new List<GameObject>();

            plantCreator.numberOfGameObjects = 0;
            plantCreator.numberOfCurrentTris = 0;

            foreach (Transform child in plantCreator.transform)
            {
                if (child.name == "CombinedMesh")
                {
                    plantCreator.numberOfGameObjects++;
                }
            }
        }
    }
    #endif
    #endregion
}

