using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using UnityEditor.Overlays;

namespace LcLTools
{
    [Overlay(typeof(SceneView), "LcL Tools", true)]
    [Icon("Assets/unity.png")]
    public class ScreenCaptureTools : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "LcL Tools Root" };

            var icon = EditorGUIUtility.IconContent("d_FrameCapture").image;

            var button = new Button() { };
            button.clicked += () =>
            {
                ScreenCapture.CaptureScreenshot("Assets/ScreenCapture.png");
                AssetDatabase.Refresh();
            };
            root.Add(button);

            var img = new Image() { image = icon };
            button.Add(img);

            return root;
        }
    }
}
