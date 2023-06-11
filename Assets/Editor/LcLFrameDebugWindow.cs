using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace LcLSoftRender
{
    public class LcLFrameDebugWindow : EditorWindow
    {
        SoftRender m_SoftRender;
        private int m_SelectedIndex = -1;
        private ListView m_ListView;
        private VisualElement m_SelectedObject;
        private Button m_EnableButton;

        [MenuItem("LcLTools/Frame Debug")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<LcLFrameDebugWindow>("LcL Frame Debug");
        }

        private void OnEnable()
        {
            m_SoftRender = FindObjectOfType<SoftRender>();

            m_EnableButton = new Button();
            m_EnableButton.text = "Enable";
            m_EnableButton.clicked += OnEnableDisableButtonClicked;
            rootVisualElement.Add(m_EnableButton);

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
                m_SoftRender.DebugIndex(indices.First());
#if UNITY_EDITOR
                m_SoftRender.Render();
#endif
            };
            // m_ListView.onSelectionChange += objects =>
            // {
            //     Debug.Log(objects);
            // };
            rootVisualElement.Add(m_ListView);
            m_ListView.style.display = DisplayStyle.None;
        }

        private void Update()
        {
            if (m_SoftRender == null)
            {
                m_SoftRender = FindObjectOfType<SoftRender>();
            }
            m_SoftRender.CollectRenderObjects();
        }

        private void OnEnableDisableButtonClicked()
        {
            if (m_ListView.style.display == DisplayStyle.None)
            {
                m_ListView.style.display = DisplayStyle.Flex;
                m_EnableButton.text = "Disable";
#if UNITY_EDITOR
                m_SoftRender.Init();
                m_SoftRender.Render();
#endif
            }
            else
            {
                m_ListView.style.display = DisplayStyle.None;
                m_EnableButton.text = "Enable";
            }
        }
    }
}