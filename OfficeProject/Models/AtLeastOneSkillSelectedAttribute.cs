using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OfficeProject.Models
{
    public class AtLeastOneSkillSelectedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var contact = (Contact)validationContext.ObjectInstance;

            // Check if Skills property is empty or null
            if (string.IsNullOrEmpty(contact.Skills))
            {
                return new ValidationResult("Please select at least one skill.");
            }

            // Split skills string into a list
            var selectedSkills = contact.Skills.Split(',').ToList();

            // Check if at least one skill is selected
            if (selectedSkills.Count == 0)
            {
                return new ValidationResult("Please select at least one skill.");
            }

            return ValidationResult.Success;
        }
    }
}
