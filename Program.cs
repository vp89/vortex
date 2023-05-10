using System.Text.Json;
using System.Text.Json.Serialization;

while (true) {
    var message = Console.ReadLine();
    Console.Error.WriteLine($"Read the following {message}");
    HandleMessageAsync(message);
}

async void HandleMessageAsync(string message) {
    try {
        var deserializedMessage = JsonSerializer.Deserialize<MaelstromMessage>(message);

        if (deserializedMessage?.Body is null) return;

        var source = deserializedMessage.Source;
        deserializedMessage.Source = deserializedMessage.Destination;
        deserializedMessage.Destination = source;
        deserializedMessage.Body["type"] += "_ok";
        deserializedMessage.Body["in_reply_to"] = deserializedMessage.Body["msg_id"];

        var serializedMessage = JsonSerializer.Serialize(deserializedMessage);
        Console.WriteLine(serializedMessage);
    } catch (Exception e) {
        Console.Error.WriteLine($"ERROR - {e.Message}");
    }
}

class MaelstromMessage
{
    [JsonPropertyName("src")]
    public string Source { get; set; }

    [JsonPropertyName("dest" )]
    public string Destination { get; set; }

    [JsonPropertyName("body")]
    public Dictionary<string, object> Body { get; set; }
}
