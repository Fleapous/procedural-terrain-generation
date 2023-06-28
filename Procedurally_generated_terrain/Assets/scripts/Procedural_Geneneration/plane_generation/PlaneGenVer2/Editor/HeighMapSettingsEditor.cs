using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeightMap))]
public class HeightMapEditor : Editor
{
    private SerializedProperty heightMapSettingsProp;
    private SerializedProperty debugProp;
    private SerializedProperty seedProb;
    private void OnEnable()
    {
        heightMapSettingsProp = serializedObject.FindProperty("heightMapSettings");
        debugProp = serializedObject.FindProperty("debug");
        seedProb = serializedObject.FindProperty("seed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(debugProp);
        EditorGUILayout.PropertyField(seedProb);
        EditorGUILayout.PropertyField(heightMapSettingsProp);

        HeightMap heightMap = target as HeightMap;
        if (heightMap == null)
        {
            HeightMapSettings heightMapSettings = heightMap.heightMapSettings;
            if (heightMapSettings != null)
            {
                // EditorGUILayout.PropertyField(heightMapSettingsProp.FindPropertyRelative("numberOfLayers"));
                
                SerializedProperty layerSettingsProp = heightMapSettingsProp.FindPropertyRelative("layerSettings");
                int numLayers = heightMapSettings.numberOfLayers;
                
                serializedObject.ApplyModifiedProperties(); // Apply the modified properties
                
                // Initialize the layerSettings array if it's null or its length doesn't match the numberOfLayers
                if (layerSettingsProp.arraySize != numLayers)
                {
                    layerSettingsProp.arraySize = numLayers;
                    serializedObject.ApplyModifiedProperties();
                    
                }

                for (int i = 0; i < layerSettingsProp.arraySize; i++)
                {
                    SerializedProperty layerSettingProp = layerSettingsProp.GetArrayElementAtIndex(i);

                    EditorGUILayout.LabelField("Layer " + i.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("enable"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("animationCurve"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("scale"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("octaves"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("persistence"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("lacunarity"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("amplificationConstant"));
                    EditorGUILayout.PropertyField(layerSettingProp.FindPropertyRelative("heightScalar"));

                    EditorGUILayout.Space();
                }
            }
        }
        serializedObject.ApplyModifiedProperties(); // Apply any remaining modified properties
        EditorGUILayout.Space();

        if (GUILayout.Button("Log Octave Values"))
        {
            SerializedProperty layerSettingsProp = heightMapSettingsProp.FindPropertyRelative("layerSettings");
            for (int i = 0; i < layerSettingsProp.arraySize; i++)
            {
                SerializedProperty layerSettingProp = layerSettingsProp.GetArrayElementAtIndex(i);
                int octaves = layerSettingProp.FindPropertyRelative("octaves").intValue;
                Debug.Log("Layer " + i + " Octaves: " + octaves);
            }
        }
    }
}







