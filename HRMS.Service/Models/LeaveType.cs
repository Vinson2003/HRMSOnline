using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Models
{
    public class LeaveTypeList
    {
        public int Id { get; set; }

        public string TypeName { get; set; } = null!;

        public bool RequiresAttachment { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
