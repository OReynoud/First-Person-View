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
    List<Material> materials = new List<Material>();
    
    float rotationX;
    float rotationY;
    float rotationZ;
    float size;
    float density;
    bool lockX;
    bool lockY;
    bool lockZ;
    int depth;
    LayerMask defaultLayer;

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
            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
            
            List<Material> list = plantCreator.materials;
            
            ListOfTextures(list);

            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Generate Plant"))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
                
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = EditorGUILayout.ObjectField("Element " + i, list[i], typeof(Material), false) as Material;
                }

                if (list.Count <= 0)
                {
                    Debug.Log("At least one texture is needed");
                    return;
                }
                
                GeneratePlant(plantCreator);
            }
            
            if (GUILayout.Button("Undo"))
            {
                Undo(plantCreator);
            }

            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Destroy Plant"))
            {
                DestroyPlant(plantCreator);
            }
        }

        private static void GeneralParameters(PlantCreator plantCreator)
        {
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
            var host = plantCreator.gameObject;

            // Creates a parent to add a destruction security
            
            GameObject parent = new GameObject("Plants");
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
                // Instantiate plane
                GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                
                RaycastHit hit;
                var x = Random.Range(-1f, 1f);
                var y = Random.Range(-1f, 1f);
                var z = Random.Range(-1f, 1f);

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
                newPlane.GetComponent<Renderer>().material = plantCreator.materials[Random.Range(0, plantCreator.materials.Count)];

                newPlane.transform.parent = parent.transform;
                DestroyImmediate(newPlane.GetComponent<MeshCollider>());
                SceneVisibilityManager.instance.DisablePicking(newPlane, false);
                
                // Set un deuxième quad au même endroit, mais retourné
                GameObject secondPlane = Instantiate(newPlane, newPlane.transform.position, newPlane.transform.rotation, parent.transform);
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
            }
        }

        private static void Undo(PlantCreator plantCreator)
        {
            if (plantCreator.lastCreated.Count <= 0) return;
            
            DestroyImmediate(plantCreator.lastCreated[^1]);
            plantCreator.lastCreated.RemoveAt(plantCreator.lastCreated.Count - 1);
        }
    }
    #endif
    #endregion
}

