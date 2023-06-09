using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace LcLSoftRender
{
    [CustomEditor(typeof(RenderObject))]
    public class RenderObjectEditor : Editor
    {
        bool showShaderBaseProp = true;
        SerializedProperty shaderProp;
        string[] shaderNames;
        IEnumerable<System.Type> shaderTypes;
        // static Dictionary<RenderObject, Dictionary<string, LcLShader>> ShaderDictGlobal = new Dictionary<RenderObject, Dictionary<string, LcLShader>>();
        Dictionary<string, LcLShader> shaderDict = new Dictionary<string, LcLShader>();
        private void OnEnable()
        {
            shaderProp = serializedObject.FindProperty("shader");
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
                shaderIndex = shaderNames.ToList().IndexOf(renderObject.shader.GetType().Name);
                shaderIndex = EditorGUILayout.Popup("Shader", shaderIndex, shaderNames);
            }
            if (EditorGUI.EndChangeCheck())
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

                // 获取shaderProp基类的shader属性
                renderObject.shader.RenderQueue = (RenderQueue)EditorGUILayout.EnumPopup("Render Queue", renderObject.shader.RenderQueue);
                renderObject.shader.CullMode = (CullMode)EditorGUILayout.EnumPopup("Cull Mode", renderObject.shader.CullMode);
                renderObject.shader.ZTest = (ZTest)EditorGUILayout.EnumPopup("ZTest", renderObject.shader.ZTest);
                renderObject.shader.ZWrite = (ZWrite)EditorGUILayout.EnumPopup("ZWrite", renderObject.shader.ZWrite);
                renderObject.shader.BlendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", renderObject.shader.BlendMode);
                EditorGUILayout.EndVertical();

            }


            EditorGUILayout.PropertyField(shaderProp,new GUIContent("Shader Property"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}