// 云函数入口文件
const cloud = require('wx-server-sdk')
cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV })
const db = cloud.database()
const userGameInfos = db.collection('UserGameInfos')
const _ = db.command

exports.main = async (event, context) => {
  try {
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

    return {
      code: 0,
      data: result.data,
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

