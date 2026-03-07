using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TEngine;

namespace GameLogic
{
	partial class UIGameOverWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgGameResultBG;
		private Text m_txtResult;
		private Transform m_tfSuccessContent;
		private Transform m_tfRewardContent;
		private Button m_btnClaimAds;
		private Text m_txtClaimAds;
		private Button m_btnClaim;
		private Text m_txtClaim;
		private Transform m_tfFailedContent;
		private Button m_btnTryAgain;
		private Text m_txtTryAgain;
		private Text m_txtEnergy;
		private Button m_btnBack;
		private Text m_txtEncourage;
		private Image m_imgUnit;

		protected override void ScriptGenerator()
		{
			m_imgGameResultBG = FindChildComponent<Image>("Content/m_imgGameResultBG");
			m_txtResult = FindChildComponent<Text>("Content/m_imgGameResultBG/m_txtResult");
			m_tfSuccessContent = FindChild("Content/m_tfSuccessContent");
			m_tfRewardContent = FindChild("Content/m_tfSuccessContent/m_tfRewardContent");
			m_btnClaimAds = FindChildComponent<Button>("Content/m_tfSuccessContent/m_btnClaimAds");
			m_txtClaimAds = FindChildComponent<Text>("Content/m_tfSuccessContent/m_btnClaimAds/m_txtClaimAds");
			m_btnClaim = FindChildComponent<Button>("Content/m_tfSuccessContent/m_btnClaim");
			m_txtClaim = FindChildComponent<Text>("Content/m_tfSuccessContent/m_btnClaim/m_txtClaim");
			m_tfFailedContent = FindChild("Content/m_tfFailedContent");
			m_btnTryAgain = FindChildComponent<Button>("Content/m_tfFailedContent/m_btnTryAgain");
			m_txtTryAgain = FindChildComponent<Text>("Content/m_tfFailedContent/m_btnTryAgain/m_txtTryAgain");
			m_txtEnergy = FindChildComponent<Text>("Content/m_tfFailedContent/m_btnTryAgain/m_txtEnergy");
			m_btnBack = FindChildComponent<Button>("Content/m_tfFailedContent/m_btnBack");
			m_txtEncourage = FindChildComponent<Text>("Content/m_tfFailedContent/m_txtEncourage");
			m_imgUnit = FindChildComponent<Image>("Content/Unit_Player/m_imgUnit");
		}
		#endregion
	}
}