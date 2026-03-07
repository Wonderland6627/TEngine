using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TEngine;

namespace GameLogic
{
	partial class UIDebugWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgPopupBG;
		private Image m_imgClose;
		private Text m_textTitle;
		private Button m_btnAddCoin;
		private Button m_btnDeductCoin;
		private TextMeshProUGUI m_tmpCoin;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgPopupBG = FindChildComponent<Image>("Content/m_imgPopupBG");
			m_imgClose = FindChildComponent<Image>("Content/m_imgPopupBG/m_imgClose");
			m_textTitle = FindChildComponent<Text>("Content/m_imgPopupBG/m_textTitle");
			m_btnAddCoin = FindChildComponent<Button>("Content/Scroll View/Viewport/Content/Coin/m_btnAddCoin");
			m_btnDeductCoin = FindChildComponent<Button>("Content/Scroll View/Viewport/Content/Coin/m_btnDeductCoin");
			m_tmpCoin = FindChildComponent<TextMeshProUGUI>("Content/Scroll View/Viewport/Content/Coin/m_tmpCoin");
		}
		#endregion
	}
}