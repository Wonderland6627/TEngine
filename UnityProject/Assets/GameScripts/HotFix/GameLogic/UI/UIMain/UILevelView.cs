using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
	partial class UILevelView
	{
		private int selectLevelId = -1;
		private int cachedLevelId = -1;

		protected override void OnCreate()
		{
			base.OnCreate();
			Log.Info($"[UILevelView] OnCreate");
			
			int nextLevelId = GetNextLevelId();
			RefreshLevelPreview(nextLevelId);
			EventTriggerListener.Get(m_btnRank).OnClick = go =>
			{
				GameModule.UI.ShowUIAsync<UIRankWindow>();
			};
			EventTriggerListener.Get(m_btnSettings).OnClick = go =>
			{
				GameModule.UI.ShowUIAsync<UISettingsWindow>();
			};
			EventTriggerListener.Get(m_btnRule).OnClick = go =>
			{
				GameModule.UI.ShowUIAsync<UIRuleTipsWindow>(false);
			};
			EventTriggerListener.Get(m_imgLevelNext.gameObject).OnClick = go =>
			{
				OnLevelNextClick(true);
			};
			EventTriggerListener.Get(m_imgLevelPrevious.gameObject).OnClick = go =>
			{
				OnLevelNextClick(false);
			};
			EventTriggerListener.Get(m_btnStart).OnClick = go =>
			{
				if (selectLevelId == -1)
				{
					Log.Warning($"[UILevelView] select level id: -1");
					return;
				}

				World.Instance.StartGame(selectLevelId);
				cachedLevelId = GetNextLevelId();
				Log.Info($"[UILevelView] on click start game [{selectLevelId}]");
			};
		}

		protected override void OnSetVisible(bool visible)
		{
			base.OnSetVisible(visible);
			if (visible)
			{
				int nextLevelId = GetNextLevelId();
				Log.Info($"[UILevelView] OnSetVisible nextLevelId [{nextLevelId}] cachedLevelId [{cachedLevelId}]");
				if (nextLevelId == cachedLevelId) return;
				RefreshLevelPreview(nextLevelId);
			}
		}

		private void OnLevelNextClick(bool isNext)
		{
			int previewLevelId = isNext ? selectLevelId + 1 : selectLevelId - 1;
			RefreshLevelPreview(previewLevelId);
		}

		private void RefreshLevelPreview(int previewLevelId)
		{
			selectLevelId = previewLevelId;

			int progressLevelID = World.Instance.GameData.ProgressLevelID;
			bool isLocked = previewLevelId > progressLevelID + 1;
			EventTriggerListener.Get(m_btnStart).SetInteractable(!isLocked);
			m_imgLevelPreview.color = isLocked ? Color.black : Color.white;
			m_imgLevelLock.gameObject.SetActive(isLocked);
			m_txtLevel.text = $"第 {previewLevelId} 关" + (isLocked ? " 未解锁" : "");
			m_imgLevelPrevious.gameObject.SetActive(previewLevelId > 1);
			m_imgLevelNext.gameObject.SetActive(previewLevelId < World.Instance.GetAllLevels().Count);

			Log.Info($"[UILevelView] refresh level preview [{previewLevelId}] progresslvid [{progressLevelID}] islocked [{isLocked}]");
		}
		
		private int GetNextLevelId()
		{
			int progressLevelID = World.Instance.GameData.ProgressLevelID; //当前最高通关关卡
			int previewLevelId = progressLevelID + 1; //下一关关卡
			var lvlConfigs = World.Instance.GetAllLevels();
			if (lvlConfigs.Count <= previewLevelId)
			{
				previewLevelId = lvlConfigs.Count;
			}
			return previewLevelId;
		}
	}

#region 脚本工具生成的代码 === 复制开始 ===
	partial class UILevelView : UIWidget
	{
		private Image m_imgBG;
		private Button m_btnSettings;
		private Button m_btnRank;
		private Button m_btnRule;
		private Button m_btnCheckIn;
		private Image m_imgLevelPreviewContent;
		private Image m_imgLevelPreviewBG;
		private Image m_imgLevelPreview;
		private Image m_imgLevelLock;
		private Image m_imgLevelNext;
		private Image m_imgLevelPrevious;
		private Image m_imgLevelBG;
		private Text m_txtLevel;
		private Button m_btnStart;
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("m_imgBG");
			m_btnSettings = FindChildComponent<Button>("layoutLeft/m_btnSettings");
			m_btnRank = FindChildComponent<Button>("layoutLeft/m_btnRank");
			m_btnRule = FindChildComponent<Button>("layoutRight/m_btnRule");
			m_btnCheckIn = FindChildComponent<Button>("layoutRight/m_btnCheckIn");
			m_imgLevelPreviewContent = FindChildComponent<Image>("m_imgLevelPreviewContent");
			m_imgLevelPreviewBG = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelPreviewBG");
			m_imgLevelPreview = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelPreview");
			m_imgLevelLock = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelLock");
			m_imgLevelNext = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelNext");
			m_imgLevelPrevious = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelPrevious");
			m_imgLevelBG = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelBG");
			m_txtLevel = FindChildComponent<Text>("m_imgLevelPreviewContent/m_imgLevelBG/m_txtLevel");
			m_btnStart = FindChildComponent<Button>("m_btnStart");
		}
	}
#endregion === 复制结束 ===
}
