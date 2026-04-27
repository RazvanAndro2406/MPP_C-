using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class UserDto
    {
        private string id;
        private string passwd;

        // Java: public UserDTO(String id)
        public UserDto(string id) : this(id, "")
        {
        }

        // Java: public UserDTO(String id, String passwd)
        public UserDto(string id, string passwd)
        {
            this.id = id;
            this.passwd = passwd;
        }

        // Matches Java: getId() and setId()
        public string Id
        {
            get => id;
            set => id = value;
        }

        // Matches Java: getPasswd()
        public string Passwd
        {
            get => passwd;
            set => passwd = value;
        }

        // Java: public String toString()
        public override string ToString()
        {
            // Java: "UserDTO["+id+' '+passwd+"]"
            return $"UserDTO[{id} {passwd}]";
        }
    }
}