using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(TerrainMaker))]
public class TerrainEditorScript : Editor
{
    private TerrainMaker terrainMaker;
    private Editor textureEditor;
    private Editor heightmapEditor;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TerrainSettings(terrainMaker.textureContainer, terrainMaker.Initialize, ref TerrainMaker.textureSettingsFoldOut, ref textureEditor);
        TerrainSettings(terrainMaker.heightMapSettings, terrainMaker.Initialize, ref TerrainMaker.heightMapSettingsFoldOut, ref heightmapEditor);
    }

    private void TerrainSettings(UnityEngine.Object settings, System.Action settingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (!foldout) return;
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();

                if (!check.changed) return;
                if (settingsUpdated != null)
                {
                    settingsUpdated();
                }
            }
        }
    }

    private void OnEnable()
    {
        terrainMaker = (TerrainMaker)target;
    }
}
