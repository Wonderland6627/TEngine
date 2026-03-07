using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLogic;
using TEngine;

namespace GameLogic
{
    public abstract class BaseObject : UIWidget
    {
        // 全局暂停状态标志
        private static bool s_isPaused = false;
        
        /// <summary>
        /// 是否处于暂停状态
        /// </summary>
        public static bool IsPaused => s_isPaused;
        
        /// <summary>
        /// 暂停所有史莱姆相关的游戏物体
        /// </summary>
        public static void PauseAll()
        {
            s_isPaused = true;
        }
        
        /// <summary>
        /// 恢复所有史莱姆相关的游戏物体
        /// </summary>
        public static void ResumeAll()
        {
            s_isPaused = false;
        }
        
        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            // 如果处于暂停状态，不执行游戏逻辑更新
            if (s_isPaused)
            {
                return;
            }
            
            // 调用子类的游戏逻辑更新方法
            OnGameUpdate();
        }
        
        /// <summary>
        /// 游戏逻辑更新方法，子类重写此方法实现具体的游戏逻辑
        /// </summary>
        protected virtual void OnGameUpdate()
        {
            // 子类重写此方法
        }
    }
}
