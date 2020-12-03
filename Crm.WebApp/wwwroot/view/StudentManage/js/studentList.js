var table = null;
var layer = null;
var form = null;
var active = {
  //条件查询
  search: function () {
    var demoReload = $('#demoReload').val();
    var sel_vip = $('#sel_vip').val();

    //执行重载
    table.reload('tableReload', {
      page: {
        curr: 1 //重新从第 1 页开始
      },
      where: {
        name: demoReload,
        isVip: sel_vip
      }
    }, 'data');
  },
  //刷新页面
  reload: function () {
    window.location.reload();
  }
};

layui.use(['form', 'table', 'layer'], function () {
  table = layui.table;
  layer = layui.layer;
  form = layui.form;

  table.render({

    elem: '#tableData',
    id: "tableReload",
    url: ApiService.SystemApi.APIService + '/Api/Student/GetData',
    headers: {
      "ToKenStr": localStorage.Token
    },
    page: true,
    height: 'full-60',
    toolbar: '#toolbarDemo', //开启头部工具栏，并为其绑定左侧模板
    defaultToolbar: ['filter'],
    where: {
      name: '',
      isVip: -1
    },
    cols: [
      [{
          title: '标签',
          templet: '#imgShow'
        },
        {
          title: '头像',
          templet: function (val) {
            var imgUrl = "/images/name.png";
            if (!IsNullOrEmpty(val.HeadImg))
              imgUrl = ApiService.UpLoadApi.APIService + val.HeadImg;
            var str = '<img style="width:50px" src="' + imgUrl + '">';
            return str;
          }
        },
        {
          field: 'SexStr',
          title: '性别'
        },
        {
          field: 'LoginName',
          title: '用户账号'
        },
        {
          field: 'NickName',
          title: '用户昵称'
        },
        {
          field: 'Telephone',
          title: '手机号'
        },
        {
          field: 'CreateTime',
          title: '注册时间'
        },
        {
          field: '',
          title: '操作',
          toolbar: '#barDemo'
        }
      ]
    ],
    response: {
      statusCode: 1 //重新规定成功的状态码为 1 ，table 组件默认为 0
    },
    done: function (res, curr, count) { //表格数据加载完后的事件
      //调用示例
      layer.photos({ //点击图片弹出
        photos: '.layer-photos-demo',
        anim: 1 //0-6的选择，指定弹出图片动画类型，默认随机（请注意，3.0之前的版本用shift参数）
      });
    },
    parseData: function (res) {
      var count = 0;
      var tableList = [];
      //将原始数据解析成 table 组件所规定的数据
      if (res.code == 1 && res.data != null) {
        count = res.data.count;
        tableList = res.data.list;
      }
      return {
        "code": res.code, //解析接口状态
        "msg": res.message, //解析提示文本
        "count": count, //解析数据长度
        "data": tableList //解析数据列表
      };
    }
  });

  //监听行工具事件
  table.on('tool(tableData)', function (obj) {
    var data = obj.data; //当前选中记录了数据
    //console.log(obj)
    if (obj.event === 'edit') {
      localStorage.detail = JSON.stringify(data);
      OpenDetail();
    }
  });

  form.on('select(sel_vip)', function (data) {
    active.search();
  });

  form.render();

});

//保存信息
function OpenDetail() {
  layer.open({
    type: 2,
    title: "用户详情",
    scrollbar: false,
    area: ['850px', '650px'],
    content: 'StudentDetail.html',
    btn: ['保存', '取消'],
    btnAlign: 'c',
    yes: function (indexs, layero) {

      var model = $(layero).find("iframe")[0].contentWindow.getNowDetailJson();
      if (IsNullOrEmpty(model.NickName)) {
        layer.msg("请填写昵称");
        return false;
      }
      if (IsNullOrEmpty(model.LoginName)) {
        layer.msg("请填写用户名");
        return false;
      }
      if (IsNullOrEmpty(model.Telephone)) {
        layer.msg("请填写手机号");
        return false;
      }
      if (IsNullOrEmpty(model.LoginPwd)) {
        layer.msg("请填写密码");
        return false;
      }
      ajaxPost({
        url: ApiService.SystemApi.APIService + "/Api/Student/UpdateModel",
        headers: {
          "ToKenStr": localStorage.Token
        },
        data: model,
        success: function (res) {
          layer.msg(res.message);
          if (res.code != 1) {
            return false;
          }
          layer.closeAll();
          active.reload();
        }
      });
    }
  });
}