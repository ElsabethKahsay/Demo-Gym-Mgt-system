using SoulFitness.DataObjects.UserManagment.Identity;
using System.Collections.Generic;

namespace SoulFitness.DataObjects.ViewModels
{
    public class UserCreateDto
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
