namespace SASD.Models
{
    public class SportEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxNoOfPlayers { get; set; }
        public string Location { get; set; } = string.Empty;
        public int SportId { get; set; }
        public Sport? Sport { get; set; }
        public List<PlayerRecord>? PlayerRecords { get; set; } 
    }
}
