
using ChatNetworking.dto;
using Ticketing.Model.Domain;
using Ticketing.Networking.Dto;

namespace ChatNetworking.JsonProtocol
{
    public static class JsonProtocolUtils
    {
        // --- Response Helpers for Chat/Status ---

        public static Response<MessageDto> CreateNewMessageResponse(Message message)
        {
            return new Response<MessageDto>(ResponseType.NewMessage, DtoUtils.ToDto(message));
        }

        public static Response<UserDto> CreateFriendLoggedInResponse(User friend)
        {
            return new Response<UserDto>(ResponseType.FriendLoggedIn, DtoUtils.ToDto(friend));
        }

        public static Response<UserDto> CreateFriendLoggedOutResponse(User friend)
        {
            return new Response<UserDto>(ResponseType.FriendLoggedOut, DtoUtils.ToDto(friend));
        }

        public static Response<string> CreateDomainDataChangedResponse(string entityType)
        {
            return new Response<string>(ResponseType.DomainDataChanged, entityType);
        }

        public static Response<UserDto[]> CreateGetLoggedFriendsResponse(User[] friends)
        {
            return new Response<UserDto[]>(ResponseType.GetLoggedFriends, DtoUtils.ToDtoArray(friends));
        }

        // --- Request Helpers ---

        public static Request CreateLoginRequest(User user)
        {
            return new Request
            {
                Type = RequestType.Login,
                User = DtoUtils.ToDto(user)
            };
        }

        public static Request CreateSendMessageRequest(Message message)
        {
            return new Request
            {
                Type = RequestType.SendMessage,
                Message = DtoUtils.ToDto(message)
            };
        }

        public static Request CreateLogoutRequest(User user)
        {
            return new Request
            {
                Type = RequestType.Logout,
                User = DtoUtils.ToDto(user)
            };
        }

        public static Request CreateLoggedFriendsRequest(User user)
        {
            return new Request
            {
                Type = RequestType.GetLoggedFriends,
                User = DtoUtils.ToDto(user)
            };
        }

        // --- Generic Response Helpers ---

        public static Response<T> CreateOkResponse<T>()
        {
            return new Response<T>(ResponseType.Ok, data:default);
        }

        public static Response<T> CreateErrorResponse<T>(string errorMessage)
        {
            return new Response<T>
            {
                Type = ResponseType.Error,
                ErrorMessage = errorMessage
            };
        }

        public static Response<T> CreateSuccessResponse<T>()
        {
            return new Response<T>(ResponseType.Success, data:default);
        }

        // --- Entity List/Single Responses ---

        public static Response<ArtistDto[]> CreateArtistsListResponse(IList<Artist> artists)
        {
            return new Response<ArtistDto[]>(ResponseType.ArtistsList, DtoUtils.ToArtistDtoArray(artists));
        }

        public static Response<ArtistDto> CreateArtistResponse(Artist artist)
        {
            return new Response<ArtistDto>(ResponseType.Artist, DtoUtils.ToDto(artist));
        }

        public static Response<SpectacleDto[]> CreateSpectaclesListResponse(IList<Spectacle> spectacles)
        {
            return new Response<SpectacleDto[]>(ResponseType.SpectaclesList, DtoUtils.ToSpectacleDtoArray(spectacles));
        }

        public static Response<SpectacleDto> CreateSpectacleResponse(Spectacle spectacle)
        {
            return new Response<SpectacleDto>(ResponseType.Spectacle, DtoUtils.ToDto(spectacle));
        }

        public static Response<TicketDto[]> CreateTicketsListResponse(IList<Ticket> tickets)
        {
            return new Response<TicketDto[]>(ResponseType.TicketsList, DtoUtils.ToTicketDtoArray(tickets));
        }

        public static Response<TicketDto> CreateTicketResponse(Ticket ticket)
        {
            return new Response<TicketDto>(ResponseType.Ticket, DtoUtils.ToDto(ticket));
        }

        public static Response<ArtistSpectacleDto[]> CreateArtistSpectaclesListResponse(IList<ArtistSpectacle> artistSpectacles)
        {
            return new Response<ArtistSpectacleDto[]>(ResponseType.ArtistSpectaclesList, DtoUtils.ToArtistSpectacleDtoArray(artistSpectacles));
        }

        public static Response<ArtistSpectacleDto> CreateArtistSpectacleResponse(ArtistSpectacle asEntity)
        {
            return new Response<ArtistSpectacleDto>(ResponseType.ArtistSpectacle, DtoUtils.ToDto(asEntity));
        }

        public static Response<bool> CreateBooleanResponse(bool value)
        {
            return new Response<bool>(ResponseType.RelationExists, value);
        }

        public static Response<BuyerDto[]> CreateBuyersListResponse(IList<Buyer> buyers)
        {
            return new Response<BuyerDto[]>(ResponseType.BuyersList, DtoUtils.ToBuyerDtoArray(buyers));
        }

        public static Response<BuyerDto> CreateBuyerResponse(Buyer buyer)
        {
            return new Response<BuyerDto>(ResponseType.Buyer, DtoUtils.ToDto(buyer));
        }

        public static Response<TicketSaleDto[]> CreateTicketSalesListResponse(IList<TicketSale> sales)
        {
            return new Response<TicketSaleDto[]>(ResponseType.TicketSalesList, DtoUtils.ToTicketSaleDtoArray(sales));
        }

        public static Response<TicketSaleDto> CreateTicketSaleResponse(TicketSale sale)
        {
            return new Response<TicketSaleDto>(ResponseType.TicketSale, DtoUtils.ToDto(sale));
        }

        public static Response<int> CreateIntegerResponse(int value)
        {
            return new Response<int>(ResponseType.IntegerValue, value);
        }
    }
}