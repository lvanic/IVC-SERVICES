namespace IVC_DDNS.models
{
    public class Node
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public DateTime LastPing { get; set; }
    }

}
