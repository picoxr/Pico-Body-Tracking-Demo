using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SimpleSample))]
public class SimpleSampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
        SimpleSample role = (SimpleSample)target;
        EditorGUILayout.LabelField("bones", EditorStyles.boldLabel);
        for (int i = 0; i < (int)BodyTrackerRole.ROLE_NUM; i++)
        {
            string boneName = ((BodyTrackerRole)i).ToString();
            role.BonesList[i] = (Transform)EditorGUILayout.ObjectField(boneName, role.BonesList[i], typeof(Transform), true);
        }
    }
}
