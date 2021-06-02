using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityWebSocket.Editor
{
    internal class TestServerWindow : EditorWindow
    {
        static TestServerWindow window = null;
        [MenuItem("Tools/UnityWebSocket/Test Server", priority = 100)]
        internal static void Open()
        {
            if (window != null)
            {
                window.Close();
            }

            window = GetWindow<TestServerWindow>(true, "Test Server");
            window.minSize = window.maxSize = new Vector2(600, 310);
            window.Show();
        }

        private void WalkFolder(string path, Action<string> callback)
        {
            foreach (var file in Directory.GetFiles(path, "*.cs"))
            {
                callback.Invoke(file);
            }

            foreach (var folder in Directory.GetDirectories(path))
            {
                WalkFolder(folder, callback);
            }
        }

        private string targetpath = "";
        private string fixDef = "#if !NET_LEGACY && (UNITY_EDITOR || !UNTIY_WEBGL)";
        private void OnGUI()
        {
            targetpath = EditorGUILayout.TextField("Path: ", targetpath);
            fixDef = EditorGUILayout.TextField("Fix Def: ", fixDef);
            if (GUILayout.Button("Fix"))
            {
                WalkFolder(targetpath, (file) =>
                {
                    var str = File.ReadAllText(file);
                    if (str.StartsWith(fixDef)) return;
                    str = fixDef + "\r\n" + str + "\r\n#endif\r\n";
                    File.WriteAllText(file, str);
                    Debug.Log("fix: " + file);
                });
            }
        }
    }
}
