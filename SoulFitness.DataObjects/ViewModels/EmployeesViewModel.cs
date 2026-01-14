using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects.ViewModels
{
    public class EmployeesViewModle
    {
        public Employee Employees { get; set; }
        public List<Schedules> schedule { get; set; }
    }

   
}
