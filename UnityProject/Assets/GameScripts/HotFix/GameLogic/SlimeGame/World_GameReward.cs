using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public partial class World
    {
        public float playerSlimeMoveSpeedCoe = 1f; // 玩家史莱姆移动速度系数
        public float enemySlimeMoveSpeedCoe = 1f; // 敌人史莱姆移动速度系数
        public float playerSlimeSpawnSpeedCoe = 1f; // 玩家史莱姆生成速度系数
        public float enemySlimeSpawnSpeedCoe = 1f; // 敌人史莱姆生成速度系数
        public bool playerGetMoreSlimeAfterOccupy = false; // 玩家占领城堡后是否获得更多史莱姆
        public SlimeReward slimeReward { get; private set; } = new();

        public RewardAction activeRewardAction { get; private set; }
        private int actionRemoveTimer = -1;

        public List<RewardAction> GetRewardActions()
        {
            return slimeReward.GetRewardActions(rewardReader.configs);
        }

        public void TryTriggerReward(RewardAction action)
        {
            if (action == null)
            {
                Log.Error("[World] RewardAction is null");
                return;
            }

            if (action.NeedAds())
            {
                //todo: show ads
            }

            GameEvent.Send(SlimeEvent.OnRewardSelect, action);
            OnRewardTriggered(action);

            if (action.GetDuration() > 0)
            {
                activeRewardAction = action; // 只有持续的锦囊奖励 才会赋值
                actionRemoveTimer = GameModule.Timer.AddTimer((args) =>
                {
                    Log.Info($"[World] reward action remove: [{action.toString()}]");
                    ResetRewardAction();
                    Log.Info($"[World] reward action remove end, reward status: [{GetRewardStatus()}]");
                }, action.GetDuration());
            }
        }

        private void ResetRewardAction()
        {
            if (actionRemoveTimer != -1)
            {
                GameModule.Timer.RemoveTimer(actionRemoveTimer);
                actionRemoveTimer = -1;
            }
            activeRewardAction = null;
            playerSlimeMoveSpeedCoe = 1f;
            enemySlimeMoveSpeedCoe = 1f;
            playerSlimeSpawnSpeedCoe = 1f;
            enemySlimeSpawnSpeedCoe = 1f;
            playerGetMoreSlimeAfterOccupy = false;
            GameEvent.Send(SlimeEvent.OnRewardSelect, null);
        }

        private string GetRewardStatus()
        {
            return $"player move speed coe: {playerSlimeMoveSpeedCoe}, enemy move speed coe: {enemySlimeMoveSpeedCoe}, player spawn speed coe: {playerSlimeSpawnSpeedCoe}, enemy spawn speed coe: {enemySlimeSpawnSpeedCoe}, player get more slime after occupy: {playerGetMoreSlimeAfterOccupy}";
        }

        private void OnRewardTriggered(RewardAction action)
        {
            RewardType type = action.config.rewardType;
            switch (type)
            {
                case RewardType.AddSingleCastleSlime:
                    {
                        BaseCastle friendlyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (friendlyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castle, reward action: [{action.toString()}]");
                            return;
                        }
                        friendlyCastle.AddOccupiedUnitCount((int)action.GetEffectValue());
                    } break;
                case RewardType.ReduceSingleCastleSlime:
                    {
                        BaseCastle enemyCastle = GetRandomOccupiedCastle(UnitType.Enemy_1);
                        if (enemyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castle, reward action: [{action.toString()}]");
                            return;
                        }
                        enemyCastle.ReduceOccupiedUnitCount((int)action.GetEffectValue());
                    } break;
                case RewardType.AddSingleCastleSlimePercent:
                    {
                        BaseCastle friendlyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (friendlyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castle, reward action: [{action.toString()}]");
                            return;
                        }
                        float ratio = action.GetEffectValue() / 100f;
                        friendlyCastle.AddOccupiedUnit(ratio);
                    } break;
                case RewardType.ReduceSingleCastleSlimePercent:
                    {
                        BaseCastle enemyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (enemyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castle, reward action: [{action.toString()}]");
                            return;
                        }
                        float ratio = action.GetEffectValue() / 100f;
                        enemyCastle.ReduceOccupiedUnit(ratio);
                    } break;
                case RewardType.AddAllCastleSlimes:
                    {
                        List<BaseCastle> friendlyCastles = GetAllOccupiedCastles(UnitType.Player);
                        if (friendlyCastles.Count == 0)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castles, reward action: [{action.toString()}]");
                            return;
                        }
                        for (int i = 0; i < friendlyCastles.Count; i++)
                        {
                            friendlyCastles[i].AddOccupiedUnitCount((int)action.GetEffectValue());
                        }
                    } break;
                case RewardType.ReduceAllCastleSlimes:
                    {
                        List<BaseCastle> enemyCastles = GetAllOccupiedCastles(UnitType.Enemy_1);
                        if (enemyCastles.Count == 0)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castles, reward action: [{action.toString()}]");
                            return;
                        }
                        for (int i = 0; i < enemyCastles.Count; i++)
                        {
                            enemyCastles[i].ReduceOccupiedUnitCount((int)action.GetEffectValue());
                        }
                    } break;
                case RewardType.AddAllCastleSlimesPercent:
                    {
                        List<BaseCastle> friendlyCastles = GetAllOccupiedCastles(UnitType.Player);
                        if (friendlyCastles.Count == 0)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castles, reward action: [{action.toString()}]");
                            return;
                        }
                        float ratio = action.GetEffectValue() / 100f;
                        for (int i = 0; i < friendlyCastles.Count; i++)
                        {
                            friendlyCastles[i].AddOccupiedUnit(ratio);
                        }
                    } break;
                case RewardType.ReduceAllCastleSlimesPercent:
                    {
                        List<BaseCastle> enemyCastles = GetAllOccupiedCastles(UnitType.Enemy_1);
                        if (enemyCastles.Count == 0)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castles, reward action: [{action.toString()}]");
                            return;
                        }
                        float ratio = action.GetEffectValue() / 100f;
                        for (int i = 0; i < enemyCastles.Count; i++)
                        {
                            enemyCastles[i].ReduceOccupiedUnit(ratio);
                        }
                    } break;
                case RewardType.IncreaseSlimeSpawnSpeed:
                    {
                        BaseCastle friendlyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (friendlyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castle, reward action: [{action.toString()}]");
                            return;
                        }
                        playerSlimeSpawnSpeedCoe = 1f * (1 + action.GetEffectValue() / 100f);
                    } break;
                case RewardType.DecreaseSlimeEnemySpawnSpeed:
                    {
                        BaseCastle enemyCastle = GetRandomOccupiedCastle(UnitType.Enemy_1);
                        if (enemyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castle, reward action: [{action.toString()}]");
                            return;
                        }
                        enemySlimeSpawnSpeedCoe = 1f * (1 - action.GetEffectValue() / 100f);
                    } break;
                case RewardType.IncreaseSlimeMoveSpeed:
                    {
                        BaseCastle friendlyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (friendlyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castle, reward action: [{action.toString()}]");
                            return;
                        }
                        playerSlimeMoveSpeedCoe = 1f * (1 + action.GetEffectValue() / 100f);
                    } break;
                case RewardType.DecreaseSlimeEnemyMoveSpeed:
                    {
                        BaseCastle enemyCastle = GetRandomOccupiedCastle(UnitType.Enemy_1);
                        if (enemyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castle, reward action: [{action.toString()}]");
                            return;
                        }
                        enemySlimeMoveSpeedCoe = 1f * (1 - action.GetEffectValue() / 100f);
                    } break;
                case RewardType.ChainOccupationBonus:
                    {
                        playerGetMoreSlimeAfterOccupy = true;
                    } break;
                case RewardType.OccupyRandomCastle:
                    {
                        BaseCastle emptyCastle = GetRandomEmptyCastle();
                        if (emptyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no empty castle, reward action: [{action.toString()}]");
                            return;
                        }
                        emptyCastle.DirectOccupiedBy(UnitType.Player);
                    } break;
            }
            
            Log.Info($"[World] reward action trigger: [{action.toString()}], reward status: [{GetRewardStatus()}]");
        }

        // 获取随机一个空城堡
        private BaseCastle GetRandomEmptyCastle()
        {
            return castles.Find(c => !c.isOccupied);
        }

        // 获取所有被unitType类型占领的城堡
        private List<BaseCastle> GetAllOccupiedCastles(UnitType unitType)
        {
            return castles.FindAll(c => c.isOccupied && c.occupiedUnitType == unitType);
        }

        // 获取随机一个被unitType类型占领的城堡
        private BaseCastle GetRandomOccupiedCastle(UnitType unitType)
        {
            List<BaseCastle> list = castles.FindAll(c => c.isOccupied && c.occupiedUnitType == unitType);
            if (list.Count == 0)
            {
                return null;
            }

            return list[Random.Range(0, list.Count)];
        }
    }
}
