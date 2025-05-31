using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Text.RegularExpressions;

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

        public bool IsSelf()
        {
            return openid == World.Instance.GameData.UserInfo.openId;
        }
    }

    partial class UIRankCell : UIWidget
    {
        public void SetData(PlayerRankInfo data)
        {
            m_textPlayerRank.text = data.playerRank.ToString();
            m_textPlayerLevel.text = data.progressLevelID.ToString();

            string nickName = data.nickName;
            Regex rex = new Regex(@"^[\u4E00-\u9FA5A-Za-z0-9]+$");
            var result = rex.Match(nickName);
            if (!result.Success)
            {
                nickName = "Player";
                Log.Warning($"[UIRankCell] nickName is unable to show, nickName = {data.nickName}");
            }
            if (data.IsSelf())
            {
                nickName += " (我)";
            }
            m_textPlayerName.text = nickName;
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
			m_textPlayerLevel = FindChildComponent<Text>("trophy/m_textPlayerLevel");
		}
		#endregion
    }
}