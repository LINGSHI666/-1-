namespace BF1ServerTools.API.Resp;

public class RespAuth
{
    public bool IsSuccess { get; set; }
    public string Remid { get; set; }
    public string Sid { get; set; }
    public string Code { get; set; }
    public string Content { get; set; }
    public double ExecTime { get; set; }
}
