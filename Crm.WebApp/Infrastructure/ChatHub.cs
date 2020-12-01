using Crm.WebApp.Models;
using Currency.Common;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.Infrastructure
{
    public class ChatHub : Hub
    {
        protected static List<LoginUserInfo> userList = new List<LoginUserInfo>();

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="obj">用户信息</param>
        /// <returns></returns>
        public void UserLoginSignalR(string obj)
        {
            var info = obj.ToObject<LoginUserInfo>();

            //每次登陆UserId都会改变
            var old_info = userList.FirstOrDefault(a => a.Gid == info.Gid);
            if (old_info != null)
            {
                old_info.OnLine = true;
                old_info.ConnectionId = Context.ConnectionId;
            }
            else
            {
                info.OnLine = true;
                info.ConnectionId = Context.ConnectionId;
                userList.Add(info);
            }
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public async Task GetUserList()
        {
            await Clients.All.SendAsync("ReceiveUserList", userList.ToJson());
        }

        /// <summary>
        /// 发消息给所有人
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }


        /// <summary>
        /// 发送消息--发送给指定用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendPrivateMessage(string userId, string message)
        {
            await Clients.Client(userId).SendAsync("ReceiveMessage", message);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 检测用户断开时处理(刷新页面也会进来)
        /// </summary>
        /// <returns></returns>
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var info = userList.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId);
            if (info != null)
            {
                info.OnLine = false;
                info.ConnectionId = "";
                await GetUserList();
            }
        }
    }

}
