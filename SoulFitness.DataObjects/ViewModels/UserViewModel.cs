using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects.ViewModels
{
    public class UserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public List<ApplicationRole> UserRoles { get; set; }
    }
}
