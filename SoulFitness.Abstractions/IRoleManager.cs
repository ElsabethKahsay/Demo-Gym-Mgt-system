using System.Threading.Tasks;

namespace SoulFitness.Abstractions
{
    public interface IRoleManager
    {
        Task<bool> RoleExists(string roleName);
        Task<bool> CreateRole(string roleName, string description = "");
    }
}
