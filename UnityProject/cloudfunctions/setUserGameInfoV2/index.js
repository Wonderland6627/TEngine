// 云函数入口文件
const cloud = require('wx-server-sdk')
cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV }) // 使用当前云环境
const db = cloud.database()
const userGameInfos = db.collection('UserGameInfos')

// 云函数入口函数
exports.main = async (event, context) => {
  try {
    const wxContext = cloud.getWXContext()
    const OPENID = wxContext.OPENID
    const now = new Date()
    
    // 构建更新数据，只使用新字段（忽略废弃的 userGameInfo 字段）
    const updateData = {
      updatedAt: now,
    }
    
    // 只更新传入的字段
    if (event.progressLevelID !== undefined) {
      updateData.progressLevelID = event.progressLevelID
    }
    if (event.nickName !== undefined) {
      updateData.nickName = event.nickName
    }
    if (event.avatarUrl !== undefined) {
      updateData.avatarUrl = event.avatarUrl
    }
    if (event.openId !== undefined) {
      updateData.openId = event.openId
    }
    
    let hasData = await userGameInfos.where({ openid: OPENID }).get()
    if (hasData.data.length === 0) {
      // 创建新记录，使用新字段
      let addData = { 
        openid: OPENID,
        progressLevelID: event.progressLevelID || 0,
        nickName: event.nickName || "",
        avatarUrl: event.avatarUrl || "",
        openId: event.openId || OPENID,
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
    
    // 更新现有记录，使用新字段
    let updateResult = await userGameInfos.where({ openid: OPENID }).update({
      data: updateData,
    })
    return {
      code: 0,
      data: updateResult,
      msg: "update user game info success"
    }
  } catch (error) {
    return {
      code: -1,
      msg: error.message
    }
  }
}

