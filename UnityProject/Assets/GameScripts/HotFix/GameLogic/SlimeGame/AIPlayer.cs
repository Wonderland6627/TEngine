using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Execute", 1f, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Execute()
    {
        List<BaseCastle> aiCastles = World.Instance.castles.FindAll(castle => castle.occupiedSlimeType != SlimeType.Player);
        for (int i = 0; i < aiCastles.Count; i++)
        {
            BaseCastle aiCastle = aiCastles[i];
            if (aiCastle == null)
            {
                continue;
            }
            if (aiCastle.occupiedUnitCount < 5)
            {
                continue;
            }

            //蚂蚁首先找最近的蜜蜂点位 找到路径 攻占路径上的点位
            List<BaseCastle> playerCastles = World.Instance.castles
                .FindAll(castle => castle != aiCastle && castle.occupiedSlimeType == SlimeType.Player || !castle.isOccupied)
                .OrderBy(castle => Vector2.Distance(castle.transform.position, aiCastle.transform.position))
                .ToList();
            foreach (var playerCastle in playerCastles)
            {
                if (!World.Instance.IsOnSameRoad(aiCastle, playerCastle))
                {
                    //找交叉点
                    var allCrossCastles = World.Instance.castles.FindAll(castle =>
                        World.Instance.IsOnSameRoad(aiCastle, castle) &&
                        World.Instance.IsOnSameRoad(playerCastle, castle));
                    var nearestCrossCastle = allCrossCastles
                       .OrderBy(castle => Vector2.Distance(castle.transform.position, aiCastle.transform.position))
                       .FirstOrDefault();
                    if (nearestCrossCastle != null)
                    {
                        aiCastle.MoveTo(nearestCrossCastle);
                        continue;
                    }
                }
                aiCastle.MoveTo(playerCastle);
            }
        }
    }
}
