
using Ticketing.Model.Domain;
using Ticketing.Networking.Dto;

namespace ChatNetworking.dto
{
    public static class DtoUtils
    {
        // --- User Conversions ---
        
        public static User FromDto(UserDto usDto)
        {
            return new User(usDto.Id, usDto.Passwd);
        }

        public static UserDto ToDto(User user)
        {
            return new UserDto(user.Id, user.Passwd);
        }

        public static UserDto[] ToDtoArray(User[] users)
        {
            if (users == null) return Array.Empty<UserDto>();
            return users.Select(ToDto).ToArray();
        }

        public static User[] FromDtoArray(UserDto[] users)
        {
            if (users == null) return Array.Empty<User>();
            return users.Select(FromDto).ToArray();
        }

        // --- Message Conversions ---

        public static Message FromDto(MessageDto mDto)
        {
            User sender = new User(mDto.SenderId);
            User receiver = new User(mDto.ReceiverId);
            return new Message(sender, mDto.Text, receiver);
        }

        public static MessageDto ToDto(Message message)
        {
            return new MessageDto(message.Sender.Id, message.Text, message.Receiver.Id);
        }

        // --- Artist Conversions ---

        public static ArtistDto? ToDto(Artist? artist)
        {
            if (artist == null) return null;
            return new ArtistDto(artist.Id, artist.Name);
        }

        public static Artist? ToEntity(ArtistDto? dto)
        {
            if (dto == null) return null;
            return new Artist(dto.Id ?? 0, dto.Name);
        }

        public static ArtistDto[] ToArtistDtoArray(IList<Artist> artists)
        {
            if (artists == null) return Array.Empty<ArtistDto>();
            return artists.Select(ToDto).Where(d => d != null).Cast<ArtistDto>().ToArray();
        }

        public static List<Artist> ToArtistList(ArtistDto[] dtos)
        {
            if (dtos == null) return new List<Artist>();
            return dtos.Select(ToEntity).Where(e => e != null).Cast<Artist>().ToList();
        }

        // --- Spectacle Conversions ---

        public static SpectacleDto? ToDto(Spectacle? spectacle)
        {
            if (spectacle == null) return null;
            return new SpectacleDto(spectacle.Id, spectacle.Name, spectacle.Start_date,
                spectacle.Duration, spectacle.Capacity, spectacle.Location);
        }

        public static Spectacle? ToEntity(SpectacleDto? dto)
        {
            if (dto == null) return null;
            return new Spectacle(dto.Id ?? 0, dto.Name, dto.StartDate,
                dto.Duration, dto.Capacity, dto.Location);
        }

        public static SpectacleDto[] ToSpectacleDtoArray(IList<Spectacle> spectacles)
        {
            if (spectacles == null) return Array.Empty<SpectacleDto>();
            return spectacles.Select(ToDto).Where(d => d != null).Cast<SpectacleDto>().ToArray();
        }

        public static List<Spectacle> ToSpectacleList(SpectacleDto[] dtos)
        {
            if (dtos == null) return new List<Spectacle>();
            return dtos.Select(ToEntity).Where(e => e != null).Cast<Spectacle>().ToList();
        }

        // --- Ticket Conversions ---

        public static TicketDto? ToDto(Ticket? ticket)
        {
            if (ticket == null) return null;
            return new TicketDto(ticket.Id, ticket.Price, ticket.SpectacleId);
        }

        public static Ticket? ToEntity(TicketDto? dto)
        {
            if (dto == null) return null;
            return new Ticket(dto.Id ?? 0, dto.Price, dto.SpectacleId ?? 0);
        }

        public static TicketDto[] ToTicketDtoArray(IList<Ticket> tickets)
        {
            if (tickets == null) return Array.Empty<TicketDto>();
            return tickets.Select(ToDto).Where(d => d != null).Cast<TicketDto>().ToArray();
        }

        public static List<Ticket> ToTicketList(TicketDto[] dtos)
        {
            if (dtos == null) return new List<Ticket>();
            return dtos.Select(ToEntity).Where(e => e != null).Cast<Ticket>().ToList();
        }

        // --- ArtistSpectacle Conversions ---

        public static ArtistSpectacleDto? ToDto(ArtistSpectacle? @as)
        {
            if (@as == null) return null;
            return new ArtistSpectacleDto(@as.Id, @as.ArtistId, @as.SpectacleId);
        }

        public static ArtistSpectacle? ToEntity(ArtistSpectacleDto? dto)
        {
            if (dto == null) return null;
            return new ArtistSpectacle(dto.Id ?? 0, dto.ArtistId ?? 0, dto.SpectacleId ?? 0);
        }

        public static ArtistSpectacleDto[] ToArtistSpectacleDtoArray(IList<ArtistSpectacle> list)
        {
            if (list == null) return Array.Empty<ArtistSpectacleDto>();
            return list.Select(ToDto).Where(d => d != null).Cast<ArtistSpectacleDto>().ToArray();
        }

        public static List<ArtistSpectacle> ToArtistSpectacleList(ArtistSpectacleDto[] dtos)
        {
            if (dtos == null) return new List<ArtistSpectacle>();
            return dtos.Select(ToEntity).Where(e => e != null).Cast<ArtistSpectacle>().ToList();
        }

        // --- Buyer Conversions ---

        public static BuyerDto? ToDto(Buyer? entity)
        {
            if (entity == null) return null;
            return new BuyerDto(entity.Id, entity.Name, entity.Email);
        }

        public static Buyer? ToEntity(BuyerDto? dto)
        {
            if (dto == null) return null;
            return new Buyer(dto.Id ?? 0, dto.Name, dto.Email);
        }

        public static BuyerDto[] ToBuyerDtoArray(IList<Buyer> buyers)
        {
            if (buyers == null) return Array.Empty<BuyerDto>();
            return buyers.Select(ToDto).Where(d => d != null).Cast<BuyerDto>().ToArray();
        }

        public static List<Buyer> ToBuyerList(BuyerDto[] dtos)
        {
            if (dtos == null) return new List<Buyer>();
            return dtos.Select(ToEntity).Where(e => e != null).Cast<Buyer>().ToList();
        }

        // --- TicketSale Conversions ---

        public static TicketSaleDto? ToDto(TicketSale? ticketSale)
        {
            if (ticketSale == null) return null;
            return new TicketSaleDto(
                ticketSale.Id,
                ticketSale.SpectacleId,
                ticketSale.BuyerId,
                ticketSale.BuyerName,
                ticketSale.Seats,
                ticketSale.SoldAt,
                ticketSale.SpectacleName
            );
        }

        public static TicketSale? ToEntity(TicketSaleDto? dto)
        {
            if (dto == null) return null;
            return new TicketSale(
                dto.Id ?? 0,
                dto.SpectacleId ?? 0,
                dto.BuyerId ?? 0,
                dto.BuyerName,
                dto.Seats,
                dto.SoldAt,
                dto.SpectacleName
            );
        }

        public static TicketSaleDto[] ToTicketSaleDtoArray(IList<TicketSale> sales)
        {
            if (sales == null) return Array.Empty<TicketSaleDto>();
            return sales.Select(ToDto).Where(d => d != null).Cast<TicketSaleDto>().ToArray();
        }

        public static List<TicketSale> ToTicketSaleList(TicketSaleDto[] dtos)
        {
            if (dtos == null) return new List<TicketSale>();
            return dtos.Select(ToEntity).Where(e => e != null).Cast<TicketSale>().ToList();
        }
    }
}