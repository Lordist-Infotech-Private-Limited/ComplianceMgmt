namespace ComplianceMgmt.Api.Models
{
    public class ServerDetails
    {
        public int Nid { get; set; }
        public string SserverIp { get; set; }
        public string SserverName { get; set; }
        public string SserverPassword { get; set; }
        public string SserverPort { get; set; }
        public string SdbName { get; set; }
        public string SdatabaseType { get; set; }
        public DateTime NlastupdatedTime { get; set; }

    }
}
