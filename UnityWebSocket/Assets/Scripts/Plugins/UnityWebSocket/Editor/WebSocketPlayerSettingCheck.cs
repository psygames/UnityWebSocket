using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace UnityWebSocket.Editor
{
    public class WebSocketPlayerSettingCheck
    {
        [InitializeOnLoadMethod]
        public static void OnInit()
        {
#if UNITY_2018 || UNITY_2019
            if (PlayerSettings.WebGL.linkerTarget == WebGLLinkerTarget.Wasm)
            {
                if (EditorUtility.DisplayDialog("Warning", "On WebGL platform should change :\n" +
                    "PlayerSettings -> WebGL -> linkerTarget \n" +
                    "to [Both] or [asm.js] !!!", "Change", "Cancel"))
                {
                    PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;
                }
            }
#endif
        }
    }
}