using UnityEngine
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(gra))]
public class locationeditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        gra mygra = (gra) target;
        EditorGUILayout.
    }
}
