var table = null;
var layer = null;
var form = null;
var active = {
  //条件查询
  search: function () {
    var demoReload = $('#demoReload').val();

    //执行重载
    table.reload('tableReload', {
      page: {
        curr: 1 //重新从第 1 页开始
      },
      where: {
        name: demoReload
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

  //监听指定开关
  form.on('switch(switchTest)', function (data) {
    let onShelf = 0;
    if (this.checked) onShelf = 1;

    ajaxGet({
      url: ApiService.SystemApi.APIService + "/Api/Product/ProductIssue",
      headers: {
        "ToKenStr": localStorage.Token
      },
      data: {
        gid: this.id,
        onShelf: onShelf
      },
      success: function (res) {
        if (onShelf == 1)
          layer.msg("上架" + res.message);
        else
          layer.msg("下架" + res.message);
      }
    });
  });


  table.render({

    elem: '#tableData',
    id: "tableReload",
    url: ApiService.SystemApi.APIService + '/Api/Product/GetData',
    headers: {
      "ToKenStr": localStorage.Token
    },
    page: true,
    height: 'full-60',
    toolbar: '#toolbarDemo' //开启头部工具栏，并为其绑定左侧模板
      ,
    defaultToolbar: ['filter'],
    where: {
      name: ''
    },
    cols: [
      [{
          field: 'CoverPath',
          title: '封面',
          width: 150,
          templet: '#imgShow'
        },
        {
          field: 'Title',
          title: '课程标题'
        },
        {
          field: 'Subtitle',
          title: '课程副标题'
        },
        {
          field: 'Price',
          title: '价格'
        },
        {
          field: 'DiscountPrice',
          title: '折后价'
        },
        {
          field: 'OnShelfStatus',
          title: '状态',
          templet: function (m) {
            let str = '<input id="' + m.Id + '" type="checkbox" lay-skin="switch" lay-filter="switchTest" lay-text="上架|下架">';
            if (m.OnShelfStatus == 1)
              str = '<input id="' + m.Id + '" type="checkbox" checked="" lay-skin="switch" lay-filter="switchTest" lay-text="上架|下架">';
            return str;
          }
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

  //头工具栏事件
  table.on('toolbar(tableData)', function (obj) {

    // var checkStatus = table.checkStatus(obj.config.id); var data = checkStatus.data; 获取check选中的数据行
    switch (obj.event) {
      case 'add':
        localStorage.detail = '';
        OpenDetail();
        break;
    };
  });

  //监听行工具事件
  table.on('tool(tableData)', function (obj) {
    var data = obj.data; //当前选中记录了数据
    //console.log(obj)
    if (obj.event === 'del') {
      if (data.LoginName == "admin") {
        layer.msg("最高权限账号,无法删除");
        return false;
      }
      layer.confirm('真的删除行么', function (index) {

        $.ajax({
          url: ApiService.SystemApi.APIService + "/Api/Product/DeleteModel",
          type: "get",
          headers: {
            "ToKenStr": localStorage.Token
          },
          data: {
            gid: data.Id
          },
          success: function (res) {
            layer.msg(res.message);
            if (res.code != 1) {
              return false;
            }
            active.reload();
          }
        });
      });
    } else if (obj.event === 'edit') {
      localStorage.detail = JSON.stringify(data);
      OpenDetail();
    } else if (obj.event === 'detail') {
      localStorage.detail = JSON.stringify(data);
      location.href = "CourseDetail.html";
    }
  });

});

//保存信息
function OpenDetail() {
  layer.open({
    type: 2,
    title: "添加课程",
    scrollbar: false,
    area: ['1200px', '800px'],
    content: 'AddCourse.html',
    btn: ['提交', '取消'],
    btnAlign: 'c',
    yes: function (indexs, layero) {
      //获取详情页指定函数
      var model = $(layero).find("iframe")[0].contentWindow.getNowDetailJson();
      if (IsNullOrEmpty(model.Title)) {
        layer.msg("请填写课程标题");
        return false;
      }
      if (IsNullOrEmpty(model.Price)) {
        layer.msg("请填写售价");
        return false;
      }
      if (!IsNullOrEmpty(model.Discount) && IsNullOrEmpty(model.DiscountPrice)) {
        layer.msg("请填写折后价");
        return false;
      }
      if (IsNullOrEmpty(model.CoverPath)) {
        layer.msg("请上传封面图");
        return false;
      }
      ajaxPost({
        url: ApiService.SystemApi.APIService + "/Api/Product/SaveModel",
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