using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.ViewModels
{
    /// <summary>
    /// View model for creating and updating patient records.
    /// </summary>
    public class PatientViewModel
    {
        /// <summary>Gets or sets the patient's MongoDB ObjectId (empty for new records).</summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's full name.</summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's age.</summary>
        [Required(ErrorMessage = "Age is required.")]
        [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
        public int Age { get; set; }

        /// <summary>Gets or sets the patient's gender.</summary>
        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's phone number.</summary>
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's address.</summary>
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; } = string.Empty;
    }
}
