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
    let hasData = await userGameInfos.where({ openid: OPENID }).get()
    if (hasData.data.length === 0) {
      // 创建空记录，使用新字段格式
      let emptyData = {
        openid: OPENID,
        progressLevelID: 0,
        nickName: "",
        avatarUrl: "",
        openId: OPENID,
        createdAt: now,
        updatedAt: now,
      }
      let isAdd = await userGameInfos.add({ data: emptyData })
      // 将 Date 对象转换为 ISO 字符串格式
      const formattedEmptyData = {
        ...emptyData,
        createdAt: emptyData.createdAt.toISOString(),
        updatedAt: emptyData.updatedAt.toISOString(),
      }
      return {
        code: 0,
        data: formattedEmptyData,
        msg: "no result found, created empty info",
      }
    }

    // 返回数据，确保使用新字段格式
    const userData = hasData.data[0]
    
    // 将 Date 对象转换为 ISO 字符串格式，确保 JSON 序列化正确
    const formattedData = {
      ...userData,
      createdAt: userData.createdAt ? new Date(userData.createdAt).toISOString() : null,
      updatedAt: userData.updatedAt ? new Date(userData.updatedAt).toISOString() : null,
    }
    
    return {
      code: 0,
      data: formattedData,
      msg: "get user game info success",
    }
  } catch (error) {
    return {
      code: -1,
      msg: error.message
    }
  }
}

