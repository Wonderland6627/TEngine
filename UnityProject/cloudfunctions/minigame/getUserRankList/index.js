// 云函数入口文件
const cloud = require('wx-server-sdk')
cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV })
const db = cloud.database()
const userGameInfos = db.collection('UserGameInfos')
const _ = db.command

exports.main = async (event, context) => {
  try {
    // 获取前100名玩家数据，按progressLevelID降序排序
    const result = await userGameInfos
      .orderBy('userGameInfo.progressLevelID', 'desc')
      .limit(100)
      .get()

    return {
      code: 0,
      data: result.data,
      msg: "get rank list success"
    }
  } catch (error) {
    return {
      code: -1,
      msg: error.message
    }
  }
}