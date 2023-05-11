using System.Text.Json.Serialization;

namespace Vortex.Messages;

abstract class BaseEnvelope
{
    [JsonPropertyName("src")]
    public string Source { get; set; }

    [JsonPropertyName("dest" )]
    public string Destination { get; set; }
}

class RequestEnvelope : BaseEnvelope
{
    [JsonPropertyName("body")]
    public Request Body { get; set; }
}

class Request
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("msg_id")]
    public int Id { get; set; }
}

class ResponseEnvelope : BaseEnvelope
{
    [JsonPropertyName("body")]
    public Response Body { get; set; }
}

abstract class Response
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("msg_id")]
    public int Id { get; set; }

    [JsonPropertyName("in_reply_to")]
    public int InReplyTo { get; set; }
}

class InitCommand : Request
{
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("node_ids")]
    public string[] NodeIds { get; set; }
}

class InitResponse : Response
{
    [JsonPropertyName("type")]
    public override string Type => "init_ok";
}

class EchoCommand : Request
{
    [JsonPropertyName("echo")]
    public string Echo { get; set; }
}

class EchoResponse : Response
{
    [JsonPropertyName("type")]
    public override string Type => "echo_ok";

    [JsonPropertyName("echo")]
    public string Echo { get; set; }
}

class GenerateCommand : Request
{
}

class GenerateResponse : Response
{
    [JsonPropertyName("type")]
    public override string Type => "generate_ok";

    [JsonPropertyName("id")]
    public string GeneratedId { get; set; }
}

class TopologyCommand : Request
{
    [JsonPropertyName("topology")]
    public Dictionary<string, string[]> Topology { get; set; }
}

class TopologyResponse : Response
{
    [JsonPropertyName("type")]
    public override string Type => "topology_ok";
}

class BroadcastCommand : Request
{
    [JsonPropertyName("message")]
    public object Message { get; set; }
}

class BroadcastResponse : Response
{
    [JsonPropertyName("type")]
    public override string Type => "broadcast_ok";
}

class ReadCommand : Request
{
}

class ReadResponse : Response
{
    [JsonPropertyName("type")]
    public override string Type => "read_ok";

    [JsonPropertyName("messages")]
    public object[] Messages { get; set; }
}
