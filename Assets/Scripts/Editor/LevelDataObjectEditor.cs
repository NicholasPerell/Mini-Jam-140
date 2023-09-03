using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBuildTool))]
public class LevelBuildToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Save To Data"))
        {
            ((LevelBuildTool)target).Save();
        }
        if (GUILayout.Button("Load From Data"))
        {
            ((LevelBuildTool)target).Load();
        }
    }
}
