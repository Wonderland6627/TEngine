using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    public class PlayerRankInfo
    {
        public int playerRank = -1;
        public string openid;
        public int progressLevelID = 0;
        public string nickName;
        public string avatarURL;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(openid) && !string.IsNullOrEmpty(nickName) && !string.IsNullOrEmpty(avatarURL);
        }
    }

    partial class UIRankCell : UIWidget
    {
        public void SetData(PlayerRankInfo data)
        {
            m_textPlayerRank.text = data.playerRank.ToString();
            m_textPlayerName.text = data.nickName;
            m_textPlayerLevel.text = data.progressLevelID.ToString();
        }
    }

    partial class UIRankCell
    {
        #region 脚本工具生成的代码
        private Text m_textPlayerRank;
        private Text m_textPlayerName;
        private Text m_textPlayerLevel;
        protected override void ScriptGenerator()
        {
            m_textPlayerRank = FindChildComponent<Text>("m_textPlayerRank");
            m_textPlayerName = FindChildComponent<Text>("m_textPlayerName");
            m_textPlayerLevel = FindChildComponent<Text>("m_textPlayerLevel");
        }
        #endregion
    }
}