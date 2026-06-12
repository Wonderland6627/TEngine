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
        public static bool isLoading = false;

        public static Font defaultFont;

        private void Start()
        {
            logTitle = gameObject.name;
            allTexts = transform.GetComponentsInChildren<Text>(true);

            async void LoadDefaultFont()
            {
                defaultFont = await GameModule.Resource.LoadAssetAsync<Font>("Assets/AssetRaw/Fonts/Alibaba-PuHuiTi-Medium.ttf");
            }
            LoadDefaultFont();
            
#if !UNITY_EDITOR
            if (wxFont != null)
            {
                ApplyFont();
                return;
            }
            
            GameEvent.AddEventListener(SlimeEvent.OnGetWXFont, OnGetWXFont);
            GameModule.UI.ShowLoading();
            TryGetWXFont();
#endif
        }

        private void OnGetWXFont()
        {
            ApplyFont();
            GameEvent.RemoveEventListener(SlimeEvent.OnGetWXFont, OnGetWXFont);
        }

        private void TryGetWXFont()
        {
            if (isLoading)
            {
                Log.Warning($"[FontGetter - {logTitle}] isLoading font, return");
                return;
            }
            isLoading = true;
            var fontName = "cartoon_font_1.ttf";
            var fallbackFontUrl = $"https://res.slime.piratecat.top/MiniGame/Fonts/{fontName}";
            WX.GetWXFont(fallbackFontUrl, (font) =>
            {
                GameModule.UI.ShowLoading(false);
                isLoading = false;
                if (font == null)
                {
                    Log.Error($"[FontGetter - {logTitle}] GetWXFont failed");
                    return;
                }
                Log.Info($"[FontGetter - {logTitle}] GetWXFont success, font name: {font.name}");
                wxFont = font;
                GameEvent.Send(SlimeEvent.OnGetWXFont);
            });
        }

        void ApplyFont()
        {
            if (wxFont == null) return;
            if (gameObject == null) return;
            if (allTexts == null || allTexts.Length == 0) return;
            for (int i = 0; i < allTexts.Length; i++)
            {
                Text text = allTexts[i];
                if (text == null) continue;
                if (text.font == wxFont) continue;
                text.font = wxFont;
            }
            Log.Info($"[FontGetter - {logTitle}] apply text font to {wxFont.name}");
        }
    }
}
