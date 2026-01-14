using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoulFitness.DataObjects.UserManagment.Identity
{
    /// <summary>
    /// Privilege base model
    /// </summary>
    public class ApplicationPrivilege
    {

        public string Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        
        public Status status { get; set; }
    }

    /// <summary>
    /// Role privilege base model
    /// </summary>
    public class ApplicationRolePrivilege
    {
        public string RoleId { get; set; }
        public string PrivilegeId { get; set; }
        public virtual ApplicationPrivilege Privilage { get; set; }
        public Status status { get; set; }
    }

    /// <summary>
    /// Role base model (inherits Identity Role model
    /// </summary>
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name, string _description, UserRole _userRole = UserRole.User)
            : base(name)
        {
            Description = _description;
            UserRole = _userRole;

        }
        public virtual UserRole UserRole { get; set; }
        public virtual string Description { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<ApplicationRolePrivilege> RolePrivileges { get; set; }
    }

    /// <summary>
    /// User role base model (inherits Identity User Role model)
    /// </summary>
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public ApplicationUserRole()
            : base()
        { }
        public virtual ApplicationRole Role { get; set; }
    }

    /// <summary>
    /// User base model (inherits Identity User model)
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            FirstLogin = true;
        }

        public bool FirstLogin { get; set; }
        public bool LockedOut { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName { get { return string.Format("{0} {1} {2}", FirstName, MiddleName, LastName); } }

        [NotMapped]
        public string FMName { get { return string.Format("{0} {1}", FirstName, MiddleName); } }
        public Status status { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        
    }
}