using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    public class PlayerRankInfo
    {
        public int playerRank = -1;
        public string openID;
        public int progressLevelID = 0;
        public string nickName;
        public string avatarURL;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(openID) && !string.IsNullOrEmpty(nickName) && !string.IsNullOrEmpty(avatarURL);
        }

        public bool IsSelf()
        {
            return openID == World.Instance.GameData.UserInfo.openID;
        }
    }

    partial class UIRankCell : UIWidget
    {
        public void SetData(PlayerRankInfo data)
        {
            m_textPlayerName.font = FontGetter.defaultFont;
            m_textPlayerRank.text = data.playerRank.ToString();
            m_textPlayerLevel.text = data.progressLevelID.ToString();

            string nickName = data.nickName;
            // string pattern =@"[^\p{L}\p{N}\p{P}\p{S}\p{Z}]";
            // Regex rex = new Regex(pattern);
            // nickName = rex.Replace(nickName, "");
            // if (nickName.Length == 0)
            // {
            //     nickName = "Player";
            //     Log.Warning($"[UIRankCell] nickName is unable to show, nickName = {data.nickName}");
            // }
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