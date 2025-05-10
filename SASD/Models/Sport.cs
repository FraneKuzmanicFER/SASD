using SASD.Models.Enums;

namespace SASD.Models
{
    public class Sport
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int NoOfPlayers { get; set; }
        public int NoOfSubscriptions { get; set; }
        public SportType SportType { get; set; }

    }
}
