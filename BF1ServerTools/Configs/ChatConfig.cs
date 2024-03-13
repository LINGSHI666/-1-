namespace BF1ServerTools.Configs;

public class ChatConfig
{
    public int KeyPressDelay { get; set; } = 50;
    public int AutoSendMsgInterval { get; set; } = 1;
    public List<ChatContent> ChatContents { get; set; }
    public class ChatContent
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
