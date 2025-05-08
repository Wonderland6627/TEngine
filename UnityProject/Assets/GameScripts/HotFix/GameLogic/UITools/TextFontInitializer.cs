using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [RequireComponent(typeof(Text))]
    public class TextFontInitializer : MonoBehaviour
    {
        public Text text;

        void Start()
        {
            //"https://a.unity.cn/client_api/v1/buckets/cde09f24-d39c-4845-a3e3-17344f4f2894/content/MiniGame/Fonts/simhei.ttf"
            string url = Application.streamingAssetsPath + "/Fonts/simhei.ttf";
            FontGetter.LoadFont(url, (font) => 
            {
                if (font == null)
                {
                    Log.Error("[TextFontInitializer] init text font fail");
                    return;
                }
                text.font = font;
                Log.Info($"[TextFontInitializer] init text font success: {text.text}");
            });
        }
    }
}
