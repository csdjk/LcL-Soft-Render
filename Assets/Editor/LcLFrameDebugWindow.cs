using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace LcLSoftRenderer
{
    public class LcLFrameDebugWindow : EditorWindow
    {
        SoftRenderer m_SoftRender;
        ListView m_ListView;
        VisualElement m_SelectedObject;
        Button m_EnableButton;
        Label m_ShaderLabel;
        Label m_BlendLabel;
        Label m_ZTestLabel;
        Label m_ZWriteLabel;
        Label m_CullLabel;

        TwoPaneSplitView m_TwoPaneSplitView;


        [MenuItem("LcLTools/Frame Debug")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<LcLFrameDebugWindow>("LcL Frame Debug");
        }

        private void OnEnable()
        {
            var root = rootVisualElement;
            m_SoftRender = FindObjectOfType<SoftRenderer>();

            m_EnableButton = new Button();
            m_EnableButton.text = "Enable";
            m_EnableButton.clicked += OnEnableDisableButtonClicked;
            root.Add(m_EnableButton);


            // 创建一个分割区域
            m_TwoPaneSplitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(m_TwoPaneSplitView);
            m_TwoPaneSplitView.SetActive(false);



            m_ListView = new ListView();
            m_ListView.style.flexGrow = 1;
            m_ListView.makeItem = () =>
            {
                var label = new Label();
                label.style.flexGrow = 1;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                return label;
            };
            m_ListView.bindItem = (e, i) =>
            {
                var label = e as Label;
                var renderObject = m_SoftRender.renderObjects[i];
                label.text = renderObject.name;
            };

            m_ListView.itemsSource = m_SoftRender.renderObjects;
            m_ListView.selectionType = SelectionType.Single;
            m_ListView.onSelectedIndicesChange += (indices) =>
            {
                var index = indices.First();
                m_SoftRender.DebugIndex(index);
                UpdateInfoBox(index);
#if UNITY_EDITOR
                m_SoftRender.Render();
#endif
            };
            m_TwoPaneSplitView.Add(m_ListView);
            m_ListView.style.display = DisplayStyle.None;


            /// ================================ Info Box ================================
            var infoBox = new Box();
            infoBox.style.flexGrow = 1;
            infoBox.style.flexDirection = FlexDirection.Column;
            infoBox.style.fontSize = 14;
            m_ShaderLabel = new Label()
            {
                style = {
                    marginBottom = 10,
                    marginLeft = 10,
                    marginTop = 10,
                    marginRight = 10,
                }
            };
            infoBox.Add(m_ShaderLabel);

            m_BlendLabel = new Label()
            {
                style = {
                    marginBottom = 10,
                    marginLeft = 10,
                    marginTop = 10,
                    marginRight = 10,
                }
            };
            infoBox.Add(m_BlendLabel);

            m_ZTestLabel = new Label()
            {
                style = {
                    marginBottom = 10,
                    marginLeft = 10,
                    marginTop = 10,
                    marginRight = 10,
                }
            };
            infoBox.Add(m_ZTestLabel);

            m_ZWriteLabel = new Label()
            {
                style = {
                    marginBottom = 10,
                    marginLeft = 10,
                    marginTop = 10,
                    marginRight = 10,
                }
            };
            infoBox.Add(m_ZWriteLabel);

            m_CullLabel = new Label()
            {
                style = {
                    marginBottom = 10,
                    marginLeft = 10,
                    marginTop = 10,
                    marginRight = 10,
                }
            };
            infoBox.Add(m_CullLabel);

            m_TwoPaneSplitView.Add(infoBox);
        }

        private void Update()
        {
            if (m_SoftRender == null)
            {
                m_SoftRender = FindObjectOfType<SoftRenderer>();
            }
            m_SoftRender.CollectRenderObjects();
        }

        private void OnEnableDisableButtonClicked()
        {
            if (m_ListView.style.display == DisplayStyle.None)
            {
                m_ListView.style.display = DisplayStyle.Flex;
                m_EnableButton.text = "Disable";
                m_TwoPaneSplitView.SetActive(true);
#if UNITY_EDITOR
                m_SoftRender.Init();
                m_SoftRender.Render();
#endif
            }
            else
            {
                m_ListView.style.display = DisplayStyle.None;
                m_EnableButton.text = "Enable";
                m_TwoPaneSplitView.SetActive(false);
            }
        }

        void UpdateInfoBox(int index)
        {
            var renderObject = m_SoftRender.renderObjects[index];
            var shader = renderObject.shader;
            m_ShaderLabel.text = $"Shader: {shader.GetType().Name}";

            var blendMode = shader.BlendMode;
            m_BlendLabel.text = $"Blend: {blendMode}";

            var zTest = shader.ZTest;
            m_ZTestLabel.text = $"ZTest: {zTest}";

            var zWrite = shader.ZWrite;
            m_ZWriteLabel.text = $"ZWrite: {zWrite}";

            var cullMode = shader.CullMode;
            m_CullLabel.text = $"Cull: {cullMode}";
        }
    }
}