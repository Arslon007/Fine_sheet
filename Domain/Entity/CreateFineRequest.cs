using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class CreateFineRequest
    {
        public int EmployeeId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
