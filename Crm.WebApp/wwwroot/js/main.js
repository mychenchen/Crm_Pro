var ApiService = {
  "SystemApi": {
    "ApiName": "服务API--> Service-->陈浩", //API名称
    "Sandbox": "http://localhost:13973", //测试环境
    "Running": "http://134.175.30.213:8011", //正式环境
    "RuntimeType": "Sandbox", //当前运行的环境   类型1：Sandbox:测试环境,类型2：Running正式环境
    "APIService": "" //API输出地址
  },
  "SignalRUrl": {
    "ApiName": "SignalR服务API--> Service-->陈浩", //API名称
    "Sandbox": "http://localhost:13973", //测试环境
    "Running": "http://134.175.30.213:8011", //正式环境
    "RuntimeType": "Sandbox", //当前运行的环境   类型1：Sandbox:测试环境,类型2：Running正式环境
    "APIService": "" //API输出地址
  },
  "UpLoadApi": {
    "ApiName": "上传服务--> 陈浩", //API名称
    "Sandbox": "http://localhost:13973", //测试环境
    "Running": "http://134.175.30.213:8011", //正式环境
    "RuntimeType": "Running", //当前运行的环境   类型1：Sandbox:测试环境,类型2：Running正式环境
    "APIService": "" //API输出地址
  },
  HtmlInfo: {
    LogTitle: "教育-Crazy",
    topTitle: "教育-Crazy",
    bottomTitle: "© 教育-Crazy"
  }
};

$(function () {
  //初始化 API服务地址环境
  JsonConfin();
});

//初始化 API服务地址环境
var JsonConfin = function () {

  $.each(ApiService, function (e, i) {

    //默认为正式环境
    ApiService[e].APIService = ApiService[e].Running;

    //如果配置Sandbox
    if (ApiService[e].RuntimeType == "Sandbox") {
      ApiService[e].APIService = ApiService[e].Sandbox;
    }
  })
}

//获取tab标签
function getTabMenu(selId, pid, defultId) {
  this.ajaxGet({
    url: ApiService.SystemApi.APIService + "/Api/Common/GetAllTabMenu",
    data: {
      pid: pid
    },
    async: false,
    success: function (res) {
      var str = '<option value="">请选择</option>';
      $.each(res.data, function (i, val) {
        if (defultId == val.Id)
          str += '<option value="' + val.Id + '" selected>' + val.Name + '</option>';
        else
          str += '<option value="' + val.Id + '">' + val.Name + '</option>';

      })
      $("#" + selId).html(str);
    }
  });
}

//获取用户标签
function getUserLableSelect(selId, pid, defultId) {
  var logUserInfo = JSON.parse(localStorage.LoginUser);
  this.ajaxGet({
    url: ApiService.SystemApi.APIService + "/Api/Common/GetAllLable",
    data: {
      pid: pid
    },
    async: false,
    success: function (res) {
      var str = '<option value="">请选择</option>';
      $.each(res.data, function (i, val) {
        if (defultId == val.id) {
          str += '<option value="' + val.id + '" selected>' + val.name + '</option>';
        } else if (val.name == "管理员" && logUserInfo.LoginUser != "admin") {
          str += '<option  disabled>' + val.name + '</option>';
        } else {
          str += '<option value="' + val.id + '">' + val.name + '</option>';
        }
      })
      $("#" + selId).html(str);
    }
  });
}

//获取数据中的指定元素
function getListFirt(v_list, index) {
  for (var i = 0; i < v_list.length; i++) {
    var element = v_list[i];
    if (element.Id == index) {
      return element;
    }
  }
}

function ajaxGet(dom) {
  $.ajax({
    type: 'get',
    url: dom.url,
    data: dom.data,
    dataType: "json",
    headers: {
      "ToKenStr": localStorage.Token
    },
    async: dom.async == undefined ? true : dom.async,
    success: function (res) {
      if (res.code == 401) {
        localStorage.LoginUser = "";
        localStorage.Token = "";
        setTimeout(() => {
          location.href = "/login.html";
        }, 2000);
        return false;
      }
      dom.success(res);
    },
    error: function (res, sss, ffff) {
      if (dom.error != undefined) {
        dom.error(res);
      }
    }
  });
}

function ajaxPost(dom) {
  $.ajax({
    type: 'post',
    url: dom.url,
    data: dom.data,
    headers: {
      "ToKenStr": localStorage.Token
    },
    async: dom.async == undefined ? true : dom.async,
    dataType: "json",
    success: function (res) {
      if (res.code == 401) {
        localStorage.LoginUser = "";
        localStorage.Token = "";
        setTimeout(() => {
          location.href = "/login.html";
        }, 2000);
        return false;
      }
      dom.success(res);
    },
    error: function (res, sss, ffff) {
      if (dom.error != undefined) {
        dom.error(res);
      }
    }
  });
}

