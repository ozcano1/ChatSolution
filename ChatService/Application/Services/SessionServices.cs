﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ChatService.Domain
{
    public class SessionServices
    {
        Guid token;
        User user;
        TcpClient client;

        /// <summary>
        /// Each session has an unique token
        /// </summary>
        public Guid Token
        {
            get
            {
                return token;
            }

            set
            {
                token = value;
            }
        }

        public User User
        {
            get
            {
                return user;
            }

            set
            {
                user = value;
            }
        }

        public TcpClient Client
        {
            get
            {
                return client;
            }

            set
            {
                client = value;
            }
        }

        public SessionServices()
        {
            this.Token = Guid.NewGuid();
            this.Client = null;
            this.User = null;
        }
    }
}
