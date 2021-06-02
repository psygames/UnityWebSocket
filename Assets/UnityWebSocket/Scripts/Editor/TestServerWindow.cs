using UnityEditor;
using UnityEngine;

namespace UnityWebSocket.Editor
{
    internal class TestServerWindow : EditorWindow
    {
        internal static readonly int[] ASM_MEMORY_SIZE = new int[] { 256, 512, 1024 };
        internal static readonly int[] LINKER_TARGET = new int[] { 0, 2 };
        static SettingsWindow window = null;
        [MenuItem("Tools/UnityWebSocket/Test Server", priority = 100)]
        internal static void Open()
        {
            if (window != null)
            {
                window.Close();
            }

            window = GetWindow<SettingsWindow>(true, "Test Server");
            window.minSize = window.maxSize = new Vector2(600, 310);
            window.Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Fix"))
            {

            }
        }
    }
}
