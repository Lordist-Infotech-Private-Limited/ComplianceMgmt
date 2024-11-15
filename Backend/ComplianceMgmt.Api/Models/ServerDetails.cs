namespace ComplianceMgmt.Api.Models
{
    public class ServerDetail
    {
        public int Id { get; set; }
        public string ServerIp { get; set; }
        public string ServerName { get; set; }
        public string ServerPassword { get; set; }
        public string ServerPort { get; set; }
        public string DbName { get; set; }
        public string DatabaseType { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}