//设置cookie
function setCookie(key, value) {
  //var oDate=new Date();
  //oDate.setDate(oDate.getDate()+iDay);
  var curDate = new Date();
  //当前时间戳
  var curTamp = curDate.getTime();
  //当日凌晨的时间戳,减去一毫秒是为了防止后续得到的时间不会达到00:00:00的状态
  var curWeeHours = new Date(curDate.toLocaleDateString()).getTime() - 1;
  //当日已经过去的时间（毫秒）
  var passedTamp = curTamp - curWeeHours;
  //当日剩余时间
  var leftTamp = 24 * 60 * 60 * 1000 - passedTamp;
  var leftTime = new Date();
  leftTime.setTime(leftTamp + curTamp);
  document.cookie = key + '=' + value + ';expires=' + leftTime.toGMTString() + ";path=/";
}

//获取cookie
function getCookie(cookie_name) {
  var allcookies = document.cookie;
  var cookie_pos = allcookies.indexOf(cookie_name); //索引的长度

  // 如果找到了索引，就代表cookie存在，
  // 反之，就说明不存在。
  if (cookie_pos != -1) {
    // 把cookie_pos放在值的开始，只要给值加1即可。
    cookie_pos += cookie_name.length + 1;
    var cookie_end = allcookies.indexOf(";", cookie_pos);

    if (cookie_end == -1) {
      cookie_end = allcookies.length;
    }

    var value = unescape(allcookies.substring(cookie_pos, cookie_end));
  }
  return value;
}

//获取URL参方法
function GetQueryString(name) {
  var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
  var r = decodeURI(window.location.search).substr(1).match(reg);
  if (r != null) return decodeURI(r[2]);
  return null;
}

//判断字符串是否为空
//<param name="str">字符串</param>
function IsNullOrEmpty(str) {
  if (str == null || str == undefined || typeof str == undefined || str == "null" || str == "undefined" || str == "") {
    return true;
  } else {
    return false;
  }
}

//验证手机号码
function VerifyMobile(m) {
  var valid_rule = /^[1](([3][0-9])|([4][5-9])|([5][0-3,5-9])|([6][5,6])|([7][0-8])|([8][0-9])|([9][1,8,9]))[0-9]{8}$/; // 手机号码校验规则

  if (IsNullOrEmpty(m)) {
    return false
  }


  if (!valid_rule.test(m.trim())) {
    return false;
  }
  return true;
}

//检查电话号码
function CheckReg(str) {
  if (str != "") {
    var phone = str;
    var p1 = /^\+?(\(\d+\))*(\d*-?\d+)+$/;
    var me = false;
    if (p1.test(phone)) me = true;
    if (!me) {
      str = '';
      return "ValidTelephone";
    }
  }
  return true;
}

//验证邮箱
var Emailreg = /^[A-Za-z\d]+([-_.][A-Za-z\d]+)*@([A-Za-z\d]+[-.])+[A-Za-z\d]{2,4}$/

function checkEmailReg(Email) {
  if (!Emailreg.test(Email)) {
    return false;
  }
  return true;
}

//验证是否为数字
function isNumber(value) {
  var patrn = /^(-)?\d+(\.\d+)?$/;
  if (patrn.exec(value) == null || value == "") {
    return false
  } else {
    return true
  }
}

////验证金额
function isDigit(s) {
  var patrn = /^(?:\d+|\d+\.\d{0,2})$/;
  if (!patrn.exec(s)) {
    return false
  } else {
    return true
  }
}


//正整数
function checkInt(e) {
  var re = new RegExp("^[0-9]*[1-9][0-9]*$");
  if (!re.test(e)) {
    return false;
  }
  return true;
}

//是否为身份证号

function IsIDCard(idCard) {
  //15位和18位身份证号码的正则表达式
  var regIdCard = /^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$/;

  //如果通过该验证，说明身份证格式正确，但准确性还需计算
  if (regIdCard.test(idCard)) {
    if (idCard.length == 18) {
      var idCardWi = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2); //将前17位加权因子保存在数组里
      var idCardY = new Array(1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2); //这是除以11后，可能产生的11位余数、验证码，也保存成数组
      var idCardWiSum = 0; //用来保存前17位各自乖以加权因子后的总和
      for (var i = 0; i < 17; i++) {
        idCardWiSum += idCard.substring(i, i + 1) * idCardWi[i];
      }

      var idCardMod = idCardWiSum % 11; //计算出校验码所在数组的位置
      var idCardLast = idCard.substring(17); //得到最后一位身份证号码

      //如果等于2，则说明校验码是10，身份证号码最后一位应该是X
      if (idCardMod == 2) {
        if (idCardLast == "X" || idCardLast == "x") {
          return true;
        } else {
          return false;
        }
      } else {
        //用计算出的验证码与最后一位身份证号码匹配，如果一致，说明通过，否则是无效的身份证号码
        if (idCardLast == idCardY[idCardMod]) {
          return true;
        } else {
          return false;
        }
      }
    }
  } else {
    return false;
  }
}
//验证护照号码
function checkPassport(code) {
  var hz = /^((1[45]\d{7})|(G\d{8})|(P\d{7})|(S\d{7,8}))?$/;
  if (!code || !hz.test(code)) {
    return false;
  }
  return true;
}
//验证中文
function IsChinese(str) {
  var lst = /^[\u2E80-\u9fa5a]+$/;
  return lst.test(str);
}
//验证中文、英文
function isName(Name) {
  var re = /^[\u4e00-\u9fa5a-zA-Z]*$/g;
  return re.test(Name);
}

