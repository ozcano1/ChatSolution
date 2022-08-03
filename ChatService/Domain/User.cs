using System;
using System.Net.Sockets;

namespace ChatService.Domain
{
    /// <summary>
    /// Handle an user
    /// </summary>
    [Serializable()]
    public class User : IComparable<User>
    {
        string login;
        string password;
        Chatroom chatroom;
        public DateTime LastPost;
        public bool LastPostTQuick;

        public string Login
        {
            get
            {
                return login;
            }

            set
            {
                login = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public Chatroom Chatroom
        {
            get
            {
                return chatroom;
            }

            set
            {
                chatroom = value;
            }
        }

        public User()
        {
            this.Login = "";
            this.Password = "";
            LastPost = DateTime.Now;
            LastPostTQuick = false;
        }

        public User(string login)
        {
            this.Login = login;
            this.Password = "";
            LastPost = DateTime.Now;
            LastPostTQuick = false;
        }

        public User(string login, string password)
        {
            this.Login = login;
            this.Password = password;
            LastPost = DateTime.Now;
            LastPostTQuick = false;
        }

        public int CompareTo(User other)
        {
            if (this.Login == other.Login && this.Password == other.Password)
                return 0;

            return -1;
        }

        public override string ToString()
        {
            return login;
        }
    }
}
