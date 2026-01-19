using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 流程 => 用户尝试更新静态版本
    /// </summary>
    public class ProcedureUpdateVersion : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;

            base.OnEnter(procedureOwner);

            UILoadMgr.Show(UIDefine.UILoadUpdate, $"更新静态版本文件...");

            //检查设备是否能够访问互联网
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Warning("The device is not connected to the network");
                UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Net_UnReachable);
                UILoadTip.ShowMessageBox(LoadText.Instance.Label_Net_UnReachable, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry,
                    GetStaticVersion().Forget,
                    () => { ChangeState<ProcedureInitResources>(procedureOwner); });
            }

            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_RequestVersionIng);

            // 用户尝试更新静态版本。
            GetStaticVersion().Forget();
        }

        /// <summary>
        /// 向用户尝试更新静态版本。
        /// </summary>
        private async UniTaskVoid GetStaticVersion()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            Log.Info($"[ResUpdate-1] [ProcedureUpdateVersion] GetStaticVersion UpdatePackageVersionAsync, GameModule.Resource.GetPackageVersion(): [{GameModule.Resource.GetPackageVersion()}], GameModule.Resource.PackageVersion: [{GameModule.Resource.PackageVersion}]");
            // 使用 appendTimeTicks: true 避免 CDN 缓存问题
            var operation = GameModule.Resource.UpdatePackageVersionAsync(appendTimeTicks: true);

            try
            {
                Log.Info($"[ResUpdate-1.1] [ProcedureUpdateVersion] after UpdatePackageVersionAsync init");
                await operation.ToUniTask();
                Log.Info($"[ResUpdate-1.2] [ProcedureUpdateVersion] after UpdatePackageVersionAsync task");

                if (operation.Status == EOperationStatus.Succeed)
                {
                    Log.Info($"[ResUpdate-1.3] [ProcedureUpdateVersion] after UpdatePackageVersionAsync succeed, operation.PackageVersion: [{operation.PackageVersion}]");
                    //线上最新版本operation.PackageVersion
                    GameModule.Resource.PackageVersion = operation.PackageVersion;
                    Log.Debug($"Updated package Version : from {GameModule.Resource.GetPackageVersion()} to {operation.PackageVersion}");
                    
                    // 更新 RemoteServices 的版本号，以便后续 bundle 请求使用正确的版本目录
                    // 注意：RemoteServices 会自动从 package 获取版本号，这里手动更新以确保及时性
                    GameModule.Resource.UpdateRemoteServicesVersion(GameModule.Resource.PackageName, operation.PackageVersion);
                    
                    ChangeState<ProcedureUpdateManifest>(_procedureOwner);
                }
                else
                {
                    OnGetStaticVersionError(operation.Error);
                }
            }
            catch (Exception e)
            {
                OnGetStaticVersionError(e.Message);
            }
        }

        private void OnGetStaticVersionError(string error)
        {
            Log.Error(error);

            UILoadTip.ShowMessageBox($"用户尝试更新静态版本失败！点击确认重试 \n \n <color=#FF0000>原因{error}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { ChangeState<ProcedureUpdateVersion>(_procedureOwner); }, UnityEngine.Application.Quit);
        }
    }
}