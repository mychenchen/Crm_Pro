var table = null;
var layer = null;
var searchModel = {
  //操作类型
  title: ""
};
var active = {
  //条件查询
  search: function () {
    searchModel.title = $('#txt_Title').val();

    //执行重载
    table.reload('tableReload', {
      page: {
        curr: 1 //重新从第 1 页开始
      },
      where: searchModel
    }, 'data');
  },
  //刷新页面
  reload: function () {
    window.location.reload();
  },
  //刷新页面
  reloadCliTable: function (tableName) {
    table.reload(tableName, 'data');
  }
};

layui.use(['table', 'layer', 'element'], function () {
  var element = layui.element;
  table = layui.table;
  layer = layui.layer;

  //加载列表
  table.render({
    elem: '#tableData',
    id: "tableReload",
    url: ApiService.SystemApi.APIService + '/Api/SystemMenu/GetData',
    headers: {
      "ToKenStr": localStorage.Token
    },
    page: true,
    height: 'full-60',
    toolbar: '#toolbarDemo' //开启头部工具栏，并为其绑定左侧模板
      ,
    defaultToolbar: ['filter'],
    where: searchModel,
    cols: [
      [{
          field: 'btn',
          width: 50,
          align: 'center',
          templet: function (d) {
            return '<a class="a_btn" style="cursor: pointer;" lay-event="addRowTable">+</a>'
          }
        },
        {
          field: 'Name',
          title: '菜单名称',
          align: 'center'
        }, {
          field: 'SortNum',
          title: '排序',
          align: 'center'
        }, {
          field: 'MenuType',
          title: '平台',
          align: 'center',
          templet: function (m) {
            return m.MenuType == 1 ? "PC端" : "小程序端";
          }
        }, {
          field: 'IsShow',
          title: '是否显示',
          align: 'center',
          templet: function (m) {
            let str = ' ';
            if (m.IsShow == 1)
              str = 'checked="" ';
            str = '<input type="checkbox" lay-skin="switch" lay-text="显示|隐藏" disabled="" ' + str + ' >';
            return str;
          }
        }, {
          field: 'CreateTimeStr',
          title: '创建时间',
          align: 'center'
        }, {
          field: '',
          title: '操作',
          align: 'center',
          toolbar: '#barDemo'
        }
      ]
    ],
    response: {
      statusCode: 1 //重新规定成功的状态码为 1 ，table 组件默认为 0
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
        OpenDetail("菜单详情", 0);
        break;
    };
  });

  //监听行工具事件
  table.on('tool(tableData)', function (obj) {
    var data = obj.data; //当前选中记录了数据
    //console.log(obj)
    if (obj.event === 'del') {
      deleteInfo(obj, data.Id);
    } else if (obj.event === 'edit') {
      localStorage.detail = JSON.stringify(data);
      OpenDetail("菜单详情", 0);
    }
    //添加子级
    else if (obj.event === 'addChild') {
      data.ParentGid = data.Id;
      data.Id = "";
      data.Name = "";
      data.SortNum = 0;
      localStorage.detail = JSON.stringify(data);
      OpenDetail("新增子级信息", 1);
    }
    //监听 "+" 操作
    else if (obj.event === 'addRowTable') {
      $(".table-item").remove();
      $(".layui-table > tr").removeClass('layui-table-click');
      $(".a_btn").attr('lay-event', 'addRowTable');

      var tr = $(this).parents('tr');
      var trIndex = tr.data('index');
      $(this).attr('lay-event', 'no');
      addRowChild(trIndex, tr, table, data.Id);
    } else if (obj.event === 'no') {
      $(".table-item").remove();
      $(this).attr('lay-event', 'addRowTable');
    }
  });

});
var childTableName = "";