/**** 2020-02-27 chenhao add */

/*当天 2017-10-17*/
function getToDayDate() {

  var systemDate = new Date();

  // 获取当年  
  var year = systemDate.getFullYear();

  // 获取当月 （月+1是因为js中月份是按0开始的）  
  var month = systemDate.getMonth() + 1;

  // 获取当日  
  var day = systemDate.getDate();

  var hours = systemDate.getHours();

  var minutes = systemDate.getMinutes();

  var seconds = systemDate.getSeconds();

  if (day < 10) { // 如果日小于10，前面拼接0  

    day = '0' + day;
  }

  if (month < 10) { // 如果月小于10，前面拼接0  

    month = '0' + month;
  }

  return year + "-" + month + "-" + day;

}

/*获取本周日期 */
function getWeekDataByType() {

  //按周日为一周的最后一天计算
  var date = new Date();

  //今天是这周的第几天
  var today = date.getDay();

  //上周日距离今天的天数（负数表示）
  var stepSunDay = -today + 1;

  // 如果今天是周日
  if (today == 0) {

    stepSunDay = -7;
  }

  // 周一距离今天的天数（负数表示）
  var stepMonday = 7 - today;

  var time = date.getTime();

  var monday = new Date(time + stepSunDay * 24 * 3600 * 1000);
  var sunday = new Date(time + stepMonday * 24 * 3600 * 1000);

  //本周一的日期 （起始日期）
  var startDate = transferDate(monday); // 日期变换
  //本周日的日期 （结束日期）
  var endDate = transferDate(sunday); // 日期变换

  return startDate + '|' + endDate;
}

/*获取本月日期 */
function getMonth() {

  // 获取当前月的第一天  
  var start = new Date();
  start.setDate(1);

  // 获取当前月的最后一天  
  var date = new Date();
  var currentMonth = date.getMonth();
  var nextMonth = ++currentMonth;
  var nextMonthFirstDay = new Date(date.getFullYear(), nextMonth, 1);
  var oneDay = 1000 * 60 * 60 * 24;
  var end = new Date(nextMonthFirstDay - oneDay);

  var startDate = transferDate(start); // 日期变换  
  var endDate = transferDate(end); // 日期变换  

  return startDate + '|' + endDate;
}

/*获取本季度日期 */
function getQuarterMonth(type) {
  var now = new Date();
  var nowMonth = now.getMonth();
  var year = now.getFullYear();
  var quarterStartMonth = 1;
  if (nowMonth < 3) {
    quarterStartMonth = 1;
  }
  if (2 < nowMonth && nowMonth < 6) {
    quarterStartMonth = 4;
  }
  if (5 < nowMonth && nowMonth < 9) {
    quarterStartMonth = 7;
  }
  if (nowMonth > 8) {
    quarterStartMonth = 10;
  }
  if (type == "s") {
    var day = year + "-" + quarterStartMonth + "-01";
  }; //1-3 4-6 7-9 10-12
  if (type == "e") {
    if (quarterStartMonth == 1) {
      var day = year + "-" + (quarterStartMonth + 2) + "-31";
    }
    if (quarterStartMonth == 10) {
      var day = year + "-" + (quarterStartMonth + 2) + "-31";
    }
    if (quarterStartMonth == 4) {
      var day = year + "-" + (quarterStartMonth + 2) + "-30";
    }
    if (quarterStartMonth == 7) {
      var day = year + "-" + (quarterStartMonth + 2) + "-30";
    }
  };
  return day;
}

/*日期变换的方法 */
function transferDate(date) {

  // 年  
  var year = date.getFullYear();
  // 月  
  var month = date.getMonth() + 1;
  // 日  
  var day = date.getDate();

  if (month >= 1 && month <= 9) {

    month = "0" + month;
  }
  if (day >= 0 && day <= 9) {

    day = "0" + day;
  }

  var dateString = year + '-' + month + '-' + day;

  return dateString;
}