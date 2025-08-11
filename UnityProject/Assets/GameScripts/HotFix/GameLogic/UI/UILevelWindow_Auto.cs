using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UILevelWindow : UIWindow
	{
		private int selectLevelId = -1;
		private int cachedLevelId = -1;

		protected override void OnCreate()
		{
			base.OnCreate();
			Log.Info($"[UILevelWindow] OnCreate");
			
			int nextLevelId = GetNextLevelId();
			RefreshLevelPreview(nextLevelId);

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				Close();
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
					Log.Warning($"[UILevelWindow] select level id: -1");
					return;
				}

				World.Instance.StartGame(selectLevelId);
				cachedLevelId = GetNextLevelId();
            	Log.Info($"[UILevelWindow] on click start game [{selectLevelId}]");
			};
		}

        protected override void OnSetVisible(bool visible)
        {
            base.OnSetVisible(visible);
			if (visible)
			{
				int nextLevelId = GetNextLevelId();
				Log.Info($"[UILevelWindow] OnSetVisible nextLevelId [{nextLevelId}] cachedLevelId [{cachedLevelId}]");
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
			m_btnStart.interactable = !isLocked;
			EventTriggerListener.Get(m_btnStart).enabled = !isLocked;
			m_imgLevelPreview.color = isLocked? Color.black : Color.white;
			m_imgLevelLock.gameObject.SetActive(isLocked);
			m_textLevel.text = $"第 {previewLevelId} 关" + (isLocked ? " 未解锁" : "");
			
			m_imgLevelPrevious.gameObject.SetActive(previewLevelId > 1);
			m_imgLevelNext.gameObject.SetActive(previewLevelId < World.Instance.GetAllLevels().Count);

			Log.Info($"[UILevelWindow] refresh level preview [{previewLevelId}] progresslvid [{progressLevelID}] islocked [{isLocked}]");
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
	
	partial class UILevelWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Button m_btnBack;
		private Button m_btnRule;
		private Image m_imgLevelPreviewContent;
		private Image m_imgLevelPreviewBG;
		private Image m_imgLevelPreview;
		private Image m_imgLevelLock;
		private Image m_imgLevelTitleBG;
		private Text m_textLevelTitle;
		private Image m_imgLevelNext;
		private Image m_imgLevelPrevious;
		private Image m_imgLevelBG;
		private Text m_textLevel;
		private Button m_btnStart;
		private Text m_textStart;
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_btnBack = FindChildComponent<Button>("Content/m_btnBack");
			m_btnRule = FindChildComponent<Button>("Content/m_btnRule");
			m_imgLevelPreviewContent = FindChildComponent<Image>("Content/m_imgLevelPreviewContent");
			m_imgLevelPreviewBG = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelPreviewBG");
			m_imgLevelPreview = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelPreview");
			m_imgLevelLock = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelLock");
			m_imgLevelTitleBG = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelTitleBG");
			m_textLevelTitle = FindChildComponent<Text>("Content/m_imgLevelPreviewContent/m_imgLevelTitleBG/m_textLevelTitle");
			m_imgLevelNext = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelNext");
			m_imgLevelPrevious = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelPrevious");
			m_imgLevelBG = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelBG");
			m_textLevel = FindChildComponent<Text>("Content/m_imgLevelPreviewContent/m_imgLevelBG/m_textLevel");
			m_btnStart = FindChildComponent<Button>("Content/m_imgLevelPreviewContent/m_btnStart");
			m_textStart = FindChildComponent<Text>("Content/m_imgLevelPreviewContent/m_btnStart/m_textStart");
		}
		#endregion
	}
}