namespace backend.Models
{
    public class DeviceMetadata
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal? Value { get; set; }
    }
}