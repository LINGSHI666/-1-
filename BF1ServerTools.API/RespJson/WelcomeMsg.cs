namespace BF1ServerTools.API.RespJson;

public class WelcomeMsg
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public Result result { get; set; }
    public class Result
    {
        public string firstMessage { get; set; }
        public string secondMessage { get; set; }
    }
}
