using System.Text.Json;
using System.Text.Json.Serialization;

while (true) {
    var message = Console.ReadLine();
    Console.Error.WriteLine($"Message received: {message}");
    HandleMessageAsync(message);
}

async void HandleMessageAsync(string message) {
    try {
        var deserializedMessage = JsonSerializer.Deserialize<MaelstromMessage>(message);

        // build reply code
        if (deserializedMessage?.Body is null) return;

        var clonedBody = new Dictionary<string, object>(deserializedMessage.Body);

        var messageType = clonedBody["type"];
        var messageId = clonedBody["msg_id"];

        Console.Error.WriteLine(
            $"Received message {messageId} of type {messageType} from {deserializedMessage.Source}");

        var replyMessageId = Interlocked.Increment(ref Globals.MESSAGE_ID);
        var replyMessageType = messageType += "_ok";

        clonedBody["msg_id"] = replyMessageId;
        clonedBody["type"] = replyMessageType;
        clonedBody["in_reply_to"] = deserializedMessage.Body["msg_id"];

        var reply = new MaelstromMessage()
        {
            Source = deserializedMessage.Destination,
            Destination = deserializedMessage.Source,
            Body = clonedBody
        };

        var serializedReply = JsonSerializer.Serialize(reply);
        Console.Error.WriteLine($"Sending message {replyMessageId} of type {replyMessageType} as reply: {serializedReply}");
        Console.WriteLine(serializedReply);
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
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("body")]
    public Dictionary<string, object> Body { get; set; }
}


static class Globals
{
    public static int MESSAGE_ID = 0;
}
