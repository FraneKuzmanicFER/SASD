namespace SASD.Models
{
    public class PlayerRecord
    {
        public int Id { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string PlayerSurname { get; set; } = string.Empty;
        public int NoOfPoints { get; set; }
        public bool Arrived { get; set; }
        public string Description { get; set; } = string.Empty;
        public int SportEventId { get; set; }
        public SportEvent? SportEvent { get; set; }
    }
}
