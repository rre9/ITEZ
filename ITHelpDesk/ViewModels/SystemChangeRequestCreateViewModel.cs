using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.ViewModels
{
    public class SystemChangeRequestCreateViewModel
    {
        // Applicant
        [Display(Name = "Requester Name")]
        public string RequesterName { get; set; } = string.Empty;

        [Display(Name = "Change Request Number")]
        public string ChangeRequestNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Change Description")]
        public string ChangeDescription { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Change Reason")]
        public string ChangeReason { get; set; } = string.Empty;

        [Display(Name = "Requester Approved")]
        public bool IsRequesterApproved { get; set; }

        // Impact assessment
        [Display(Name = "Change Type")]
        public int ChangeType { get; set; }

        [Display(Name = "Change Priority")]
        public int ChangePriority { get; set; }

        [Display(Name = "Change Impact")]
        public int ChangeImpact { get; set; }

        [Display(Name = "Affected Assets")]
        public string? AffectedAssets { get; set; }

        // Execution plans
        [Display(Name = "Implementation Plan")]
        public string? ImplementationPlan { get; set; }

        [Display(Name = "Backout Plan")]
        public string? BackoutPlan { get; set; }

        // Implementer
        [Display(Name = "Implementer Name")]
        public string? ImplementerName { get; set; }

        [Display(Name = "Execution Date")]
        [DataType(DataType.Date)]
        public DateTime? ExecutionDate { get; set; }

        // For rendering (not used anymore)
        public string? SelectedManagerId { get; set; }
        public List<string> Departments { get; set; } = new List<string>();
        public List<UserLookupViewModel>? Managers { get; set; }
    }
}
