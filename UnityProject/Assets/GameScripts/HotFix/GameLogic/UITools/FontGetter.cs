using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using WeChatWASM;

namespace GameLogic
{
    public class FontGetter
    {
        private static Font WXFont;

        public static void LoadFont(string fallbackUrl, Action<Font> onComplete)
        {
            if (WXFont != null)
            {
                Log.Info($"[FontGetter] WXFont exists, font: {WXFont.name}");
                onComplete.Invoke(WXFont);
                return;
            }
            WX.GetWXFont(fallbackUrl, (font) => 
            {
                if (font == null)
                {
                    Log.Error("[FontGetter] GetWXFont fail");
                    onComplete.Invoke(null);
                    return;
                }

                WXFont = font;
                onComplete.Invoke(WXFont);
                Log.Info($"[FontGetter] GetWXFont success, font: {font.name}");
            });
        }
    }
}
