using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareCRM.Models
{
    [Table("Appointment")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        public int PatientId { get; set; }

        public DateTime ScheduledAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
