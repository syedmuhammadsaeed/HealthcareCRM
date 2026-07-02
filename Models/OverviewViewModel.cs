using System.Collections.Generic;

namespace HealthcareCRM.Models
{
    /// <summary>
    /// View model for the Overview dashboard page containing dynamic patient statistics.
    /// </summary>
    public class OverviewViewModel
    {
        public long TotalPatients { get; set; }
        public long ActivePatients { get; set; }
        public long FollowUpsDue { get; set; }
        public long CriticalCases { get; set; }
        
        /// <summary>
        /// A small list of recent patients to display in the overview table.
        /// </summary>
        public List<Patient> RecentPatients { get; set; } = new List<Patient>();
    }
}
