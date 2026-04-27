using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.Model.Domain;
using Ticketing.Services;
using log4net;
using Ticketing.Persistence.Repository; // C# equivalent of Log4j

namespace Ticketing.Service
{
    public class ChatServicesImpl : IChatServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ConcurrentDictionary<string, IChatObserver> _loggedClients;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatServicesImpl));
        
        // Object used for synchronization (equivalent to synchronized(this))
        private readonly object _lock = new object();

        private const int DefaultThreadsNo = 3;

        public ChatServicesImpl(IUserRepository uRepo, IMessageRepository mRepo)
        {
            _userRepository = uRepo;
            _messageRepository = mRepo;
            _loggedClients = new ConcurrentDictionary<string, IChatObserver>();
        }

        public void Login(User user, IChatObserver client)
        {
            lock (_lock)
            {
                // Note: user.Id maps to Username in our previous User refactor
                User userR = _userRepository.FindBy(user.Id, user.Passwd);
                if (userR != null)
                {
                    if (_loggedClients.ContainsKey(user.Id))
                        throw new ChatException("User already logged in.");
                    
                    _loggedClients.TryAdd(user.Id, client);
                    NotifyFriendsLoggedIn(user);
                }
                else
                {
                    throw new ChatException("Authentication failed.");
                }
            }
        }

        private void NotifyFriendsLoggedIn(User user)
        {
            IEnumerable<User> friends = _userRepository.GetFriendsOf(user);
            Logger.Debug($"Logged {friends}");

            foreach (User us in friends)
            {
                if (_loggedClients.TryGetValue(us.Id, out var chatClient))
                {
                    // Task.Run is the modern C# equivalent of Executor.execute()
                    Task.Run(() =>
                    {
                        try
                        {
                            Logger.Debug($"Notifying [{us.Id}] friend [{user.Id}] logged in.");
                            chatClient.FriendLoggedIn(user);
                        }
                        catch (ChatException e)
                        {
                            Logger.Error($"Error notifying friend {user.Id}", e);
                        }
                    });
                }
            }
        }

        private void NotifyFriendsLoggedOut(User user)
        {
            IEnumerable<User> friends = _userRepository.GetFriendsOf(user);
            foreach (User us in friends)
            {
                if (_loggedClients.TryGetValue(us.Id, out var chatClient))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            Logger.Debug($"Notifying [{us.Id}] friend [{user.Id}] logged out.");
                            chatClient.FriendLoggedOut(user);
                        }
                        catch (ChatException e)
                        {
                            Logger.Error("Error notifying friend ", e);
                        }
                    });
                }
            }
        }

        public void SendMessage(Message message)
        {
            lock (_lock)
            {
                string idReceiver = message.Receiver.Id;
                if (_loggedClients.TryGetValue(idReceiver, out var receiverClient))
                {
                    _messageRepository.Save(message);
                    receiverClient.MessageReceived(message);
                }
                else
                {
                    throw new ChatException($"User {idReceiver} not logged in.");
                }
            }
        }

        public void Logout(User user, IChatObserver client)
        {
            lock (_lock)
            {
                if (!_loggedClients.TryRemove(user.Id, out _))
                {
                    throw new ChatException($"User {user.Id} is not logged in.");
                }
                NotifyFriendsLoggedOut(user);
            }
        }

        public User[] GetLoggedFriends(User user)
        {
            lock (_lock)
            {
                IEnumerable<User> friends = _userRepository.GetFriendsOf(user);
                // SortedSet is the C# equivalent of TreeSet
                SortedSet<User> result = new SortedSet<User>();
                
                Logger.Debug($"Logged friends for: {user.Id}");
                foreach (User friend in friends)
                {
                    if (_loggedClients.ContainsKey(friend.Id))
                    {
                        result.Add(new User(friend.Id));
                        Logger.Debug($"+{friend.Id}");
                    }
                }
                Logger.Debug($"Size {result.Count}");
                return result.ToArray();
            }
        }

        public void DomainDataChanged(string entityType)
        {
            // We use the lock because iterating over a dictionary being modified 
            // is safe with ConcurrentDictionary, but we want 1:1 sync logic.
            lock (_lock)
            {
                foreach (var entry in _loggedClients)
                {
                    IChatObserver chatClient = entry.Value;
                    string userId = entry.Key;

                    Task.Run(() =>
                    {
                        try
                        {
                            chatClient.DomainDataChanged(entityType);
                        }
                        catch (ChatException e)
                        {
                            Logger.Error($"Error notifying domain change for {userId}", e);
                        }
                    });
                }
            }
        }
    }
}