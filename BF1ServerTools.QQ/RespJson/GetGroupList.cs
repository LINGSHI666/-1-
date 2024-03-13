namespace BF1ServerTools.QQ.RespJson;

public class GetGroupList
{
    public List<DataItem> data { get; set; }
    public int retcode { get; set; }
    public string status { get; set; }
    public class DataItem
    {
        public int group_create_time { get; set; }
        public int group_id { get; set; }
        public int group_level { get; set; }
        public string group_name { get; set; }
        public int max_member_count { get; set; }
        public int member_count { get; set; }
    }
}
