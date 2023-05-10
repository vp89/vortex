using System.Text.Json;
using System.Text.Json.Serialization;

while (true) {
    var message = Console.ReadLine();
    if (string.IsNullOrEmpty(message)) continue;

    Console.Error.WriteLine($"Message received: {message}");
    var _ = HandleMessageAsync(message);
}

async Task HandleMessageAsync(string message)
{
    var options = new JsonSerializerOptions();
    options.Converters.Add(new MaelstromPayloadConverter());
    var foo = JsonSerializer.Deserialize<MaelstromEnvelope>(message, options);
    
    if (foo is null)
        throw new Exception("NOOOOOO!");
    else if (foo.Body is InitMessage initMessage)
        await HandleInitMessageAsync(foo, initMessage, options);
    else if (foo.Body is EchoMessage echoMessage)
        await HandleEchoMessageAsync(foo, echoMessage, options);
    else
        Console.Error.WriteLine("HERE11111");
}

async Task HandleInitMessageAsync(MaelstromEnvelope envelope, InitMessage message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"INIT MESSAGE RECEIVED NodeId {message.NodeId} NodeIds {string.Join(",", message.NodeIds)}!");

    var reply = new MaelstromEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new InitReply()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            InReplyTo = message.Id
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING INIT REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

async Task HandleEchoMessageAsync(MaelstromEnvelope envelope, EchoMessage message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"ECHO MESSAGE RECEIVED {message.Echo}");

    var reply = new MaelstromEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new EchoReply()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            Echo = message.Echo,
            InReplyTo = message.Id
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING ECHO REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

class MaelstromPayloadConverter : JsonConverter<MaelstromPayload>
{
    public override MaelstromPayload? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var clonedReader = reader;

        var payload = JsonSerializer.Deserialize<MaelstromPayload>(ref clonedReader);
        
        try
        {
            MaelstromPayload? deserialized = payload.Type switch
            {
                "init" => JsonSerializer.Deserialize<InitMessage>(ref reader),
                "echo" => JsonSerializer.Deserialize<EchoMessage>(ref reader),
                _ => throw new JsonException()
            };

            return deserialized;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("herwekrw235");
            throw;
        }
    }

    public override void Write(Utf8JsonWriter writer, MaelstromPayload value, JsonSerializerOptions options)
    {
        if (value is InitReply initReply)
            JsonSerializer.Serialize<InitReply>(writer, initReply);
        else if (value is EchoReply echoReply)
            JsonSerializer.Serialize<EchoReply>(writer, echoReply);
        else
            throw new Exception("UNEXPECTED RESPONSE TYPE!");
    }
}

class MaelstromEnvelope
{
    [JsonPropertyName("src")]
    public string Source { get; set; }

    [JsonPropertyName("dest" )]
    public string Destination { get; set; }

    [JsonPropertyName("body")]
    public MaelstromPayload Body { get; set; }
}

class MaelstromPayload
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("msg_id")]
    public int Id { get; set; }
}

class InitMessage : MaelstromPayload
{
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("node_ids")]
    public string[] NodeIds { get; set; }
}

class InitReply : MaelstromPayload
{
    [JsonPropertyName("type")]
    public string Type => "init_ok";
    
    [JsonPropertyName("in_reply_to")]
    public int InReplyTo { get; set; }
}

class EchoMessage : MaelstromPayload
{
    [JsonPropertyName("echo")]
    public string Echo { get; set; }
}

class EchoReply : MaelstromPayload
{
    [JsonPropertyName("type")]
    public string Type => "echo_ok";
    
    [JsonPropertyName("in_reply_to")]
    public int InReplyTo { get; set; }

    [JsonPropertyName("echo")]
    public string Echo { get; set; }
}

static class Globals
{
    public static int MESSAGE_ID = 0;
}
