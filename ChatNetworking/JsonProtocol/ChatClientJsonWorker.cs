using System;
using System.IO;
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

        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(ChatClientJsonWorker));

        public ChatClientJsonWorker(IServicesFacade servicesFacade, TcpClient connection)
        {
            _servicesFacade = servicesFacade;
            _connection = connection;
            _jsonOptions = GsonFactory.Create();
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
                Logger.Debug("Handling request: " + request.Type);
                switch (request.Type)
                {
                    // --- Chat / User Operations ---
                    case RequestType.Login:
                        Logger.Debug("Login request for user");
                        User user = DtoUtils.FromDto(request.User);
                        // user.Passwd = TextUtils.SimpleDecode(user.Passwd); // Uncomment if decoding is needed
                        _servicesFacade.Login(user, this);
                        return JsonProtocolUtils.CreateOkResponse<object>();

                    case RequestType.Logout:
                        Logger.Debug("Logout request");
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

                    // --- Artist Operations ---
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

                    // --- Spectacle Operations ---
                    case RequestType.GetAllSpectacles:
                        return JsonProtocolUtils.CreateSpectaclesListResponse(_servicesFacade.GetAllSpectacles());

                    case RequestType.GetSpectacle:
                        var spec = _servicesFacade.GetSpectacleById(request.Id ?? 0);
                        return spec != null 
                            ? JsonProtocolUtils.CreateSpectacleResponse(spec) 
                            : JsonProtocolUtils.CreateErrorResponse<SpectacleDto>("Spectacle not found");

                    case RequestType.AddSpectacle:
                        Spectacle newSpec = DtoUtils.ToEntity(request.Spectacle);
                        // Make sure your Domain model property matches (e.g. StartDate instead of getStart_date())
                        _servicesFacade.AddSpectacle(newSpec.Name, newSpec.Start_date, newSpec.Duration, newSpec.Capacity, newSpec.Location);
                        NotifyDomainChange("SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.UpdateSpectacle:
                        Spectacle spectacleToUpdate = DtoUtils.ToEntity(request.Spectacle);
                        _servicesFacade.UpdateSpectacle(spectacleToUpdate);
                        NotifyDomainChange("SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.DeleteSpectacle:
                        _servicesFacade.DeleteSpectacle(request.Id ?? 0);
                        NotifyDomainChange("SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    // --- Ticket Operations ---
                    case RequestType.GetAllTickets:
                        return JsonProtocolUtils.CreateTicketsListResponse(_servicesFacade.GetAllTickets());

                    case RequestType.GetTicket:
                        var ticket = _servicesFacade.GetTicketById(request.Id ?? 0);
                        return ticket != null 
                            ? JsonProtocolUtils.CreateTicketResponse(ticket) 
                            : JsonProtocolUtils.CreateErrorResponse<TicketDto>("Ticket not found");

                    case RequestType.AddTicket:
                        _servicesFacade.AddTicket(request.TicketPrice ?? 0, request.SpectacleId ?? 0);
                        NotifyDomainChange("TICKETS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.UpdateTicket:
                        Ticket ticketToUpdate = DtoUtils.ToEntity(request.Ticket);
                        _servicesFacade.UpdateTicket(ticketToUpdate);
                        NotifyDomainChange("TICKETS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.DeleteTicket:
                        _servicesFacade.DeleteTicket(request.Id ?? 0);
                        NotifyDomainChange("TICKETS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.GetTicketsBySpectacle:
                        return JsonProtocolUtils.CreateTicketsListResponse(_servicesFacade.GetTicketsBySpectacleId(request.SpectacleId ?? 0));

                    // --- ArtistSpectacle Operations ---
                    case RequestType.GetAllArtistSpectacles:
                        return JsonProtocolUtils.CreateArtistSpectaclesListResponse(_servicesFacade.GetAllArtistSpectacles());

                    case RequestType.GetArtistSpectacle:
                        var asEntity = _servicesFacade.GetArtistSpectacleById(request.Id ?? 0);
                        return asEntity != null 
                            ? JsonProtocolUtils.CreateArtistSpectacleResponse(asEntity) 
                            : JsonProtocolUtils.CreateErrorResponse<ArtistSpectacleDto>("ArtistSpectacle not found");

                    case RequestType.AddArtistSpectacle:
                        _servicesFacade.AddArtistSpectacle(request.ArtistId ?? 0, request.SpectacleId ?? 0);
                        NotifyDomainChange("ARTIST_SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.DeleteArtistSpectacle:
                        _servicesFacade.DeleteArtistSpectacle(request.Id ?? 0);
                        NotifyDomainChange("ARTIST_SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.GetByArtistId:
                        return JsonProtocolUtils.CreateArtistSpectaclesListResponse(_servicesFacade.GetArtistSpectacleByArtistId(request.ArtistId ?? 0));

                    case RequestType.GetBySpectacleId:
                        return JsonProtocolUtils.CreateArtistSpectaclesListResponse(_servicesFacade.GetArtistSpectacleBySpectacleId(request.SpectacleId ?? 0));

                    case RequestType.DeleteByArtistAndSpectacle:
                        _servicesFacade.DeleteByArtistAndSpectacle(request.ArtistId ?? 0, request.SpectacleId ?? 0);
                        NotifyDomainChange("ARTIST_SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.ExistsRelation:
                        return JsonProtocolUtils.CreateBooleanResponse(_servicesFacade.ExistsRelationBetweenArtistAndSpectacle(request.ArtistId ?? 0, request.SpectacleId ?? 0));

                    // --- Buyer Operations ---
                    case RequestType.GetAllBuyers:
                        return JsonProtocolUtils.CreateBuyersListResponse(_servicesFacade.GetAllBuyers());

                    case RequestType.GetBuyer:
                        var buyer = _servicesFacade.GetBuyerById(request.Id ?? 0);
                        return buyer != null 
                            ? JsonProtocolUtils.CreateBuyerResponse(buyer) 
                            : JsonProtocolUtils.CreateErrorResponse<BuyerDto>("Buyer not found");

                    case RequestType.GetBuyerByEmail:
                        var buyerByEmail = _servicesFacade.GetBuyerByEmail(request.BuyerEmail);
                        return buyerByEmail != null 
                            ? JsonProtocolUtils.CreateBuyerResponse(buyerByEmail) 
                            : JsonProtocolUtils.CreateErrorResponse<BuyerDto>("Buyer not found with email: " + request.BuyerEmail);

                    case RequestType.GetOrCreateBuyer:
                        Buyer newOrExistingBuyer = _servicesFacade.GetOrCreateBuyer(request.BuyerEmail, request.BuyerName);
                        NotifyDomainChange("BUYERS");
                        return JsonProtocolUtils.CreateBuyerResponse(newOrExistingBuyer);

                    case RequestType.DeleteBuyer:
                        _servicesFacade.DeleteBuyer(request.Id ?? 0);
                        NotifyDomainChange("BUYERS");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    // --- TicketSale Operations ---
                    case RequestType.GetAllTicketSales:
                        return JsonProtocolUtils.CreateTicketSalesListResponse(_servicesFacade.GetAllTicketSales());

                    case RequestType.GetTicketSalesByBuyer:
                        return JsonProtocolUtils.CreateTicketSalesListResponse(_servicesFacade.GetTicketSalesByBuyer(request.BuyerId ?? 0));

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

                    case RequestType.IncreaseTicketSeats:
                        _servicesFacade.IncreaseTicketSeats(request.Id ?? 0, request.ExtraSeats ?? 0);
                        NotifyDomainChange("TICKET_SALES");
                        NotifyDomainChange("SPECTACLES");
                        return JsonProtocolUtils.CreateSuccessResponse<object>();

                    case RequestType.GetSoldSeatsForSpectacle:
                        return JsonProtocolUtils.CreateIntegerResponse(_servicesFacade.GetSoldSeatsForSpectacle(request.SpectacleId ?? 0));

                    default:
                        return JsonProtocolUtils.CreateErrorResponse<object>("Unknown request type");
                }
            }
            catch (Exception e)
            {
                // This will catch any missing parameters or DB errors and safely report them to the Client console.
                Logger.Error("ERROR IN HANDLE REQUEST: " + e.Message, e);
                return JsonProtocolUtils.CreateErrorResponse<object>(e.Message ?? "Unknown server error");
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