using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SoulFitness.DataObjects.UserManagment
{
    public enum UserRole
    {
        [Description("Admin")]
        Admin,

        [Description("Standard User")]
        User,
    }
}
