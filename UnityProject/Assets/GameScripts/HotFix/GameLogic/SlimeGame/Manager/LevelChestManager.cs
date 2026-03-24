using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameBase;
using GameConfig;
using GameLogic.Network;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 推关激励宝箱管理器
    /// 负责配置加载、滑动窗口显示、领取逻辑和状态同步
    /// </summary>
    public class LevelChestManager : Singleton<LevelChestManager>
    {
        private const string KEY_CLAIMED_CACHE = "LevelChest_Claimed";

        private List<LevelChest> _allMilestones;
        private HashSet<int> _claimedSet = new();

        public bool IsLoaded => _allMilestones != null && _allMilestones.Count > 0;

        /// <summary>
        /// 从 ConfigSystem 加载 TbLevelChest 配置
        /// </summary>
        public void Init()
        {
            var dataList = ConfigSystem.Instance.Tables.TbLevelChest.DataList;
            _allMilestones = new List<LevelChest>(dataList);
            _allMilestones.Sort((a, b) => a.LevelId.CompareTo(b.LevelId));

            LoadClaimedCache();
            Log.Info($"[LevelChest] Init: {_allMilestones.Count} milestones loaded, {_claimedSet.Count} claimed");
        }

        /// <summary>
        /// 从 FetchUserGameInfo 同步已领取数据
        /// </summary>
        public void SyncClaimedData(List<int> claimedLevelChests)
        {
            _claimedSet.Clear();
            if (claimedLevelChests != null)
            {
                foreach (var id in claimedLevelChests)
                {
                    _claimedSet.Add(id);
                }
            }
            SaveClaimedCache();
            Log.Info($"[LevelChest] SyncClaimedData: {_claimedSet.Count} claimed");
        }

        #region 查询

        public LevelChestState GetChestState(int chestLevelId)
        {
            if (_claimedSet.Contains(chestLevelId)) return LevelChestState.Claimed;

            int progress = World.Instance.GameData.ProgressLevelID;
            if (progress >= chestLevelId) return LevelChestState.Claimable;

            return LevelChestState.Locked;
        }

        public bool CanClaim(int chestLevelId)
        {
            return GetChestState(chestLevelId) == LevelChestState.Claimable;
        }

        public bool IsClaimed(int chestLevelId)
        {
            return _claimedSet.Contains(chestLevelId);
        }

        /// <summary>
        /// 从 Luban TbReward 读取奖励预览
        /// </summary>
        public RewardParam GetRewardPreview(int chestLevelId)
        {
            if (_allMilestones == null) return new RewardParam();

            var milestone = _allMilestones.Find(m => m.LevelId == chestLevelId);
            if (milestone == null) return new RewardParam();

            var reward = ConfigSystem.Instance.Tables.TbReward.GetOrDefault(milestone.RewardId);
            if (reward == null) return new RewardParam();

            var param = new RewardParam();
            foreach (var entry in reward.RewardItems)
            {
                param.AddReward(entry.ItemType, entry.ItemId, entry.Amount);
            }
            return param;
        }

        #endregion

        #region 滑动窗口

        private const int DisplayChestSlotCount = 3;

        /// <summary>
        /// 选关界面滑动窗口：以 anchorLevelId 为锚取相邻 3 个里程碑；返回列表固定 3 个元素，槽位无配置时为 null。
        /// </summary>
        public List<LevelChestDisplayInfo> GetDisplayChests(int anchorLevelId)
        {
            var result = new List<LevelChestDisplayInfo>(DisplayChestSlotCount);
            for (int i = 0; i < DisplayChestSlotCount; i++)
            {
                result.Add(null);
            }

            if (_allMilestones == null || _allMilestones.Count == 0)
            {
                return result;
            }

            int n = _allMilestones.Count;
            if (n == 1)
            {
                result[1] = BuildDisplayInfo(_allMilestones[0]);
                return result;
            }

            if (n == 2)
            {
                result[0] = BuildDisplayInfo(_allMilestones[0]);
                result[1] = BuildDisplayInfo(_allMilestones[1]);
                return result;
            }

            // 中心：首个 LevelId >= 锚点；若全部小于锚点则中心为最后一项，再取连续 3 格并贴边裁剪
            int center = n - 1;
            for (int i = 0; i < n; i++)
            {
                if (_allMilestones[i].LevelId >= anchorLevelId)
                {
                    center = i;
                    break;
                }
            }

            int start = Math.Max(0, Math.Min(center - 1, n - DisplayChestSlotCount));
            for (int i = 0; i < DisplayChestSlotCount; i++)
            {
                result[i] = BuildDisplayInfo(_allMilestones[start + i]);
            }

            return result;
        }

        private LevelChestDisplayInfo BuildDisplayInfo(LevelChest cfg)
        {
            if (cfg == null) return null;

            return new LevelChestDisplayInfo
            {
                config = cfg,
                state = GetChestState(cfg.LevelId),
            };
        }

        #endregion

        #region 领取

        /// <summary>
        /// 向服务端请求领取宝箱
        /// </summary>
        public async UniTask<bool> ClaimChest(int chestLevelId)
        {
            if (!CanClaim(chestLevelId))
            {
                Log.Warning($"[LevelChest] Cannot claim chest: chestLevelId={chestLevelId}, state={GetChestState(chestLevelId)}");
                return false;
            }

            var response = await NetManager.Instance.CallHttp<ClaimLevelChestResponse>(
                "claimLevelChest",
                new { chestLevelId }
            );

            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[LevelChest] ClaimChest failed: {response.ErrorMessage}");
                return false;
            }

            // 同步已领取列表
            if (response.data.claimedLevelChests != null)
            {
                SyncClaimedData(response.data.claimedLevelChests);
            }
            else
            {
                _claimedSet.Add(chestLevelId);
                SaveClaimedCache();
            }

            // 同步资源和物品
            World.Instance.SyncResources(response.data.resources);
            World.Instance.SyncGoods(response.data.goods);

            GameEvent.Send(SlimeEvent.OnLevelChestClaimed, chestLevelId);
            Log.Info($"[LevelChest] Claimed chest: chestLevelId={chestLevelId}");
            return true;
        }

        #endregion

        #region 本地缓存

        private void LoadClaimedCache()
        {
            var cached = PlayerPrefs.GetString(KEY_CLAIMED_CACHE, "");
            if (string.IsNullOrEmpty(cached)) return;

            foreach (var s in cached.Split(','))
            {
                if (int.TryParse(s, out int id))
                {
                    _claimedSet.Add(id);
                }
            }
        }

        private void SaveClaimedCache()
        {
            var str = string.Join(",", _claimedSet);
            PlayerPrefs.SetString(KEY_CLAIMED_CACHE, str);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
