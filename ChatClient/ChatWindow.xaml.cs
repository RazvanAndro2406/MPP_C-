using System;
using System.Collections.ObjectModel;
using System.Windows;
using log4net;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatClient
{
    public partial class ChatWindow : Window, IChatObserver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatWindow));

        private IChatServices? _server;
        private User? _user;
        
        // C# version of ObservableList
        private ObservableCollection<User> _friendsList = new ObservableCollection<User>();

        public ChatWindow()
        {
            InitializeComponent();
            FriendsTable.ItemsSource = _friendsList;
            Logger.Debug("Constructor ChatWindow");
        }

        public void SetServer(IChatServices s)
        {
            _server = s;
        }

        public void SetUser(User user)
        {
            _user = user;
        }

        // Logic to load friends currently online
        public void SetLoggedFriends()
        {
            try
            {
                if (_server == null || _user == null) return;

                User[] loggedFriends = _server.GetLoggedFriends(_user);
                _friendsList.Clear();
                foreach (User u in loggedFriends)
                {
                    _friendsList.Add(u);
                }

                if (_friendsList.Count > 0)
                {
                    FriendsTable.SelectedIndex = 0;
                }
            }
            catch (ChatException e)
            {
                Logger.Error("Failed to load logged friends", e);
            }
        }

        private void HandleSendMessage(object sender, RoutedEventArgs e)
        {
            int index = FriendsTable.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Please select a friend from the list", "Message has no specified receiver");
                return;
            }

            string msg = MsgTxt.Text;
            if (string.IsNullOrWhiteSpace(msg))
            {
                MessageBox.Show("Please fill in the message field before sending", "Message empty");
                return;
            }

            try
            {
                SendMessage(index, msg);
                RcvMsgTxt.AppendText($"[me]: {msg}\n");
                MsgTxt.Clear();
            }
            catch (ChatException ex)
            {
                MessageBox.Show("Your server probably closed connection", "Communication error");
                Logger.Error("Send message error", ex);
            }
        }

        private void SendMessage(int indexFriend, string txtMsg)
        {
            if (_user == null) return;
            
            User sender = new User(_user.Id);
            User receiver = _friendsList[indexFriend];
            Message message = new Message(sender, txtMsg, receiver);
            
            _server?.SendMessage(message);
        }

        private void HandleLogout(object sender, RoutedEventArgs e)
        {
            Logout();
            this.Close(); // Or this.Hide() depending on your navigation logic
        }

        private void Logout()
        {
            try
            {
                if (_user != null)
                {
                    _server?.Logout(_user, this);
                }
            }
            catch (ChatException e)
            {
                Logger.Error("Logout error " + e);
            }
        }

        // --- IChatObserver Implementation ---

        public void MessageReceived(Message message)
        {
            // Java: Platform.runLater
            Dispatcher.Invoke(() => 
            {
                RcvMsgTxt.AppendText($"{message.Sender.Id}: {message.Text}\n");
                RcvMsgTxt.ScrollToEnd(); // Auto-scroll to latest message
            });
        }

        public void FriendLoggedIn(User friend)
        {
            Dispatcher.Invoke(() =>
            {
                _friendsList.Add(friend);
                Logger.Debug($"Friend logged in: {friend.Id}");
            });
        }

        public void FriendLoggedOut(User friend)
        {
            Dispatcher.Invoke(() =>
            {
                // In C#, we usually search by ID to find the object in the collection to remove
                for (int i = 0; i < _friendsList.Count; i++)
                {
                    if (_friendsList[i].Id == friend.Id)
                    {
                        _friendsList.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        public void DomainDataChanged(string entityType)
        {
            // Chat screen ignores domain CRUD notifications.
        }
    }
}