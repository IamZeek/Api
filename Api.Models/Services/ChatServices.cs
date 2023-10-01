using Api.Models.HubConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Services
{
    public class ChatServices
    {
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();


        public bool AddUserToList(string UserToAdd)
        {
            lock (Users)
            {
                foreach(var user in Users)
                {
                    if(user.Key.ToLower() == UserToAdd.ToLower())
                    {
                        return false;
                    }
                }
                Users.Add(UserToAdd, null);
                return true;
            }
        }

        public void AddUserConnectionId(string user, string connectionId)
        {
            lock (Users)
                {
                if (Users.ContainsKey(user))
                {
                    Users[user] = connectionId;
                }
            }
        }


        public string GetUserByConnectionId(string connectionId)
        {
            lock (Users)
            {
                string email = Users.Where(x => x.Value == connectionId).Select(x => x.Key).FirstOrDefault();
                return Users.Where(x => x.Value == connectionId).Select(x => x.Key).FirstOrDefault();
            }
        }

        public string GetUserByUser(string user)
        {
            lock (Users)
            {
                return Users.Where(x => x.Key == user).Select(x => x.Value).FirstOrDefault();
            }
        }

        public void RemoveUser(string user)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users.Remove(user);
                }
            }
        }

        public string[] GetOnlineUsers()
        {
            lock (Users)
            {
                return Users.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }
        }
    }
}
