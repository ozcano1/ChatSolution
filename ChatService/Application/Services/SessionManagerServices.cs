using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatService.Exceptions;

namespace ChatService.Application.Services
{
    /// <summary>
    /// Handle sessions with a manager
    /// </summary>
    public class SessionManagerServices
    {
        List<SessionServices> sessionList;

        /// <summary>
        /// Store all sessions in a list
        /// </summary>
        public List<SessionServices> SessionList
        {
            get
            {
                return sessionList;
            }

            set
            {
                sessionList = value;
            }
        }

        public SessionManagerServices()
        {
            SessionList = new List<SessionServices>();
        }

        /// <summary>
        /// Add a session to the manager. Make sure it is not already stored with the GUID
        /// </summary>
        /// <param name="other">Other session</param>
        public void addSession(SessionServices other)
        {
            foreach (SessionServices session in SessionList.ToList())
            {
                if (session.Token == other.Token)
                {
                    throw new SessionAlreadyExistsException(session.Token.ToString());
                }
            }

            SessionList.Add(other);

        }

        /// <summary>
        /// Delete a session using its token
        /// </summary>
        /// <param name="token"></param>
        public void removeSession(Guid token)
        {
            SessionServices sessionToDelete = null;

            foreach (SessionServices session in SessionList.ToList())
            {
                if (session.Token == token)
                {
                    sessionToDelete = session;
                }
            }

            if (sessionToDelete == null)
            {
                //throw new SessionUnknownException(token.ToString());
            }

            SessionList.Remove(sessionToDelete);
        }
    }
}