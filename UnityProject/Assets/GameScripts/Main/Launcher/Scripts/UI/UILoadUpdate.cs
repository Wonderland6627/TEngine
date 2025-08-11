using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Version = TEngine.Version;

namespace GameMain
{
    [Window(UILayer.UI, fromResources: true, location: "AssetLoad/UILoadUpdate", fullScreen: true)]
    public class UILoadUpdate : UIWindow
    {
		#region 脚本工具生成的代码
		private Image m_imgBackGround;
		private Text m_textDesc;
		private Button m_btnClear;
		private Text m_textAppid;
		private Text m_textResid;
        private Scrollbar m_scrollbarProgress;
		private Button m_btnGM1;
		private Button m_btnGM2;
		protected override void ScriptGenerator()
		{
			m_imgBackGround = FindChildComponent<Image>("m_imgBackGround");
			m_textDesc = FindChildComponent<Text>("m_textDesc");
			m_btnClear = FindChildComponent<Button>("TopNode/m_btnClear");
			m_textAppid = FindChildComponent<Text>("TopNode/m_textAppid");
			m_textResid = FindChildComponent<Text>("TopNode/m_textResid");
            m_scrollbarProgress = FindChildComponent<Scrollbar>("m_scrollbarProgress");
			m_btnGM1 = FindChildComponent<Button>("m_btnGM1");
			m_btnGM2 = FindChildComponent<Button>("m_btnGM2");
			m_btnClear.onClick.AddListener(OnClickClearBtn);
			m_btnGM1.onClick.AddListener(OnClickGM1Btn);
			m_btnGM2.onClick.AddListener(OnClickGM2Btn);
		}
		#endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            LoadUpdateLogic.Instance.DownloadCompleteAction += DownLoad_Complete_Action;
            LoadUpdateLogic.Instance.DownProgressAction += DownLoad_Progress_Action;
            LoadUpdateLogic.Instance.UnpackedCompleteAction += Unpacked_Complete_Action;
            LoadUpdateLogic.Instance.UnpackedProgressAction += Unpacked_Progress_Action;
            m_btnClear.gameObject.SetActive(true);
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            AddUIEvent(RuntimeId.ToRuntimeId("RefreshVersion"), RefreshVersion);
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            if (base.userDatas.Length > 0 && base.userDatas[0] != null)
            {
                m_textDesc.text = base.userDatas[0].ToString(); //获取更新内容，由ProcedureDownloadFile -> OnDownloadProgressCallback 传递
            }
        }

        #region 事件

        private void OnClickClearBtn()
        {
            OnStop(null);
            UILoadTip.ShowMessageBox(LoadText.Instance.Label_Clear_Comfirm, MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Clear,
                () =>
                {
                    GameModule.Resource.ClearSandbox();
                    Application.Quit();
                }, () => { OnContinue(null); });
        }

        private int gm1ClickCount = 0;
        private int gm2ClickCount = 0;

        private void OnClickGM1Btn()
        {
            gm1ClickCount++;
            CheckGMVisibleState();
        }

        private void OnClickGM2Btn()
        {
            gm2ClickCount++;
            CheckGMVisibleState();
        }

        private void CheckGMVisibleState()
        {
            int visibleCount = 6;
            if (gm1ClickCount + gm2ClickCount >= visibleCount)
            {
                GameModule.Debugger.ActiveWindow = !GameModule.Debugger.ActiveWindow;
                gm1ClickCount = 0;
                gm2ClickCount = 0;
            }
        }

        #endregion

        private void RefreshVersion()
        {
            m_textAppid.text = string.Format(LoadText.Instance.Label_App_id, Version.GameVersion);
            m_textResid.text = string.Format(LoadText.Instance.Label_Res_id, GameModule.Resource.GetPackageVersion());
        }

        public virtual void OnContinue(GameObject obj)
        {
            // LoadMgr.Instance.StartDownLoad();
        }

        public virtual void OnStop(GameObject obj)
        {
            // LoadMgr.Instance.StopDownLoad();
        }

        /// <summary>
        /// 下载进度完成
        /// </summary>
        /// <param name="type"></param>
        protected virtual void DownLoad_Complete_Action(int type)
        {
            Log.Info("DownLoad_Complete");
        }

        /// <summary>
        /// 下载进度更新
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void DownLoad_Progress_Action(float progress)
        {
            m_scrollbarProgress.gameObject.SetActive(true);

            m_scrollbarProgress.size = progress;
        }

        /// <summary>
        /// 解压缩完成回调
        /// </summary>
        /// <param name="type"></param>
        protected virtual void Unpacked_Complete_Action(bool type)
        {
            m_scrollbarProgress.gameObject.SetActive(true);
            m_textDesc.text = LoadText.Instance.Label_Load_UnpackComplete;
        }

        /// <summary>
        /// 解压缩进度更新
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void Unpacked_Progress_Action(float progress)
        {
            m_scrollbarProgress.gameObject.SetActive(true);
            m_textDesc.text = LoadText.Instance.Label_Load_Unpacking;

            m_scrollbarProgress.size = progress;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnStop(null);
            LoadUpdateLogic.Instance.DownloadCompleteAction -= DownLoad_Complete_Action;
            LoadUpdateLogic.Instance.DownProgressAction -= DownLoad_Progress_Action;
        }
    }
}