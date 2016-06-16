using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TNG.NashWare.Services.Models
{
    public class SPTokenModel
    {
        public string nw_Id { get; set; }
        public string nw_LoginName { get; set; } 
        public string nw_DisplayName { get; set; }
        public string nw_Email { get; set; }        
        public string nw_SPOIDCRL { get; set; }
        /*public string nw_DIGEST { get; set; }*/
    }

    public class ClientModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long QB_Id { get; set; }
        public Guid ClassId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string COntactPhone { get; set; }
        public int isActive { get; set; }
    }

    public class ProjectModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? EndDate { get; set; }
        public int isActive { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class ServiceModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long QB_Id { get; set; }
        public string Description { get; set; }
        public int isBillable { get; set; }
        public Guid ParentId { get; set; }
        public int isActive { get; set; }
    }

    public class WorkClassModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long QB_Id { get; set; }
        public int isActive { get; set; }
    }
    
}