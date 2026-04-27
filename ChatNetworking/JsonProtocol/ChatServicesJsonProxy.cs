using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using ChatNetworking.dto;
using Ticketing.Model.Domain;
using Ticketing.Networking.Dto;

using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class ChatServicesJsonProxy : IChatServices, INetworkProtocol
    {
        private readonly string _host;
        private readonly int _port;

        private IChatObserver? _client;

        private StreamReader? _input;
        private StreamWriter? _output;
        private TcpClient? _connection;
        private JsonSerializerOptions _jsonOptions;

        private readonly BlockingCollection<Response<object>> _qresponses;
        private volatile bool _finished;
        private readonly object _lock = new object();

        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(ChatServicesJsonProxy));

        public ChatServicesJsonProxy(string host, int port)
        {
            _host = host;
            _port = port;
            _qresponses = new BlockingCollection<Response<object>>();
            _jsonOptions = JsonFactory.Create();
        }

        // --- IChatServices Implementation ---

        public virtual void Login(User user, IChatObserver client)
        {
            lock (_lock)
            {
                InitializeConnection();
                // user.Passwd = TextUtils.SimpleEncode(user.Passwd); // Add your encoding if needed
                Request req = JsonProtocolUtils.CreateLoginRequest(user);
                Response<object> response = SendRequestAndRead(req);

                if (response.Type == ResponseType.Ok)
                {
                    _client = client;
                    return;
                }
                if (response.Type == ResponseType.Error)
                {
                    CloseConnection();
                    throw new ChatException(response.ErrorMessage);
                }
            }
        }

        public virtual void SendMessage(Message message)
        {
            lock (_lock)
            {
                Request req = JsonProtocolUtils.CreateSendMessageRequest(message);
                Response<object> response = SendRequestAndRead(req);
                if (response.Type == ResponseType.Error)
                {
                    throw new ChatException(response.ErrorMessage);
                }
            }
        }

        public virtual void Logout(User user, IChatObserver client)
        {
            lock (_lock)
            {
                Request req = JsonProtocolUtils.CreateLogoutRequest(user);
                Response<object> response = SendRequestAndRead(req);
                CloseConnection();
                if (response.Type == ResponseType.Error)
                {
                    throw new ChatException(response.ErrorMessage);
                }
            }
        }

        public virtual User[] GetLoggedFriends(User user)
        {
            lock (_lock)
            {
                Request req = JsonProtocolUtils.CreateLoggedFriendsRequest(user);
                Response<object> response = SendRequestAndRead(req);
                if (response.Type == ResponseType.Error)
                {
                    throw new ChatException(response.ErrorMessage);
                }

                UserDto[] dtos = ConvertTo<UserDto[]>(response.Data);
                return DtoUtils.FromDtoArray(dtos);
            }
        }

        public void DomainDataChanged(string entityType)
        {
            // Usually empty on proxy side as per Java
        }

        // --- INetworkProtocol / Domain API (Remote Methods) ---

        public virtual IList<Artist> GetAllArtistsRemote()
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetAllArtists };
                Response<object> response = SendDomainRequest(request);
                ArtistDto[] dtos = ConvertTo<ArtistDto[]>(response.Data);
                return DtoUtils.ToArtistList(dtos);
            }
        }

        public virtual void AddArtistRemote(string name)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.AddArtist, ArtistName = name };
                SendDomainRequest(request);
            }
        }

        public virtual void UpdateArtistRemote(Artist artist)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.UpdateArtist, Artist = DtoUtils.ToDto(artist) };
                SendDomainRequest(request);
            }
        }

        public virtual void DeleteArtistRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.DeleteArtist, Id = id };
                SendDomainRequest(request);
            }
        }

        public virtual Artist? GetArtistByIdRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetArtist, Id = id };
                Response<object> response = SendDomainRequest(request);
                ArtistDto? dto = ConvertTo<ArtistDto>(response.Data);
                return DtoUtils.ToEntity(dto);
            }
        }

        public virtual IList<Spectacle> GetAllSpectaclesRemote()
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetAllSpectacles };
                Response<object> response = SendDomainRequest(request);
                SpectacleDto[] dtos = ConvertTo<SpectacleDto[]>(response.Data);
                return DtoUtils.ToSpectacleList(dtos);
            }
        }

        public virtual void AddSpectacleRemote(string name, DateTime date, int duration, int capacity, string location)
        {
            lock (_lock)
            {
                Spectacle spec = new Spectacle(name, date, duration, capacity, location);
                Request request = new Request { Type = RequestType.AddSpectacle, Spectacle = DtoUtils.ToDto(spec) };
                SendDomainRequest(request);
            }
        }

        public virtual void UpdateSpectacleRemote(Spectacle spectacle)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.UpdateSpectacle, Spectacle = DtoUtils.ToDto(spectacle) };
                SendDomainRequest(request);
            }
        }

        public virtual void DeleteSpectacleRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.DeleteSpectacle, Id = id };
                SendDomainRequest(request);
            }
        }

        public virtual Spectacle? GetSpectacleByIdRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetSpectacle, Id = id };
                Response<object> response = SendDomainRequest(request);
                SpectacleDto? dto = ConvertTo<SpectacleDto>(response.Data);
                return DtoUtils.ToEntity(dto);
            }
        }

        public virtual IList<Ticket> GetAllTicketsRemote()
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetAllTickets };
                Response<object> response = SendDomainRequest(request);
                TicketDto[] dtos = ConvertTo<TicketDto[]>(response.Data);
                return DtoUtils.ToTicketList(dtos);
            }
        }

        public virtual void AddTicketRemote(double price, long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.AddTicket, TicketPrice = price, SpectacleId = spectacleId };
                SendDomainRequest(request);
            }
        }

        public virtual void UpdateTicketRemote(Ticket ticket)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.UpdateTicket, Ticket = DtoUtils.ToDto(ticket) };
                SendDomainRequest(request);
            }
        }

        public virtual void DeleteTicketRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.DeleteTicket, Id = id };
                SendDomainRequest(request);
            }
        }

        public virtual Ticket? GetTicketByIdRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetTicket, Id = id };
                Response<object> response = SendDomainRequest(request);
                TicketDto? dto = ConvertTo<TicketDto>(response.Data);
                return DtoUtils.ToEntity(dto);
            }
        }

        public virtual IList<Ticket> GetTicketsBySpectacleIdRemote(long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetTicketsBySpectacle, SpectacleId = spectacleId };
                Response<object> response = SendDomainRequest(request);
                TicketDto[] dtos = ConvertTo<TicketDto[]>(response.Data);
                return DtoUtils.ToTicketList(dtos);
            }
        }

        public virtual IList<ArtistSpectacle> GetAllArtistSpectaclesRemote()
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetAllArtistSpectacles };
                Response<object> response = SendDomainRequest(request);
                ArtistSpectacleDto[] dtos = ConvertTo<ArtistSpectacleDto[]>(response.Data);
                return DtoUtils.ToArtistSpectacleList(dtos);
            }
        }

        public virtual void AddArtistSpectacleRemote(long artistId, long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.AddArtistSpectacle, ArtistId = artistId, SpectacleId = spectacleId };
                SendDomainRequest(request);
            }
        }

        public virtual void DeleteArtistSpectacleRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.DeleteArtistSpectacle, Id = id };
                SendDomainRequest(request);
            }
        }

        public virtual ArtistSpectacle? GetArtistSpectacleByIdRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetArtistSpectacle, Id = id };
                Response<object> response = SendDomainRequest(request);
                ArtistSpectacleDto? dto = ConvertTo<ArtistSpectacleDto>(response.Data);
                return DtoUtils.ToEntity(dto);
            }
        }

        public virtual IList<ArtistSpectacle> GetByArtistIdRemote(long artistId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetByArtistId, ArtistId = artistId };
                Response<object> response = SendDomainRequest(request);
                ArtistSpectacleDto[] dtos = ConvertTo<ArtistSpectacleDto[]>(response.Data);
                return DtoUtils.ToArtistSpectacleList(dtos);
            }
        }

        public virtual IList<ArtistSpectacle> GetBySpectacleIdRemote(long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetBySpectacleId, SpectacleId = spectacleId };
                Response<object> response = SendDomainRequest(request);
                ArtistSpectacleDto[] dtos = ConvertTo<ArtistSpectacleDto[]>(response.Data);
                return DtoUtils.ToArtistSpectacleList(dtos);
            }
        }

        public virtual void DeleteByArtistAndSpectacleRemote(long artistId, long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.DeleteByArtistAndSpectacle, ArtistId = artistId, SpectacleId = spectacleId };
                SendDomainRequest(request);
            }
        }

        public virtual bool ExistsRelationRemote(long artistId, long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.ExistsRelation, ArtistId = artistId, SpectacleId = spectacleId };
                Response<object> response = SendDomainRequest(request);
                return ConvertTo<bool>(response.Data);
            }
        }

        public virtual Buyer GetOrCreateBuyerRemote(string email, string name)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetOrCreateBuyer, BuyerEmail = email, BuyerName = name };
                Response<object> response = SendDomainRequest(request);
                BuyerDto? dto = ConvertTo<BuyerDto>(response.Data);
                return DtoUtils.ToEntity(dto)!;
            }
        }

        public virtual Buyer GetOrCreateBuyerRemote(long buyerId, string buyerName)
        {
            lock (_lock)
            {
                // 1. Initialize Request with the ID-based parameters
                Request request = new Request 
                { 
                    Type = RequestType.GetOrCreateBuyer, 
                    BuyerId = buyerId, 
                    BuyerName = buyerName 
                };

                // 2. Send request through the established domain channel
                Response<object> response = SendDomainRequest(request);

                // 3. Convert the generic response data to the BuyerDto
                BuyerDto? dto = ConvertTo<BuyerDto>(response.Data);

                // 4. Transform the DTO back to the Domain Entity
                return DtoUtils.ToEntity(dto)!;
            }
        }
        
        public virtual Buyer? GetBuyerByEmailRemote(string email)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetBuyerByEmail, BuyerEmail = email };
                Response<object> response = SendDomainRequest(request);
                if (response.Data == null) return null;
                BuyerDto? dto = ConvertTo<BuyerDto>(response.Data);
                return DtoUtils.ToEntity(dto);
            }
        }

        public virtual IList<Buyer> GetAllBuyersRemote()
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetAllBuyers };
                Response<object> response = SendDomainRequest(request);
                BuyerDto[] dtos = ConvertTo<BuyerDto[]>(response.Data);
                return DtoUtils.ToBuyerList(dtos);
            }
        }

        
        public virtual Buyer? GetBuyerByIdRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetBuyer, Id = id };
                Response<object> response = SendDomainRequest(request);
                BuyerDto? dto = ConvertTo<BuyerDto>(response.Data);
                return DtoUtils.ToEntity(dto);
            }
        }

        public virtual void DeleteBuyerRemote(long id)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.DeleteBuyer, Id = id };
                SendDomainRequest(request);
            }
        }

        public virtual IList<TicketSale> GetAllTicketSalesRemote()
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetAllTicketSales };
                Response<object> response = SendDomainRequest(request);
                TicketSaleDto[] dtos = ConvertTo<TicketSaleDto[]>(response.Data);
                return DtoUtils.ToTicketSaleList(dtos);
            }
        }

        public virtual IList<TicketSale> GetTicketSalesByBuyerRemote(long buyerId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetTicketSalesByBuyer, BuyerId = buyerId };
                Response<object> response = SendDomainRequest(request);
                TicketSaleDto[] dtos = ConvertTo<TicketSaleDto[]>(response.Data);
                return DtoUtils.ToTicketSaleList(dtos);
            }
        }

        public virtual TicketSale SellTicketRemote(long spectacleId, string buyerEmail, int seats)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.SellTicket, SpectacleId = spectacleId, BuyerEmail = buyerEmail, Seats = seats };
                Response<object> response = SendDomainRequest(request);
                TicketSaleDto? dto = ConvertTo<TicketSaleDto>(response.Data);
                return DtoUtils.ToEntity(dto)!;
            }
        }

        public virtual void IncreaseTicketSeatsRemote(long ticketSaleId, int extraSeats)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.IncreaseTicketSeats, Id = ticketSaleId, ExtraSeats = extraSeats };
                SendDomainRequest(request);
            }
        }

        public virtual int GetSoldSeatsForSpectacleRemote(long spectacleId)
        {
            lock (_lock)
            {
                Request request = new Request { Type = RequestType.GetSoldSeatsForSpectacle, SpectacleId = spectacleId };
                Response<object> response = SendDomainRequest(request);
                return ConvertTo<int>(response.Data);
            }
        }

        // --- Internal Utility Methods ---

        private Response<object> SendDomainRequest(Request request)
        {
            try
            {
                Response<object> response = SendRequestAndRead(request);
                if (response.Type == ResponseType.Error)
                {
                    throw new Exception(response.ErrorMessage);
                }
                return response;
            }
            catch (ChatException e)
            {
                throw new Exception(e.Message, e);
            }
        }

        private void CloseConnection()
        {
            _finished = true;
            try
            {
                _input?.Close();
                _output?.Close();
                _connection?.Close();
                _client = null;
            }
            catch (IOException e)
            {
                Logger.Error("Error closing client connection", e);
            }
        }

        private Response<object> SendRequestAndRead(Request request)
        {
            EnsureConnected();
            SendRequest(request);
            return ReadResponse();
        }

        private void EnsureConnected()
        {
            if (_connection == null || !_connection.Connected || _finished)
            {
                InitializeConnection();
            }
        }

        private void SendRequest(Request request)
        {
            string reqLine = JsonSerializer.Serialize(request, _jsonOptions);
            try
            {
                _output?.WriteLine(reqLine);
                _output?.Flush();
            }
            catch (Exception e)
            {
                throw new ChatException("Error sending request", e);
            }
        }

        private Response<object> ReadResponse()
        {
            try
            {
                return _qresponses.Take();
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
                throw new ChatException("Interrupted while waiting for server response");
            }
        }

        private T? ConvertTo<T>(object? data)
        {
            if (data == null) return default;
            if (data is T result) return result;

            // Handle System.Text.Json element conversion
            if (data is JsonElement element)
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText(), _jsonOptions);
            }
            return default;
        }

        private void InitializeConnection()
        {
            try
            {
                _connection = new TcpClient(_host, _port);
                var stream = _connection.GetStream();
                _output = new StreamWriter(stream) { AutoFlush = true };
                _input = new StreamReader(stream);
                _finished = false;
                StartReader();
            }
            catch (Exception e)
            {
                Logger.Error($"Cannot initialize connection to {_host}:{_port}", e);
                throw new ChatException($"Nu ma pot conecta la serverul {_host}:{_port}. Porneste serverul mai intai.", e);
            }
        }

        private void StartReader()
        {
            Thread tw = new Thread(ReaderThreadLoop) { IsBackground = true };
            tw.Start();
        }

        private void HandleUpdate(Response<object> response)
        {
            if (response.Type == ResponseType.FriendLoggedIn)
            {
                User friend = DtoUtils.FromDto(ConvertTo<UserDto>(response.Data)!);
                _client?.FriendLoggedIn(friend);
            }
            else if (response.Type == ResponseType.FriendLoggedOut)
            {
                User friend = DtoUtils.FromDto(ConvertTo<UserDto>(response.Data)!);
                _client?.FriendLoggedOut(friend);
            }
            else if (response.Type == ResponseType.NewMessage)
            {
                Message message = DtoUtils.FromDto(ConvertTo<MessageDto>(response.Data)!);
                _client?.MessageReceived(message);
            }
            else if (response.Type == ResponseType.DomainDataChanged)
            {
                string entityType = response.Data?.ToString() ?? "ALL";
                _client?.DomainDataChanged(entityType);
            }
        }

        private bool IsUpdate(Response<object> response)
        {
            return response.Type == ResponseType.FriendLoggedOut
                   || response.Type == ResponseType.FriendLoggedIn
                   || response.Type == ResponseType.NewMessage
                   || response.Type == ResponseType.DomainDataChanged;
        }

        private void ReaderThreadLoop()
        {
            while (!_finished)
            {
                try
                {
                    string? responseLine = _input?.ReadLine();
                    if (responseLine == null)
                    {
                        _finished = true;
                        break;
                    }
                    var response = JsonSerializer.Deserialize<Response<object>>(responseLine, _jsonOptions);
                    if (response == null) continue;

                    if (IsUpdate(response))
                    {
                        HandleUpdate(response);
                    }
                    else
                    {
                        _qresponses.Add(response);
                    }
                }
                catch (Exception e)
                {
                    if (!_finished) Logger.Error("Reading error", e);
                    _finished = true;
                }
            }
        }
    }
}