using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoulFitness.DataObjects.UserManagment.Identity;
using System;

namespace SoulFitness.DataObjects.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException("ModelBuilder is NULL");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            modelBuilder.Entity<ApplicationPrivilege>().ToTable("Privileges").HasKey(p => p.Id);
            modelBuilder.Entity<ApplicationRolePrivilege>().ToTable("RolePrivileges").HasKey(p => new { p.RoleId, p.PrivilegeId });
        }

        //Account management
        public DbSet<ApplicationPrivilege> ApplicationPrivileges { get; set; }
        public DbSet<ApplicationRolePrivilege> ApplicationRolePrivileges { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Schedules> Schedule { get; set; }

        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Coaches> Coaches { get; set; }
        public DbSet<CoachesAttendance> CoachesAttendance { get; set; }
        public DbSet<CoachesSchedule> CoachesSchedule { get; set; }

        public DbSet<GymAttendances> GymAttendance { get; set; }
        public DbSet<PortalManagement> PortalManagement { get; set; } 
    public DbSet<MisconductLog> MisconductLog { get; set;}
        public DbSet<Locker> Locker { get; set; }
 

    }
}
