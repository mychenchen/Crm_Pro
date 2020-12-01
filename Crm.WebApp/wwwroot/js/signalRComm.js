var signalr_connection = null;
$(function () {
  //创建连接对象connection
  signalr_connection = new signalR.HubConnectionBuilder()
    .withUrl(ApiService.SignalRUrl.APIService + "/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

  //启动connection
  signalr_connection.start()
    .then(function () {
      //记录用户登陆
      signalr_connection.invoke("UserLoginSignalR", localStorage.LoginUser)
        .then(() => {
          //当有新的用户连接上服务器时,重新获取用户列表
          signalr_connection.invoke("GetUserList");
        })
        .catch(err =>
          console.error("登陆失败：" + err.toString())
        );

    }).catch(function (ex) {
      console.log("连接失败" + ex);
      //SignalR JavaScript 客户端不会自动重新连接，必须编写代码将手动重新连接你的客户端
      setTimeout(() => start(), 5000);
    });

  signalr_connection.onclose(async () => {
    start();
  });

  //绑定事件("ReceiveMessage"和服务器端的SendMessage方法中的第一个参数一致)
  signalr_connection.on("ReceiveUserList", function (data) {
    var list = JSON.parse(data);
    localStorage.ReceiveUserList = data;
    // var str = '<option value="" >请选择</option>';
    // $.each(list, function (i, val) {
    //   if (val.OnLine) {
    //     str += '<option value="">' + val.Name + '(' + val.OnLineStr + ')</option>';
    //   } else {
    //     str += '<option value="' + val.ConnectionId + '">' + val.Name + '</option>';
    //   }
    // });
    // $("#sel_user").html(str);
  });

  //绑定事件("ReceiveMessage"和服务器端的SendMessage方法中的第一个参数一致)
  signalr_connection.on("ReceiveMessage", function (res) {
    var data = JSON.parse(res);
    $("#resultHtml").append("<h1>" + data.message + "</h1>");
  });
});


async function start() {
  try {
    await signalr_connection.start();
    console.log("connected");
  } catch (err) {
    console.log(err);
    setTimeout(() => start(), 5000);
  }
};


//发送指定人消息
function btnSendMsg() {
  var userId = $.trim($("#sel_user").val());
  var msgcontent = $.trim($("#msgcontent").val());
  var msgObj = {};
  msgObj.message = msgcontent;

  signalr_connection.invoke("SendPrivateMessage", userId, JSON.stringify(msgObj))
    .catch(err =>
      console.error("发送失败：" + err.toString())
    );
}