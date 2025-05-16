using SASD.Models;
using System.Collections.Generic;

namespace SASD.ViewModels
{
    public class SportEventDetailViewModel
    {
        public SportEvent? CurrentSportEvent { get; set; }
        public int? PreviousEventId { get; set; }
        public int? NextEventId { get; set; }
        public bool HasEvents { get; set; } // To know if there are any events at all
    }
}