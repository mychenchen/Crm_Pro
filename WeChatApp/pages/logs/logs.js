var util = require('../../utils/util.js')
Page({
  data: {
    logs: [],
    dates: "",
    modalHidden: true,
    toastHidden: true
  },
  onShow: function () {
    wx.setNavigationBarTitle({
      title: '任务记录'
    })
    // this.getLogs()
  },
  set: function () {

  },
  getPhoneNumber: function (e) {
    console.log('值为', e)

  },
  getLogs: function () {
    let logs = wx.getStorageSync('logs')
    logs.forEach(function (item, index, arry) {
      item.startTime = new Date(item.startTime).toLocaleString()
    })
    this.setData({
      logs: logs
    })
  },
  onLoad: function () {
    wx.login({
      success(res) {
console.log(res.code)
      }
    })
  },
  switchModal: function () {
    this.setData({
      modalHidden: !this.data.modalHidden
    })
  },
  hideToast: function () {
    this.setData({
      toastHidden: true
    })
  },
  clearLog: function (e) {
    wx.setStorageSync('logs', [])
    this.switchModal()
    this.setData({
      toastHidden: false
    })
    this.getLogs()
  }
})