using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBuildTool))]
public class LevelDataObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Load To Data"))
        {
            ((LevelBuildTool)target).Save();
        }
    }
}
