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

        [MenuItem("UnityWebSocket/LinkerTarget/asm.js", false, 0)]
        private static void WebSocketSettingLinkerTargetAsm()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Asm;
        }

        [MenuItem("UnityWebSocket/LinkerTarget/Both", false, 2)]
        private static void WebSocketSettingLinkerTargetBoth()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;
        }

        [MenuItem("UnityWebSocket/CheckSettings", false, 10)]
        private static void CheckSettings()
        {
            if (PlayerSettings.WebGL.linkerTarget == WebGLLinkerTarget.Wasm)
            {
                EditorUtility.DisplayDialog("Warning"
                    , "On WebGL platform WebGL Linker Target should be asm.js or Both, via Menu:\nUnityWebSocket -> LinkerTarget -> asm.js or Both"
                    , "OK");
            }
            else if (PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy)
            {
                EditorUtility.DisplayDialog("Warning"
                    , "Scripting Runtime Version should be .NET 4.x, via Menu:\nPlayerSettings -> Other Settings -> Script Runtime Version -> .Net 4.x Equivalent"
                    , "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Success"
                    , "Your settings is OK."
                    , "OK");
            }
        }

        [InitializeOnLoadMethod]
        public static void OnInit()
        {
            if (PlayerSettings.WebGL.linkerTarget == WebGLLinkerTarget.Wasm)
            {
                EditorUtility.DisplayDialog("Warning"
                    , "On WebGL platform WebGL Linker Target should be Asm or Both, via Menu:\nUnityWebSocket -> LinkerTarget -> Asm or Both"
                    , "OK");
            }

            if (PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy)
            {
                EditorUtility.DisplayDialog("Warning"
                    , "Scripting Runtime Version should be .NET 4.x, via Menu:\nPlayerSettings -> Other Settings -> Script Runtime Version -> .Net 4.x Equivalent"
                    , "OK");
            }
        }
#endif
    }
}