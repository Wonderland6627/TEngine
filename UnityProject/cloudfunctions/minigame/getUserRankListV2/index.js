// 云函数入口文件
const cloud = require('wx-server-sdk')
cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV })
const db = cloud.database()
const userGameInfos = db.collection('UserGameInfos')
const _ = db.command

exports.main = async (event, context) => {
  try {
    const currentOpenID = cloud.getWXContext().OPENID
    
    // 获取前100名玩家数据，按新字段结构 progressLevelID 降序排序
    // 筛选条件：progressLevelID 必须大于 0，nickName 必须存在且不为空字符串
    const result = await userGameInfos
      .where({
        progressLevelID: _.gt(0),
        nickName: _.and(_.exists(true), _.neq(""))
      })
      .orderBy('progressLevelID', 'desc')
      .limit(100)
      .get()

    let rankList = result.data || []
    
    const selfInList = rankList.some(item => item.openID === currentOpenID)
    if (!selfInList) {
      // 如果自己不在前100名，查询自己的数据
      const selfResult = await userGameInfos
        .where({
          openID: currentOpenID
        })
        .get()
      
      if (selfResult.data && selfResult.data.length > 0) {
        const selfData = selfResult.data[0]
        const selfProgressLevelID = selfData.progressLevelID || 0
        
        // 判断自己是否满足前100的条件
        // 条件：progressLevelID > 0
        // 如果已有100条数据，则自己的 progressLevelID 需要 >= 第100名的 progressLevelID
        let shouldInclude = false
        
        if (selfProgressLevelID > 0) {
          if (rankList.length < 100) {
            // 少于100条，满足基本条件即可加入
            shouldInclude = true
          } else {
            // 已有100条，检查自己的 progressLevelID 是否 >= 第100名的 progressLevelID
            const lastRankProgressLevelID = rankList[rankList.length - 1].progressLevelID || 0
            if (selfProgressLevelID >= lastRankProgressLevelID) {
              shouldInclude = true
            }
          }
        }
        
        if (shouldInclude) {
          if (rankList.length < 100) {
            // 少于100条，直接加入
            rankList.push(selfData)
          } else {
            // 已有100条，替换最后一条
            rankList[rankList.length - 1] = selfData
          }
        }
      }
    }

    return {
      code: 0,
      data: rankList,
      msg: "get rank list v2 success"
    }
  } catch (error) {
    return {
      code: -1,
      data: [],
      msg: error.message
    }
  }
}

