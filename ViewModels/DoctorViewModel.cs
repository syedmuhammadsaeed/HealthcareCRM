using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.ViewModels
{
    public class DoctorViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doctor Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required.")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
