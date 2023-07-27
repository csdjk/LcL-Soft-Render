using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace LcLSoftRenderer
{
    [CustomEditor(typeof(RenderObject))]
    public class RenderObjectEditor : Editor
    {
        bool showShaderBaseProp = true;
        SerializedProperty shaderProp;
        SerializedProperty debugProp;
        string[] shaderNames;
        IEnumerable<System.Type> shaderTypes;
        // static Dictionary<RenderObject, Dictionary<string, LcLShader>> ShaderDictGlobal = new Dictionary<RenderObject, Dictionary<string, LcLShader>>();
        Dictionary<string, LcLShader> shaderDict = new Dictionary<string, LcLShader>();
        private void OnEnable()
        {
            shaderProp = serializedObject.FindProperty("shader");
            debugProp = serializedObject.FindProperty("debug");

            InitShaderList();
        }

        public void InitShaderList()
        {
            // 获取所有基于ShaderBase的子类
            shaderTypes = System.AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(a => a.GetTypes().Where(t => t.IsSubclassOf(typeof(LcLShader)) && !t.IsAbstract));
            // 获取所有基于ShaderBase的子类的名字
            shaderNames = shaderTypes.Select(t => t.Name).ToArray();
        }

        int shaderIndex = 0;
        public override void OnInspectorGUI()
        {
            var renderObject = target as RenderObject;
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            {
                if (renderObject.shader != null)
                    shaderIndex = shaderNames.ToList().IndexOf(renderObject.shader.GetType().Name);
                shaderIndex = EditorGUILayout.Popup("Shader", shaderIndex, shaderNames);
            }
            if (EditorGUI.EndChangeCheck() || renderObject.shader == null)
            {
                var shaderName = shaderNames[shaderIndex];
                if (!shaderDict.TryGetValue(shaderName, out var shader))
                {
                    shader = System.Activator.CreateInstance(shaderTypes.ElementAt(shaderIndex)) as LcLShader;
                    shaderDict.Add(shaderName, shader);
                }
                renderObject.SetShader(shader);
                serializedObject.ApplyModifiedProperties();

            }

            // 绘制一个折叠区域
            showShaderBaseProp = EditorGUILayout.Foldout(showShaderBaseProp, "Shader Settings");
            if (showShaderBaseProp)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Render Queue", GUILayout.ExpandWidth(false));
                        renderObject.shader.RenderQueue = (RenderQueue)EditorGUILayout.EnumPopup(renderObject.shader.RenderQueue, GUILayout.ExpandWidth(true));
                        renderObject.shader.RenderQueue = (RenderQueue)EditorGUILayout.IntField((int)renderObject.shader.RenderQueue, GUILayout.ExpandWidth(false));
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Cull Mode", GUILayout.ExpandWidth(false));
                        renderObject.shader.CullMode = (CullMode)EditorGUILayout.EnumPopup(renderObject.shader.CullMode);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("ZTest", GUILayout.ExpandWidth(false));
                        renderObject.shader.ZTest = (ZTest)EditorGUILayout.EnumPopup(renderObject.shader.ZTest);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("ZWrite", GUILayout.ExpandWidth(false));
                        renderObject.shader.ZWrite = (ZWrite)EditorGUILayout.EnumPopup(renderObject.shader.ZWrite);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Blend Mode", GUILayout.ExpandWidth(false));
                        // renderObject.shader.BlendMode = (BlendMode)EditorGUILayout.EnumPopup(renderObject.shader.BlendMode);
                        renderObject.shader.BlendMode = (BlendMode)EditorGUILayout.EnumPopup(renderObject.shader.BlendMode);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

            }


            EditorGUILayout.PropertyField(shaderProp, new GUIContent("Shader Property"), true);


            debugProp.boolValue = EditorGUILayout.Toggle("Debug", debugProp.boolValue);
            // debugProp.boolValue = EditorGUILayout.Foldout(debugProp.boolValue, "Debug");
            // if (debugProp.boolValue)
            // {

            // }


            serializedObject.ApplyModifiedProperties();
        }
    }
}