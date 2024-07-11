using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OfficeProject.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter an email address.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a city.")]
        public string City { get; set; }

        [AtLeastOneSkillSelected(ErrorMessage = "Please select at least one skill.")]
        public string Skills { get; set; }

        public bool isDeleted { get; set; }  // New property
    }
}
