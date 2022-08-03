﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Domain
{


    [Serializable]
    public class Message
    {
        public enum Header { REGISTER, JOIN, QUIT, JOIN_CR, QUIT_CR, CREATE_CR, POST}
        private Header head;
        private List<string> messageList;

        public Header Head
        {
            get
            {
                return head;
            }

            set
            {
                head = value;
            }
        }

        public List<string> MessageList
        {
            get
            {
                return messageList;
            }

            set
            {
                messageList = value;
            }
        }

        public Message(Header head, string message)
        {
            this.Head = head;
            this.MessageList = new List<string>();
            this.MessageList.Add(message);
        }

        public Message(Header head)
        {
            this.Head = head;
            this.MessageList = new List<string>();
        }

        public Message(Header head, List<string> messages)
        {
            this.Head = head;
            this.MessageList = new List<string>();
            this.MessageList = messages;
        }

        /// <summary>
        /// Add data to the message list
        /// </summary>
        /// <param name="message"></param>
        public void addData(string message)
        {
            
            this.MessageList.Add(message);
        }
    }
}
