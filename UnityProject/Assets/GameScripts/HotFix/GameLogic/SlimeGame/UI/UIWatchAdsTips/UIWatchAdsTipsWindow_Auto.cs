using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TEngine;

namespace GameLogic
{
	partial class UIWatchAdsTipsWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgPopupBG;
		private Image m_imgClose;
		private Transform m_tfPopupContent;
		private Transform m_tfRewardsContent;
		private Button m_btnWatch;
		private GameObject m_itemRewardItem;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgPopupBG = FindChildComponent<Image>("Content/m_imgPopupBG");
			m_imgClose = FindChildComponent<Image>("Content/m_imgPopupBG/m_imgClose");
			m_tfPopupContent = FindChild("Content/m_tfPopupContent");
			m_tfRewardsContent = FindChild("Content/m_tfPopupContent/m_tfRewardsContent");
			m_btnWatch = FindChildComponent<Button>("Content/m_tfPopupContent/m_btnWatch");
			m_itemRewardItem = FindChild("Content/m_tfPopupContent/m_itemRewardItem").gameObject;
		}
		#endregion
	}
}