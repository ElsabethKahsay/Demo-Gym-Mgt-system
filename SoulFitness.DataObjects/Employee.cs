using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulFitness.DataObjects.UserManagment.Identity;

namespace SoulFitness.DataObjects
{
    // This class represents a model for the Employees table which contains members personal data 
    public class Employee: AuditTrail
            
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }

        [Required]
            public string EmployeeID { get; set; }     

        // Employee Name
        [Required]
        public string FirstName { get; set; }


        public string MiddleName { get; set; }
        public string LastName { get; set; }

            [NotMapped]
            public string FullName { get { return string.Format("{0} {1} {2}", FirstName, MiddleName, LastName); } }

            [NotMapped]
            public string FMName { get { return string.Format("{0} {1}", FirstName, MiddleName); } }

        [Required]
        public Gender Gender { get; set; }     
               
        public string CostCenter { get; set; }
                
        public string Position { get; set; }

        [Required]
        public Status Status { get; set; }

        public WeekDays Days { get; set; }

        public string RFID { get; set; }    

        public virtual Schedules Schedule { get; set; }  

        [ForeignKey("Schedule")]
        public int ScheduleID { get; set; }
     //   public int LockerNumber { get; set; }
       // public AccountStatus AccountStatus { get; set; }
       
    }
}


