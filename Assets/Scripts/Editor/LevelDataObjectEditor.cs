using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelDataObject))]
public class LevelDataObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Load Image To Data"))
        {
            ((LevelDataObject)target).LoadFromImage();
        }
    }
}
