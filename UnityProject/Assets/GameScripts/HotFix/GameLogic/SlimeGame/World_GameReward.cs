using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public partial class World
    {
        public float playerSlimeMoveSpeedCoe = 1f; // 玩家史莱姆移动速度系数
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
            Log.Info($"[World] reward action trigger: [{action.toString()}]");
            OnRewardTriggered(action);

            if (action.GetDuration() > 0)
            {
                activeRewardAction = action; // 只有持续的锦囊奖励 才会赋值
                actionRemoveTimer = GameModule.Timer.AddTimer((args) =>
                {
                    Log.Info($"[World] reward action remove: [{action.toString()}]");
                    ResetRewardAction();
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
            playerGetMoreSlimeAfterOccupy = false;
            GameEvent.Send(SlimeEvent.OnRewardSelect, null);
        }

        private void OnRewardTriggered(RewardAction action)
        {
            RewardType type = action.config.rewardType;
            switch (type)
            {
                case RewardType.ReduceEnemyCount:
                    {
                        BaseCastle enemyCastle = GetRandomOccupiedCastle(UnitType.Enemy_1);
                        if (enemyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castle, reward action: [{action.toString()}]");
                            return;
                        }
                        enemyCastle.ReduceOccupiedUnitCount((int)action.GetEffectValue());
                    }
                    break;
                case RewardType.AddFriendlyCount:
                    {
                        BaseCastle friendlyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (friendlyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castle, reward action: [{action.toString()}]");
                            return;
                        }
                        friendlyCastle.AddOccupiedUnitCount((int)action.GetEffectValue());
                    }
                    break;

                case RewardType.SpeedUpProduction:
                    {
                        BaseCastle friendlyCastle = GetRandomOccupiedCastle(UnitType.Player);
                        if (friendlyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no friendly castle, reward action: [{action.toString()}]");
                            return;
                        }
                    }
                    break;
                case RewardType.EnhanceMoveSpeed:
                    {
                        float newSpeedCoe = 1f + action.GetEffectValue() / 100f;
                        playerSlimeMoveSpeedCoe = newSpeedCoe;
                    }
                    break;
                case RewardType.OccupyRandomCastle:
                    {
                        BaseCastle emptyCastle = GetRandomEmptyCastle();
                        if (emptyCastle == null)
                        {
                            Log.Error($"[World] OnRewardTriggered: no empty castle, reward action: [{action.toString()}]");
                            return;
                        }
                        emptyCastle.OccupiedBy(UnitType.Player);
                    }
                    break;
                case RewardType.InstantArmyBoost:
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
                    }
                    break;
                    case RewardType.EnemyArmyDisperse:
                    {
                        List<BaseCastle> enemyCastles = GetAllOccupiedCastles(UnitType.Enemy_1);
                        if (enemyCastles.Count == 0)
                        {
                            Log.Error($"[World] OnRewardTriggered: no enemy castles, reward action: [{action.toString()}]");
                            return;
                        }
                        for (int i = 0; i < enemyCastles.Count; i++)
                        {
                            float ratio = action.GetEffectValue() / 100f;
                            enemyCastles[i].ReduceOccupiedUnitCount(ratio);
                        }
                    }
                    break;
                case RewardType.ChainOccupation:
                    {
                       playerGetMoreSlimeAfterOccupy = true;
                    }
                    break;
            }
        }

        private BaseCastle GetRandomEmptyCastle()
        {
            return castles.Find(c => !c.isOccupied);
        }

        private List<BaseCastle> GetAllOccupiedCastles(UnitType unitType)
        {
            return castles.FindAll(c => c.isOccupied && c.occupiedUnitType == unitType);
        }

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
