using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace LcLSoftRenderer
{
    [CustomEditor(typeof(SoftRenderer))]
    public class SoftRendererEditor : Editor
    {
        SerializedProperty rasterizerTypeProp;
        SerializedProperty clearFlagsProp;
        SerializedProperty clearColorProp;
        SerializedProperty msaaModeProp;
        SerializedProperty primitiveTypeProp;
        SerializedProperty frameIntervalProp;
        SerializedProperty colorComputeShaderProp;



        private void OnEnable()
        {
            rasterizerTypeProp = serializedObject.FindProperty("rasterizerType");
            clearFlagsProp = serializedObject.FindProperty("clearFlags");
            clearColorProp = serializedObject.FindProperty("clearColor");
            msaaModeProp = serializedObject.FindProperty("msaaMode");
            primitiveTypeProp = serializedObject.FindProperty("primitiveType");
            frameIntervalProp = serializedObject.FindProperty("frameInterval");
            colorComputeShaderProp = serializedObject.FindProperty("colorComputeShader");
        }



        public override void OnInspectorGUI()
        {
            var renderer = target as SoftRenderer;
            serializedObject.Update();
            EditorGUILayout.PropertyField(rasterizerTypeProp);
            if(renderer.rasterizerType == RasterizerType.GPUDriven)
            {
                EditorGUILayout.PropertyField(colorComputeShaderProp);
            }



            EditorGUILayout.PropertyField(clearFlagsProp);
            if (renderer.clearFlags == CameraClearFlags.Color)
            {
                EditorGUILayout.PropertyField(clearColorProp);
            }
            EditorGUILayout.PropertyField(msaaModeProp);
            EditorGUILayout.PropertyField(primitiveTypeProp);
            EditorGUILayout.PropertyField(frameIntervalProp);


            serializedObject.ApplyModifiedProperties();
        }
    }
}