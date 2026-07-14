using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyaRFApp
{
    public class AppointmentInfo
    {
        public int AppointmentId { get; set; }
        public string ClientName { get; set; }
        public string TreatmentName { get; set; }
        public int DurationMinutes { get; set; } 
        public string Notes { get; set; }

        public int ClientId { get; set; }
        public int TreatmentId { get; set; }
    }
}
