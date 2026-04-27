
using System.Net.Sockets;
using System.Text.Json;
using ChatNetworking.dto;
using Ticketing.Model.Domain;
using Ticketing.Networking.Dto;
using Ticketing.Services;


namespace ChatNetworking.JsonProtocol
{
    public class ChatClientJsonWorker : IChatObserver
    {
        private readonly IServicesFacade _servicesFacade;
        private readonly TcpClient _connection;

        private readonly StreamReader _input;
        private readonly StreamWriter _output;
        private readonly JsonSerializerOptions _jsonOptions;
        private volatile bool _connected;

        // Using a simplified logger pattern - you can swap for NLog/Log4Net
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(ChatServicesJsonProxy));

        public ChatClientJsonWorker(IServicesFacade servicesFacade, TcpClient connection)
        {
            _servicesFacade = servicesFacade;
            _connection = connection;
            _jsonOptions = JsonFactory.Create();
            try
            {
                var stream = _connection.GetStream();
                _output = new StreamWriter(stream) { AutoFlush = true };
                _input = new StreamReader(stream);
                _connected = true;
            }
            catch (IOException e)
            {
                Logger.Error("Error initializing worker streams", e);
            }
        }

        public virtual void Run()
        {
            while (_connected)
            {
                try
                {
                    string? requestLine = _input.ReadLine();
                    if (requestLine == null)
                    {
                        _connected = false;
                        break;
                    }

                    Request? request = JsonSerializer.Deserialize<Request>(requestLine, _jsonOptions);
                    if (request != null)
                    {
                        object response = HandleRequest(request);
                        if (response != null)
                        {
                            SendResponse(response);
                        }
                    }
                }
                catch (Exception e)
                {
                    _connected = false;
                    Logger.Debug("Client connection closed or error occurred", e);
                }
            }
            CloseConnection();
        }

        private void CloseConnection()
        {
            try
            {
                _input.Close();
                _output.Close();
                _connection.Close();
            }
            catch (IOException e)
            {
                Logger.Error("Error closing connection: " + e.Message);
            }
        }

        // --- IChatObserver Implementation (Push Notifications to Client) ---

        public void MessageReceived(Message message)
        {
            var resp = JsonProtocolUtils.CreateNewMessageResponse(message);
            Logger.Debug($"Message received: {message}");
            SendResponse(resp);
        }

        public void FriendLoggedIn(User friend)
        {
            var resp = JsonProtocolUtils.CreateFriendLoggedInResponse(friend);
            Logger.Debug($"Friend logged in: {friend}");
            SendResponse(resp);
        }

        public void FriendLoggedOut(User friend)
        {
            var resp = JsonProtocolUtils.CreateFriendLoggedOutResponse(friend);
            Logger.Debug($"Friend logged out: {friend}");
            SendResponse(resp);
        }

        public void DomainDataChanged(string entityType)
        {
            var resp = JsonProtocolUtils.CreateDomainDataChangedResponse(entityType);
            Logger.Debug($"Domain changed: {entityType}");
            SendResponse(resp);
        }

        // --- Request Handling (The Big Switch) ---

        private object HandleRequest(Request request)
        {
            try
            {
                switch (request.Type)
                {
                    case RequestType.Login:
                        Logger.Debug("Login request for user");
                        User user = DtoUtils.FromDto(request.User);
                        // Assuming TextUtils is ported or simple decoding
                        _servicesFacade.Login(user, this);
                        return JsonProtocolUtils.CreateOkResponse<object>();

                    case RequestType.Logout:
                        User logoutUser = DtoUtils.FromDto(request.User);
                        _servicesFacade.Logout(logoutUser, this);
                        _connected = false;
                        return JsonProtocolUtils.CreateOkResponse<object>();

                    case RequestType.SendMessage:
                        Message message = DtoUtils.FromDto(request.Message);
                        _servicesFacade.SendMessage(message);
                        return JsonProtocolUtils.CreateOkResponse<object>();

                    case RequestType.GetLoggedFriends:
                        User loggedUser = DtoUtils.FromDto(request.User);
                        User[] friends = _servicesFacade.GetLoggedFriends(loggedUser);
                        return JsonProtocolUtils.CreateGetLoggedFriendsResponse(friends);

                    case RequestType.GetAllArtists:
                        return JsonProtocolUtils.CreateArtistsListResponse(_servicesFacade.GetAllArtists());

                    case RequestType.GetArtist:
                        var artist = _servicesFacade.GetArtistById(request.Id ?? 0);
                        return artist != null 
                            ? JsonProtocolUtils.CreateArtistResponse(artist) 
                            : JsonProtocolUtils.CreateErrorResponse<ArtistDto>("Artist not found");

                    case RequestType.AddArtist:
                        _servicesFacade.AddArtist(request.ArtistName);
                        NotifyDomainChange("ARTISTS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.UpdateArtist:
                        _servicesFacade.UpdateArtist(DtoUtils.ToEntity(request.Artist));
                        NotifyDomainChange("ARTISTS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.DeleteArtist:
                        _servicesFacade.DeleteArtist(request.Id ?? 0);
                        NotifyDomainChange("ARTISTS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.GetAllSpectacles:
                        return JsonProtocolUtils.CreateSpectaclesListResponse(_servicesFacade.GetAllSpectacles());

                    case RequestType.GetSpectacle:
                        var spec = _servicesFacade.GetSpectacleById(request.Id ?? 0);
                        return spec != null 
                            ? JsonProtocolUtils.CreateSpectacleResponse(spec) 
                            : JsonProtocolUtils.CreateErrorResponse<SpectacleDto>("Spectacle not found");

                    case RequestType.SellTicket:
                        TicketSale sale = _servicesFacade.SellTicket(
                            request.SpectacleId ?? 0,
                            request.BuyerEmail,
                            request.Seats ?? 0
                        );
                        NotifyDomainChange("SALES");
                        NotifyDomainChange("SPECTACLES");
                        NotifyDomainChange("BUYERS");
                        return JsonProtocolUtils.CreateTicketSaleResponse(sale);

                    // ... Implement other cases (GET_ALL_TICKETS, BUYERS, etc.) exactly like above ...

                    default:
                        return JsonProtocolUtils.CreateErrorResponse<object>("Unknown request type");
                }
            }
            catch (Exception e)
            {
                if (request.Type == RequestType.Login) _connected = false;
                return JsonProtocolUtils.CreateErrorResponse<object>(e.Message);
            }
        }

        private void NotifyDomainChange(string entityType)
        {
            try
            {
                _servicesFacade.DomainDataChanged(entityType);
            }
            catch (Exception e)
            {
                Logger.Debug($"Domain update failed for {entityType}: {e.Message}");
            }
        }

        private void SendResponse(object response)
        {
            string responseLine = JsonSerializer.Serialize(response, _jsonOptions);
            Logger.Debug("Sending response: " + responseLine);
            lock (_output)
            {
                _output.WriteLine(responseLine);
                _output.Flush();
            }
        }
    }
}