using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace UnityWebSocket.Editor
{
    public class WebSocketPlayerSettingCheck
    {
#if UNITY_2018_1_OR_NEWER
        [MenuItem("UnityWebSocket/LinkerTarget/Wasm", false, 1)]
        private static void WebSocketSettingLinkerTargetWasm()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        }

        [MenuItem("UnityWebSocket/LinkerTarget/Wasm", true, 1)]
        private static bool WebSocketSettingLinkerTargetWasmValidate()
        {
            return false;
        }

        [MenuItem("UnityWebSocket/LinkerTarget/Asm", false, 0)]
        private static void WebSocketSettingLinkerTargetAsm()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Asm;
        }

        [MenuItem("UnityWebSocket/LinkerTarget/Both", false, 2)]
        private static void WebSocketSettingLinkerTargetBoth()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;
        }

        [InitializeOnLoadMethod]
        public static void OnInit()
        {
            if (PlayerSettings.WebGL.linkerTarget == WebGLLinkerTarget.Wasm)
            {
                EditorUtility.DisplayDialog("Warning"
                    , "On WebGL platform should change via Menu:\nUnityWebSocket -> LinkerTarget -> Asm or Both"
                    , "OK");
            }
        }
#endif
    }
}