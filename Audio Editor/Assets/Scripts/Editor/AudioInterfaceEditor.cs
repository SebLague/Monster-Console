using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (AudioInterface))]
public class AudioInterfaceEditor : Editor {
    public override void OnInspectorGUI () {
        base.OnInspectorGUI ();

        if (GUILayout.Button ("Export")) {
            string result = ((AudioInterface) target).Export ();
            EditorGUIUtility.systemCopyBuffer = result;
            Debug.Log (result);
        }

        if (GUILayout.Button ("Save")) {
            ((AudioInterface) target).Save ();
        }
        if (GUILayout.Button ("Load")) {
            ((AudioInterface) target).Load ();

        }

    }
}