function addRowChild(trIndex, tr, table, pid) {

  // 异常不要用它原来的这个作为tr的dom
  // var tr = obj.tr; //获得当前行 tr 的DOM对象
  childTableName = 'tableOut_tableIn_' + trIndex;
  var _html = [
    '<tr class="table-item">',
    '<td colspan="' + tr.find('td').length + '" style="padding: 6px 12px;">',
    '<table id="' + childTableName + '" lay-filter="childtable"></table>',
    '</td>',
    '</tr>'
  ];
  tr.after(_html.join('\n'));
  // 渲染 子级table
  var _tablechild = table.render({
    elem: '#' + childTableName,
    url: ApiService.SystemApi.APIService + '/Api/SystemMenu/GetDataByPid',
    page: false //关闭分页
      // cellMinWidth: 80,//最小列宽
      ,
    method: "get",
    headers: {
      "ToKenStr": localStorage.Token
    },
    where: {
      pid: pid
    },
    cols: [
      [{
        field: 'Name',
        title: '菜单名称',
        align: 'center'
      }, {
        field: 'SortNum',
        title: '排序',
        align: 'center'
      }, {
        field: 'MenuType',
        title: '平台',
        align: 'center',
        templet: function (m) {
          return m.MenuType == 1 ? "PC端" : "小程序端";
        }
      }, {
        field: 'IsShow',
        title: '是否显示',
        align: 'center',
        templet: function (m) {
          let str = ' ';
          if (m.IsShow == 1)
            str = 'checked="" ';
          str = '<input type="checkbox" lay-skin="switch" lay-text="显示|隐藏" disabled="" ' + str + ' >';
          return str;
        }
      }, {
        field: 'CreateTimeStr',
        title: '创建时间',
        align: 'center'
      }, {
        field: '',
        title: '操作',
        align: 'center',
        toolbar: '#barDemoChild'
      }]
    ],
    response: {
      statusCode: 1 //重新规定成功的状态码为 1 ，table 组件默认为 0
    },
    parseData: function (res) {
      var count = 0;
      var tableList = [];
      //将原始数据解析成 table 组件所规定的数据
      if (res.code == 1 && res.data != null) {
        count = res.data.length;
        tableList = res.data;
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
  table.on('tool(childtable)', function (obj) {
    var data = obj.data; //当前选中记录了数据
    if (obj.event === 'childEdit') {
      localStorage.detail = JSON.stringify(data);
      OpenDetail("修改子级信息", 1);
    } else if (obj.event === 'childDel') {
      deleteInfo(obj, data.Id);
    }
  });
}

//删除信息
function deleteInfo(obj, gid) {
  layer.confirm('真的删除行么', function (index) {

    ajaxGet({
      url: ApiService.SystemApi.APIService + "/Api/SystemMenu/DeleteModel",
      data: {
        gid: gid
      },
      success: function (res) {
        layer.msg(res.message);
        if (res.code != 1) {
          return false;
        }
        obj.del();
        layer.close(index);
      }
    });
  });
}

//保存信息
function OpenDetail(title, type) {
  layer.open({
    type: 2,
    title: title,
    scrollbar: false,
    area: ['550px', '450px'],
    content: 'Detail.html',
    btn: ['提交', '取消'],
    btnAlign: 'c',
    yes: function (indexs, layero) {
      //获取详情页指定函数
      var model = $(layero).find("iframe")[0].contentWindow.getNowDetailJson();

      if (IsNullOrEmpty(model.Name)) {
        layer.msg("请填写名称");
        return false;
      }
      if (IsNullOrEmpty(model.SortNum)) {
        layer.msg("请填写排序");
        return false;
      }
      if (!isNumber(model.SortNum)) {
        layer.msg("排序格式错误,请填写数字");
        return false;
      }
      if (IsNullOrEmpty(model.Location)) {
        layer.msg("请输入地址链接");
        return false;
      }
      if (IsNullOrEmpty(model.MenuType)) {
        layer.msg("请输入选择平台");
        return false;
      }

      ajaxPost({
        url: ApiService.SystemApi.APIService + "/Api/SystemMenu/SaveModel",
        data: model,
        success: function (res) {
          layer.msg(res.message);
          if (res.code != 1) {
            return false;
          }
          if (type == 0) {
            active.reload();
          } else {
            active.reloadCliTable(childTableName);
          }
          layer.closeAll();
        }
      });
    }
  });
}