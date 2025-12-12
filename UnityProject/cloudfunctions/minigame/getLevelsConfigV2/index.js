// 云函数入口文件
const cloud = require('wx-server-sdk')
cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV }) // 使用当前云环境
const db = cloud.database()
const levels = db.collection('Levels')

// 云函数入口函数
exports.main = async (event, context) => {
  try {
    const { data } = await levels.get({
      filter: {
        where: {
          $and: [
            {
              _id: {
                $eq: "c0a2d8e468439784021a265910ba40eb",
              },
            },
          ]
        }
      },
    });
    
    return {
      code: 0,
      data: data,
      msg: "get levels config success"
    }
  } catch (error) {
    return {
      code: -1,
      msg: error.message
    }
  }
}

