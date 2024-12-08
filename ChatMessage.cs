using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace checkers
{
    public class ChatMessage
    {
        [JsonPropertyName("who")] //white или black
        public string who { get; set; }

        [JsonPropertyName("text")] //само сообщение
        public string text { get; set; }
    }
}
