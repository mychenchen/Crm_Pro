
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
            }
            , where: searchModel
        }, 'data');
    },
    //刷新页面
    reload: function () {
        window.location.reload();
    }
};

layui.use(['table', 'layer'], function () {
    table = layui.table;
    layer = layui.layer;
    table.render({

        elem: '#tableData'
        , id: "tableReload"
        , url: ApiService.SystemApi.APIService + '/Api/HotSpot/GetData'
        , page: true
        , height: 'full-44'
        , toolbar: '#toolbarDemo' //开启头部工具栏，并为其绑定左侧模板
        , defaultToolbar: ['filter']
        , where: searchModel
        , cols: [[
            // { type: 'checkbox' },
            { field: 'ImgTitle', title: '标题' }
            , { field: 'ImgPath', title: '图片', templet: '#imgShow' }
            , { field: 'ContentUrl', title: '内容链接' }
            , { field: 'CreateTimeStr', title: '创建时间' }
            , { field: '', title: '操作', toolbar: '#barDemo' }
        ]]
        , response: {
            statusCode: 1 //重新规定成功的状态码为 1 ，table 组件默认为 0
        }
        , parseData: function (res) {
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
        var data = obj.data;//当前选中记录了数据
        //console.log(obj)
        if (obj.event === 'del') {
            layer.confirm('真的删除行么', function (index) {

                $.ajax({
                    url: ApiService.SystemApi.APIService + "/Api/HotSpot/DeleteModel",
                    type: "get",
                    headers: { "ToKenStr": localStorage.Token },
                    data: { gid: data.Id },
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
        }
    });
});

//保存信息
function OpenDetail() {
    layer.open({
        type: 2,
        title: "详情",
        scrollbar: false,
        area: ['550px', '700px'],
        content: 'Detail.html',
        btn: ['提交', '取消'],
        btnAlign: 'c',
        yes: function (indexs, layero) {
            //获取详情页指定函数
            var model = $(layero).find("iframe")[0].contentWindow.getNowDetailJson();

            if (IsNullOrEmpty(model.ImgTitle)) {
                layer.msg("请填写标题");
                return false;
            }
            // if (IsNullOrEmpty(model.ImgPath)) {
            //     layer.msg("请填写上传图片");
            //     return false;
            // }

            $.ajax({
                url: ApiService.SystemApi.APIService + "/Api/HotSpot/SaveModel",
                type: "post",
                headers: { "ToKenStr": localStorage.Token },
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


