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
		private UIDailyChestBtn dailyChestBtn;
		private UILevelChestBtn[] levelChestBtns = new UILevelChestBtn[3];

		protected override void OnCreate()
		{
			base.OnCreate();
			Log.Info($"[UILevelView] OnCreate");

			m_txtEnergy.text = $"x{ConfigSystem.Instance.Tables.TbGlobalConfig.LevelEnergyConsume}";
			dailyChestBtn = CreateWidget<UIDailyChestBtn>(m_itemDailyChestBtn);
			
			// 初始化 3 个关卡激励奖励按钮
			levelChestBtns[0] = CreateWidget<UILevelChestBtn>(m_itemLevelChest_1);
			levelChestBtns[1] = CreateWidget<UILevelChestBtn>(m_itemLevelChest_2);
			levelChestBtns[2] = CreateWidget<UILevelChestBtn>(m_itemLevelChest_3);
			
			int nextLevelId = GetNextLevelId();
			RefreshLevelPreview(nextLevelId);

			EventTriggerListener.Get(m_itemRank).OnClick = go =>
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
				Log.Info($"[UILevelView] on click start game [{selectLevelId}]");
			};

			GameEvent.AddEventListener<int>(SlimeEvent.OnProgressLevelIDChanged, OnProgressLevelIDChanged);
		}

		protected override void OnSetVisible(bool visible)
		{
			base.OnSetVisible(visible);
			if (!visible) return;
			
			int nextLevelId = GetNextLevelId();
			Log.Info($"[UILevelView] OnSetVisible nextLevelId [{nextLevelId}]");
			RefreshLevelPreview(nextLevelId);
		}

		private void OnProgressLevelIDChanged(int newProgressLevelID)
		{
			int nextLevelId = GetNextLevelId();
			Log.Info($"[UILevelView] OnProgressLevelIDChanged: progress={newProgressLevelID}, refreshing to level {nextLevelId}");
			RefreshLevelPreview(nextLevelId);
		}

		protected override void OnDestroy()
		{
			GameEvent.RemoveEventListener<int>(SlimeEvent.OnProgressLevelIDChanged, OnProgressLevelIDChanged);
			base.OnDestroy();
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

			dailyChestBtn?.RefreshState();
			RefreshLevelChestBtns();

			Log.Info($"[UILevelView] refresh level preview [{previewLevelId}] progresslvid [{progressLevelID}] islocked [{isLocked}]");
		}

		/// <summary>
		/// 刷新关卡激励奖励按钮
		/// </summary>
		private void RefreshLevelChestBtns()
		{
			var displayChests = LevelChestManager.Instance.GetDisplayChests(selectLevelId);

			for (int i = 0; i < levelChestBtns.Length; i++)
			{
				levelChestBtns[i]?.RefreshState(displayChests[i]);
			}

			Log.Info($"[UILevelView] RefreshLevelChestBtns: anchorLevel={selectLevelId}, slots={displayChests.Count}");
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
		private GameObject m_itemRank;
		private Button m_btnRule;
		private GameObject m_itemDailyChestBtn;
		private Image m_imgLevelPreviewContent;
		private Image m_imgLevelPreviewBG;
		private Image m_imgLevelPreview;
		private Image m_imgLevelLock;
		private Image m_imgLevelNext;
		private Image m_imgLevelPrevious;
		private Image m_imgLevelBG;
		private Text m_txtLevel;
		private Button m_btnStart;
		private Text m_txtStart;
		private Text m_txtEnergy;
		private GameObject m_itemLevelChest_1;
		private GameObject m_itemLevelChest_2;
		private GameObject m_itemLevelChest_3;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("m_imgBG");
			m_btnSettings = FindChildComponent<Button>("layoutLeft/m_btnSettings");
			m_itemRank = FindChild("layoutLeft/m_itemRank").gameObject;
			m_btnRule = FindChildComponent<Button>("layoutRight/m_btnRule");
			m_itemDailyChestBtn = FindChild("layoutRight/m_itemDailyChestBtn").gameObject;
			m_imgLevelPreviewContent = FindChildComponent<Image>("m_imgLevelPreviewContent");
			m_imgLevelPreviewBG = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelPreviewBG");
			m_imgLevelPreview = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelPreview");
			m_imgLevelLock = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelLock");
			m_imgLevelNext = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelNext");
			m_imgLevelPrevious = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelPrevious");
			m_imgLevelBG = FindChildComponent<Image>("m_imgLevelPreviewContent/m_imgLevelBG");
			m_txtLevel = FindChildComponent<Text>("m_imgLevelPreviewContent/m_imgLevelBG/m_txtLevel");
			m_btnStart = FindChildComponent<Button>("m_btnStart");
			m_txtStart = FindChildComponent<Text>("m_btnStart/m_txtStart");
			m_txtEnergy = FindChildComponent<Text>("m_btnStart/m_txtEnergy");
			m_itemLevelChest_1 = FindChild("layoutBottom/m_itemLevelChest_1").gameObject;
			m_itemLevelChest_2 = FindChild("layoutBottom/m_itemLevelChest_2").gameObject;
			m_itemLevelChest_3 = FindChild("layoutBottom/m_itemLevelChest_3").gameObject;
		}
	}
#endregion === 复制结束 ===
}
