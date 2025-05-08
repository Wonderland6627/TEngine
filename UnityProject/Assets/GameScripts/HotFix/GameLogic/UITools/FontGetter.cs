using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using WeChatWASM;

namespace GameLogic
{
    public class FontGetter : MonoBehaviour
    {
        public Text[] allTexts;

        public string logTitle;
        public static Font wxFont;

        void Start()
        {
            logTitle = gameObject.name;
            allTexts = transform.GetComponentsInChildren<Text>(true);
            if (wxFont != null)
            {
                ApplyFont();
                return;
            }

            var fontName = "cartoon_font_1.ttf";
            var fallbackFontUrl = $"https://a.unity.cn/client_api/v1/buckets/cde09f24-d39c-4845-a3e3-17344f4f2894/content/MiniGame/Fonts/{fontName}";
            WX.GetWXFont(fallbackFontUrl, (font) =>
            {
                if (font == null)
                {
                    Log.Error($"[FontGetter - {logTitle}] GetWXFont failed");
                    return;
                }
                wxFont = font;
                ApplyFont();
            });
        }

        void ApplyFont()
        {
            for (int i = 0; i < allTexts.Length; i++)
            {
                if (allTexts[i].font == wxFont) continue;
                allTexts[i].font = wxFont;
            }
            Log.Info($"[FontGetter - {logTitle}] GetWXFont success, replace text font to {wxFont.name}");
        }
    }
}
