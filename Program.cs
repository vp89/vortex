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
    else
        Console.Error.WriteLine("HERE11111");
}

async Task HandleInitMessageAsync(RequestEnvelope envelope, InitCommand message, JsonSerializerOptions options)
{
    Console.Error.WriteLine($"INIT MESSAGE RECEIVED NodeId {message.NodeId} NodeIds {string.Join(",", message.NodeIds)}!");

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
        else
            throw new Exception("UNEXPECTED RESPONSE TYPE!");
    }
}

static class Globals
{
    public static int MESSAGE_ID = 0;
}
