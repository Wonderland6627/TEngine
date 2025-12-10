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
    const nowISO = now.toISOString() // 用于返回的 ISO 字符串
    
    // 构建更新数据，只使用新字段（忽略废弃的 userGameInfo 字段）
    const updateData = {
      updatedAt: now, // 数据库存储用 Date 对象
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
      // 创建新记录，使用新字段（数据库存储用 Date 对象）
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
      // 返回时使用 ISO 字符串格式
      return {
        code: 0,
        data: {
          ...addData,
          createdAt: nowISO,
          updatedAt: nowISO,
        },
        msg: "no result found, add info",
      }
    }
    
    // 更新现有记录，使用新字段
    let updateResult = await userGameInfos.where({ openid: OPENID }).update({
      data: updateData,
    })
    // updateResult 通常只包含统计信息，不包含日期字段，但为了安全起见，如果包含日期则转换
    const formattedResult = updateResult
    if (formattedResult && typeof formattedResult === 'object') {
      if (formattedResult.createdAt) {
        formattedResult.createdAt = new Date(formattedResult.createdAt).toISOString()
      }
      if (formattedResult.updatedAt) {
        formattedResult.updatedAt = new Date(formattedResult.updatedAt).toISOString()
      }
    }
    return {
      code: 0,
      data: formattedResult,
      msg: "update user game info success"
    }
  } catch (error) {
    return {
      code: -1,
      msg: error.message
    }
  }
}

