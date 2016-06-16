using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TNG.NashWare.Services.Models
{
    public class TimesheetFilterModel
    {
        public List<ClientViewModel> ClientList { get; set; }
        public List<ProjectViewModel> ProjectList { get; set; }
        public List<WorkClassViewModel> WorkClassList { get; set; }
        public List<ServiceTypeViewModel> ServiceTypeList { get; set; }
    }

    public class ProjectViewModel
    {
        public string Name { get; set; }
        //public Guid Id { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        //public Guid ClientID { get; set; }
        public string ClientID { get; set; }
        //public ClientModel Client { get; set; }
        public string isActive { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ClientViewModel
    {
        public string Name { get; set; }
        //public Guid Id { get; set; }
        public string Id { get; set; }
        public long QBId { get; set; }
        public string isActive { get; set; }
    }

    public class ServiceTypeViewModel
    {
        public string Name { get; set; }
        public string FullyQualifiedName { get; set; }
        public int HierachyType { get; set; } // 0 Normal, 1 Children, 2 Parent
        //public Guid Id { get; set; }
        public string Id { get; set; }
        public long QBId { get; set; }
        //public Guid ParentID { get; set; }
        public string ParentID { get; set; }
        public long ParentQBID { get; set; }
        public int isBillable { get; set; }
        public string isActive { get; set; }
    }

    public class WorkClassViewModel
    {
        public string Name { get; set; }
        //public Guid Id { get; set; }
        public string Id { get; set; }
        public long QBId { get; set; }
        public string isActive { get; set; }
    }
}