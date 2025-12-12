// 云函数入口文件
const cloud = require('wx-server-sdk')
cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV }) // 使用当前云环境
const db = cloud.database()
const userGameInfos = db.collection('UserGameInfos')

// 云函数入口函数
exports.main = async (event, context) => {
  const wxContext = cloud.getWXContext()
  const OPENID = wxContext.OPENID
  const now = new Date()
  let hasData = await userGameInfos.where({ openid: OPENID }).get()
  if (hasData.data.length === 0) {
    let addData = { 
      openid: OPENID,
      userGameInfo: event,
      createdAt: now,
      updatedAt: now,
    }
    let isAdd = await userGameInfos.add({ data: addData });
    return {
      code: 0,
      data: addData,
      msg: "no result found, add info",
    }
  }
  return await userGameInfos.where({ openid: wxContext.OPENID }).update({
    data: {
      userGameInfo: event,
      updatedAt: now,
    },
  })
}