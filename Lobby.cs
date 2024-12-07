using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace checkers
{
    public class Lobby
    {
        [JsonPropertyName("LobbyName")]
        public string LobbyName { get; set; }
        [JsonPropertyName("Connected")]
        public int Connected { get; set; }
        [JsonPropertyName("LobbyId")]
        public Guid LobbyId { get; set; }
    }
}
