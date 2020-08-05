using Currency.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Crm.WebApp.Infrastructure
{
    public class ChatHub : Hub
    {
        private readonly static Dictionary<string, string> userList = new Dictionary<string, string>();

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="userName">用户Id</param>
        /// <returns></returns>
        public void UserLoginSignalR(string userName)
        {
            //每次登陆UserId都会改变
            if (userList.ContainsKey(userName))
            {
                userList[userName] = Context.ConnectionId;
            }
            else
            {
                userList.Add(userName, Context.ConnectionId);
            }
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public async Task GetUserList()
        {
            await Clients.All.SendAsync("ReceiveUserList", userList.ToArray().ToJson());
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

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }

}
