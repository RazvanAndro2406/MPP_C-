
namespace ChatNetworking.JsonProtocol
{
    [Serializable]
    public class Response<T>
    {
        // Properties with get; set; to replace Java getters/setters
        public ResponseType Type { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Matches Java: private T data;
        // This can hold UserDto, ArtistDto, ArtistDto[], etc.
        public T? Data { get; set; }

        // Java: public Response() {}
        public Response()
        {
        }

        // Java: public Response(ResponseType type, T data)
        public Response(ResponseType type, T data)
        {
            this.Type = type;
            this.Data = data;
        }

        // Java: public Response(ResponseType type, String errorMessage)
        public Response(ResponseType type, string errorMessage)
        {
            this.Type = type;
            this.ErrorMessage = errorMessage;
        }

        // 1:1 Match with Java toString() format
        public override string ToString()
        {
            // Note the single quotes around the error message to match Java
            return $"Response{{" +
                   $"type={Type}, " +
                   $"errorMessage='{ErrorMessage}', " +
                   $"data={Data}" +
                   $"}}";
        }
    }
}