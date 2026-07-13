using System;
using System.Collections.Generic;

namespace HealthcareCRM.ViewModels
{
    public class BillingViewModel
    {
        public decimal DailyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<BillingItemViewModel> MonthlyAppointments { get; set; } = new List<BillingItemViewModel>();
    }

    public class BillingItemViewModel
    {
        public string PatientName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
    }
}
