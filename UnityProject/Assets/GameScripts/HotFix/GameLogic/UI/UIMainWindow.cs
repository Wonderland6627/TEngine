using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TEngine;

namespace GameLogic
{
    [Window(UILayer.UI, "Assets/AssetRaw/UI/UIMain/UIMainWindow.prefab")]
    public class UIMainWindow : UIWindow
    {
        [SerializeField]
        private RectTransform roadContainer;
        
        public void Start()
        {
            roadContainer = transform.Find("BG/RoadContainer").GetComponent<RectTransform>();
            
            World.Instance.Init();
            World.Instance.CreateRoads(roadContainer);
        }
    }
}
