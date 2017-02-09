using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(gra))]
public class locEditor : Editor {

	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		gra mygra = (gra) target;
		if (GUILayout.Button("Make bot")) {
			mygra.init();
		}
	}
}
