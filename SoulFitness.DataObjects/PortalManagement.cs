using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Http;

namespace SoulFitness.DataObjects
{
    public class PortalManagement : AuditTrail
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }



        [Required]
        public string Title { get; set; }
        public string Text { get; set; }
        public string Path { get; set; }
        
       // public ResourceType ResourceType {get; set;}

        [Required]
        public Types Type { get; set; }

        [Required]
        public Status Status { get; set; }
    
        public DateTime ExpirationDate { get; set; }
        [DataType(DataType.Upload)]
        public string ImgLocation { get; set; }

        [Required]
        public Priority Priority { get; set; }

        [Required]
        public PostStat PostStat { get; set; }  

    }

    public class PortalManagementCreateRequest : AuditTrail
    {

        public int Id { get; set; }

               public string Title { get; set; }
        public string Text { get; set; }
        public string Path { get; set; }

        // public ResourceType ResourceType {get; set;}

        public Types Type { get; set; }

        public Status Status { get; set; }

        public DateTime ExpirationDate { get; set; }
        public List<IFormFile> ImgLocation { get; set; }

        public Priority Priority { get; set; }

        public PostStat PostStat { get; set; }

    }
}
