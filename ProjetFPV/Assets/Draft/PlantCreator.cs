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
    List<Material> materials;
    
    float rotation;
    float size;
    float density;
    bool lockX;
    bool lockY;
    bool lockZ;
    float depth;
    
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

            if (GUILayout.Button("Destroy Plant"))
            {
                while (plantCreator.transform.childCount > 0)
                {
                    DestroyImmediate(plantCreator.transform.GetChild(0).gameObject);
                }
            }
        }
        
        private static void GeneralParameters(PlantCreator plantCreator)
        {
            EditorGUILayout.LabelField("General Parameters", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("    Rotation", GUILayout.MaxWidth(80));
            plantCreator.rotation = EditorGUILayout.FloatField(plantCreator.rotation);

            EditorGUILayout.LabelField("        Size", GUILayout.MaxWidth(80));
            plantCreator.size = EditorGUILayout.FloatField(plantCreator.size);

            EditorGUILayout.LabelField("        Density", GUILayout.MaxWidth(80));
            plantCreator.density = EditorGUILayout.FloatField(plantCreator.density);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Axis locks", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("    X", GUILayout.MaxWidth(30));
            plantCreator.lockX = EditorGUILayout.Toggle(plantCreator.lockX);
            
            EditorGUILayout.LabelField("    Y", GUILayout.MaxWidth(30));
            plantCreator.lockY = EditorGUILayout.Toggle(plantCreator.lockY);
            
            EditorGUILayout.LabelField("   Z", GUILayout.MaxWidth(30));
            plantCreator.lockZ = EditorGUILayout.Toggle(plantCreator.lockZ);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private static void ListOfTextures(List<Material> list)
        {
            int size = Mathf.Max(0, EditorGUILayout.IntField("Size", list.Count));

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
            
            
            for (int i = 0; i < plantCreator.density; i++)
            {
                // Instantiate plane
                GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                
                RaycastHit hit;
                var x = Random.Range(-1f, 1f);
                var y = Random.Range(-1f, 1f);
                var z = Random.Range(-1f, 1f);

                var r = new Vector3(x, y, z).normalized;
                var raycastSpawner = plantCreator.gameObject.transform.position + r * Math.Max(
                    plantCreator.transform.localScale.x,
                    Math.Max(plantCreator.transform.localScale.y, plantCreator.transform.localScale.z));
                
                if (Physics.Raycast(raycastSpawner, plantCreator.gameObject.transform.position - raycastSpawner, out hit, 3f))
                {
                    newPlane.transform.position = hit.point + r/100;
                }
            
                // Set plane size
                newPlane.transform.localScale = Vector3.one * plantCreator.size / 100f;

                // Set plane orientation
                var rotPoint = hit.normal;

                // newPlane.transform.up = rotPoint;
                // newPlane.transform.RotateAround(rotPoint, Random.Range(0f, plantCreator.rotation));
                
                newPlane.transform.rotation = quaternion.Euler(Random.Range(0f, plantCreator.rotation), Random.Range(0f, plantCreator.rotation), Random.Range(0f, plantCreator.rotation));

                // Set plane texture
                newPlane.GetComponent<Renderer>().material = plantCreator.materials[Random.Range(0, plantCreator.materials.Count)];

                newPlane.transform.parent = host.transform;
                DestroyImmediate(newPlane.GetComponent<MeshCollider>());
            }
            
            // Set la rotation autour de la normale
            // Ajouter des constraints sur les 3 axes
            // Ajouter la depth pour faire les niveaux de feuillage
            // Ajouter un sécurité sur le destroy plant
            
            
        }
    }
    #endif
    #endregion
}

