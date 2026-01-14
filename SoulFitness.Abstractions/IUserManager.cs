using System.Threading.Tasks;

namespace SoulFitness.Abstractions
{
    public interface IUserManager
    {
        Task<bool> AddUserToRole(string userId, string roleName);
        Task ClearUserRoles(string userId);
        Task RemoveFromRole(string userId, string roleName);
        Task DeleteRole(string roleId);
    }
}
