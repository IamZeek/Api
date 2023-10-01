using Api.Models.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.HubConfig
{
    public class ApiHub: Hub
    {

        private readonly ChatServices _services;
        public ApiHub(ChatServices services)
        {
            _services = services;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ChatApp");
            await Clients.Caller.SendAsync("UserConnected");
        }
         

        public async Task SendMessage(string sender, string reciver, string message)
        {
            await Clients.Group(reciver).SendAsync("ReceiverMessage", sender, message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatApp");
            var user = _services.GetUserByConnectionId(Context.ConnectionId);
            _services.RemoveUser(user);
            await DisplayOnlineUsers();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ReceiveMessage(Messages message)
        {
            await Clients.Group("ChatApp").SendAsync("NewMessage",message);
        }

        public async Task AddUserConnectionId(string name)
        {
            _services.AddUserConnectionId(name, Context.ConnectionId);
            await DisplayOnlineUsers();
        }

        public async Task CreatePrivateChat(Messages Msg)
        {
            string privateGroupName = GetPrivateGroupName(Msg.Sender, Msg.Receiver);
            await Groups.AddToGroupAsync(Context.ConnectionId, privateGroupName);
            var toConnectionId = _services.GetUserByUser(Msg.Receiver);
            await Groups.AddToGroupAsync(toConnectionId, privateGroupName);

            await Clients.Client(toConnectionId).SendAsync("OpenPrivateChat", Msg);

        }

        public async Task DisplayOnlineUsers()
        {
            var onlineUsers = _services.GetOnlineUsers();
            await Clients.Groups("ChatApp").SendAsync("OnlineUsers", onlineUsers);
        }

        public async Task ReceivePrivateMessage(Messages msg)
        {
            string privateGroupName = GetPrivateGroupName(msg.Sender, msg.Receiver);
            await Clients.Group(privateGroupName).SendAsync("NewPrivateMessage", msg);
        }
        public async Task RemovePrivateChat(string sender, string receiver)
        {
            string privateGroupName = GetPrivateGroupName(sender, receiver);
            await Clients.Group(privateGroupName).SendAsync("ClosePrivateChat");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, privateGroupName);
            var toConnectionId = _services.GetUserByUser(receiver);
            await Groups.RemoveFromGroupAsync(toConnectionId, privateGroupName);


        }

        private string GetPrivateGroupName(string sender, string receiver)
        {
            var stringCompare = string.CompareOrdinal(sender, receiver) > 0;
            return stringCompare ? $"{sender}-{receiver}" : $"{receiver}-{sender}";
        }

    }
}
