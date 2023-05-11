using System.Text.Json;
using System.Text.Json.Serialization;
using Vortex.Messages;

while (true) {
    var message = Console.ReadLine();
    if (string.IsNullOrEmpty(message)) continue;

    Console.Error.WriteLine($"Message received: {message}");
    var _ = HandleMessageAsync(message);
}

async Task HandleMessageAsync(string message)
{
    var deserializerOptions = new JsonSerializerOptions();
    deserializerOptions.Converters.Add(new RequestConverter());
    var request = JsonSerializer.Deserialize<RequestEnvelope>(message, deserializerOptions);

    var serializerOptions = new JsonSerializerOptions();
    serializerOptions.Converters.Add(new ResponseConverter());
    
    if (request is null)
        throw new Exception("NOOOOOO!");
    else if (request.Body is InitCommand initCommand)
        await HandleInitMessageAsync(request, initCommand, serializerOptions);
    else if (request.Body is EchoCommand echoCommand)
        await HandleEchoMessageAsync(request, echoCommand, serializerOptions);
    else if (request.Body is GenerateCommand generateCommand)
        await HandleGenerateCommandAsync(request, generateCommand, serializerOptions);
    else if (request.Body is TopologyCommand topologyCommand)
        await HandleTopologyCommandAsync(request, topologyCommand, serializerOptions);
    else if (request.Body is BroadcastCommand broadcastCommand)
        await HandleBroadcastCommandAsync(request, broadcastCommand, serializerOptions);
    else if (request.Body is ReadCommand readCommand)
        await HandleReadCommandAsync(request, readCommand, serializerOptions);
    else
        Console.Error.WriteLine("HERE11111");
}

async Task HandleInitMessageAsync(RequestEnvelope envelope, InitCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"INIT MESSAGE RECEIVED NodeId {message.NodeId} NodeIds {string.Join(",", message.NodeIds)}!");

    Globals.NODE_ID = message.NodeId;

    var reply = new ResponseEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new InitResponse()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            InReplyTo = message.Id
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING INIT REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

async Task HandleEchoMessageAsync(RequestEnvelope envelope, EchoCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"ECHO MESSAGE RECEIVED {message.Echo}");

    var reply = new ResponseEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new EchoResponse()
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

async Task HandleGenerateCommandAsync(RequestEnvelope envelope, GenerateCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"GENERATE MESSAGE RECEIVED");

    var reply = new ResponseEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new GenerateResponse()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            GeneratedId = Guid.NewGuid().ToString(),
            InReplyTo = message.Id
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING GENERATE REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

async Task HandleTopologyCommandAsync(RequestEnvelope envelope, TopologyCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"TOPOLOGY MESSAGE RECEIVED");

    if (message.Topology.ContainsKey(Globals.NODE_ID))
    {
        Globals.NEIGHBOR_NODES = message.Topology[Globals.NODE_ID];
        Console.Error.WriteLine($"My neighbors are {string.Join(",", Globals.NEIGHBOR_NODES)}");
    }

    var reply = new ResponseEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new TopologyResponse()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            InReplyTo = message.Id
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING TOPOLOGY REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

async Task HandleBroadcastCommandAsync(RequestEnvelope envelope, BroadcastCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"BROADCAST MESSAGE RECEIVED");

    Globals.MESSAGES.Add(message.Message);

    var reply = new ResponseEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new BroadcastResponse()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            InReplyTo = message.Id
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING BROADCAST REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

async Task HandleReadCommandAsync(RequestEnvelope envelope, ReadCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"READ MESSAGE RECEIVED");

    var reply = new ResponseEnvelope()
    {
        Source = envelope.Destination,
        Destination = envelope.Source,
        Body = new ReadResponse()
        {
            Id = Interlocked.Increment(ref Globals.MESSAGE_ID),
            InReplyTo = message.Id,
            Messages = Globals.MESSAGES.ToArray() // TODO?
        }
    };

    var serialized = JsonSerializer.Serialize(reply, options);

    Console.Error.WriteLine($"SENDING READ REPLY {serialized}");
    Console.Out.WriteLine(serialized);
}

class RequestConverter : JsonConverter<Request>
{
    public override Request? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var clonedReader = reader;

        var payload = JsonSerializer.Deserialize<Request>(ref clonedReader);
        
        try
        {
            Request? deserialized = payload.Type switch
            {
                "init" => JsonSerializer.Deserialize<InitCommand>(ref reader),
                "echo" => JsonSerializer.Deserialize<EchoCommand>(ref reader),
                "generate" => JsonSerializer.Deserialize<GenerateCommand>(ref reader),
                "topology" => JsonSerializer.Deserialize<TopologyCommand>(ref reader),
                "broadcast" => JsonSerializer.Deserialize<BroadcastCommand>(ref reader),
                "read" => JsonSerializer.Deserialize<ReadCommand>(ref reader),
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

    public override void Write(Utf8JsonWriter writer, Request value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

class ResponseConverter : JsonConverter<Response>
{
    public override Response? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Response value, JsonSerializerOptions options)
    {
        if (value is InitResponse initReply)
            JsonSerializer.Serialize<InitResponse>(writer, initReply);
        else if (value is EchoResponse echoReply)
            JsonSerializer.Serialize<EchoResponse>(writer, echoReply);
        else if (value is GenerateResponse generateResponse)
            JsonSerializer.Serialize<GenerateResponse>(writer, generateResponse);
        else if (value is TopologyResponse topologyResponse)
            JsonSerializer.Serialize<TopologyResponse>(writer, topologyResponse);
        else
            throw new Exception("UNEXPECTED RESPONSE TYPE!");
    }
}

static class Globals
{
    public static int MESSAGE_ID = 0;
    public static string NODE_ID = null;
    public static string[] NEIGHBOR_NODES = null;
    public static List<object> MESSAGES = new List<object>();
}
