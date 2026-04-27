using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        // Kept for backward compatibility if called synchronously elsewhere
        public void SetLoggedFriends()
        {
            _ = SetLoggedFriendsAsync();
        }

        // REFACTOR: Load online friends in the background
        public async Task SetLoggedFriendsAsync()
        {
            if (_server == null || _user == null) return;

            try
            {
                var loggedFriends = await Task.Run(() => _server.GetLoggedFriends(_user));
                
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
            catch (Exception e)
            {
                Logger.Error("Failed to load logged friends", e);
            }
        }

        // REFACTOR: Async message sending
        private async void HandleSendMessage(object sender, RoutedEventArgs e)
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

            // Extract receiver on UI thread before jumping to Task
            User receiver = _friendsList[index];
            
            // Prevent user from spamming the send button
            this.IsEnabled = false;

            try
            {
                await SendMessageAsync(receiver, msg);
                
                RcvMsgTxt.AppendText($"[me]: {msg}\n");
                RcvMsgTxt.ScrollToEnd();
                MsgTxt.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your server probably closed connection: " + ex.Message, "Communication error");
                Logger.Error("Send message error", ex);
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        private async Task SendMessageAsync(User receiver, string txtMsg)
        {
            if (_user == null) return;
            
            User sender = new User(_user.Id);
            Message message = new Message(sender, txtMsg, receiver);
            
            await Task.Run(() => _server?.SendMessage(message));
        }

        // REFACTOR: Async Logout
        private async void HandleLogout(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            await LogoutAsync();
            this.Close(); // Or this.Hide() depending on your navigation logic
        }

        private async Task LogoutAsync()
        {
            try
            {
                if (_user != null)
                {
                    await Task.Run(() => _server?.Logout(_user, this));
                }
            }
            catch (Exception e)
            {
                Logger.Error("Logout error " + e);
            }
        }

        // --- IChatObserver Implementation ---

        // REFACTOR: Prevent deadlocks from rapid-fire incoming messages
        public void MessageReceived(Message message)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                RcvMsgTxt.AppendText($"{message.Sender.Id}: {message.Text}\n");
                RcvMsgTxt.ScrollToEnd(); // Auto-scroll to latest message
            }));
        }

        // REFACTOR: Prevent deadlocks on login notifications
        public void FriendLoggedIn(User friend)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Prevent duplicates just in case
                bool exists = false;
                foreach(var f in _friendsList) { if (f.Id == friend.Id) exists = true; }
                
                if (!exists) 
                {
                    _friendsList.Add(friend);
                    Logger.Debug($"Friend logged in: {friend.Id}");
                }
            }));
        }

        // REFACTOR: Prevent deadlocks on logout notifications
        public void FriendLoggedOut(User friend)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < _friendsList.Count; i++)
                {
                    if (_friendsList[i].Id == friend.Id)
                    {
                        _friendsList.RemoveAt(i);
                        Logger.Debug($"Friend logged out: {friend.Id}");
                        break;
                    }
                }
            }));
        }

        public void DomainDataChanged(string entityType)
        {
            // Chat screen ignores domain CRUD notifications.
        }
    }
}