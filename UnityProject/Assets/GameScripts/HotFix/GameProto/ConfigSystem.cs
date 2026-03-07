// 定义 LUBAN_USE_JSON 使用 cs-simple-json 生成的代码（JSON加载）
// 不定义则使用 cs-bin 生成的代码（二进制加载）
#define LUBAN_USE_JSON

using Luban;
using GameBase;
using GameConfig;
using TEngine;
using UnityEngine;
#if LUBAN_USE_JSON
using SimpleJSON;
#endif

/// <summary>
/// 配置加载器。
/// <para>通过文件顶部的 LUBAN_USE_JSON 宏切换 JSON / Binary 加载模式。</para>
/// </summary>
public class ConfigSystem : Singleton<ConfigSystem>
{
    private bool _init = false;

    private Tables _tables;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                Load();
            }

            return _tables;
        }
    }

    /// <summary>
    /// 加载配置。
    /// </summary>
    public void Load()
    {
#if LUBAN_USE_JSON
        _tables = new Tables(LoadJson);
#else
        _tables = new Tables(LoadByteBuf);
#endif
        _init = true;
    }

#if LUBAN_USE_JSON
    private JSONNode LoadJson(string file)
    {
        TextAsset textAsset = GameModule.Resource.LoadAsset<TextAsset>(file);
        string json = textAsset.text;
        GameModule.Resource.UnloadAsset(textAsset);
        return JSON.Parse(json);
    }
#else
    private ByteBuf LoadByteBuf(string file)
    {
        TextAsset textAsset = GameModule.Resource.LoadAsset<TextAsset>(file);
        byte[] bytes = textAsset.bytes;
        GameModule.Resource.UnloadAsset(textAsset);
        return new ByteBuf(bytes);
    }
#endif
}