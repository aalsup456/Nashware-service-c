using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TNG.NashWare.Services.Models;
using TNG.NashWare.Services.Models.Services;

namespace TNG.NashWare.Services.Handler
{
    /// <summary>
    /// Handler for Timesheet Service
    /// </summary>
    public class TimeSheetHandler
    {
        private DBHandler dbController;
        private QuickBookService qbService;
        public TimeSheetHandler()
        {
            dbController = new DBHandler();
            qbService = new QuickBookService();
        }

        /// <summary>
        /// Get Filter Data for View
        /// </summary>
        /// <returns></returns>
        public TimesheetFilterModel GetTimesheetFilter()
        {
            var toReturn = new TimesheetFilterModel();

            toReturn.ClientList = GetActiveClientList();
            toReturn.ProjectList = GetActiveProjectList();
            toReturn.WorkClassList = GetActiveWorkClassList();
            toReturn.ServiceTypeList = GetActiveServiceTypeList();

            return toReturn;
        }

        /// <summary>
        /// Get All Active Client
        /// </summary>
        /// <returns></returns>
        public List<ClientViewModel> GetActiveClientList()
        {
            var toReturn = new List<ClientViewModel>();

            dbController.ConnectionCheck();
            var query = @"SELECT * FROM CL_CLIENTS";
            using (SqlCommand com = new SqlCommand(query, dbController.connection))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempClient = new ClientViewModel();
                        tempClient.Name = reader["CL_NAME"].ToString();
                        //tempClient.Id = new Guid(reader["CL_ID"].ToString().ToUpper());
                        tempClient.Id = reader["CL_ID"].ToString().ToUpper();
                        if (DBNull.Value != reader["CL_QB_ID"])
                            tempClient.QBId = Convert.ToInt64(reader["CL_QB_ID"].ToString());//Convert.ToInt64(reader["CL_QB_ID"]);
                        tempClient.isActive = reader["CL_IS_ACTIVE"].ToString();

                        toReturn.Add(tempClient);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get All Active Project
        /// </summary>
        /// <returns></returns>
        public List<ProjectViewModel> GetActiveProjectList()
        {
            var toReturn = new List<ProjectViewModel>();

            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = @"SELECT * FROM PRJ_PROJECTS";
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tempProject = new ProjectViewModel();
                            tempProject.Name = reader["PRJ_NAME"].ToString();
                            //tempProject.Id = new Guid(reader["PRJ_ID"].ToString().ToUpper());
                            tempProject.Id = reader["PRJ_ID"].ToString().ToUpper();
                            tempProject.isActive = reader["PRJ_IS_ACTIVE"].ToString();//(int)reader["PRJ_IS_ACTIVE"];
                            tempProject.Description = reader["PRJ_DESCRIPTION"].ToString();
                            //tempProject.ClientID = new Guid(reader["PRJ_CL_ID"].ToString()); //Convert.ToInt64(reader["PRJ_CL_ID"]);
                            tempProject.ClientID = reader["PRJ_CL_ID"].ToString(); //Convert.ToInt64(reader["PRJ_CL_ID"]);
                            if (DBNull.Value != reader["PRJ_END_DATE"])
                                tempProject.EndDate = Convert.ToDateTime(reader["PRJ_END_DATE"]);

                            toReturn.Add(tempProject);
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get All Active Work Class
        /// </summary>
        /// <returns></returns>
        public List<WorkClassViewModel> GetActiveWorkClassList()
        {
            var toReturn = new List<WorkClassViewModel>();

            dbController.ConnectionCheck();
            var query = @"SELECT * FROM WC_WORK_CLASS";
            using (SqlCommand com = new SqlCommand(query, dbController.connection))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempWC = new WorkClassViewModel();
                        tempWC.Name = reader["WC_NAME"].ToString();
                        //tempWC.Id = new Guid(reader["WC_ID"].ToString());
                        tempWC.Id = reader["WC_ID"].ToString();
                        if (DBNull.Value != reader["WC_QB_ID"])
                            tempWC.QBId = Convert.ToInt64(reader["WC_QB_ID"]);
                        tempWC.isActive = reader["WC_IS_ACTIVE"].ToString();//(int)reader["WC_IS_ACTIVE"];

                        toReturn.Add(tempWC);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get All Active Services
        /// </summary>
        /// <returns></returns>
        public List<ServiceTypeViewModel> GetActiveServiceTypeList()
        {
            var toReturn = new List<ServiceTypeViewModel>();
            
            dbController.ConnectionCheck();
            var query = @"SELECT * FROM SRV_SERVICES";
            var parentList = new Dictionary<long, List<Guid>>();
            using (SqlCommand com = new SqlCommand(query, dbController.connection))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempService = new ServiceTypeViewModel();
                        tempService.Name = reader["SRV_NAME"].ToString();
                        //tempService.Id = new Guid(reader["SRV_ID"].ToString().ToUpper());
                        tempService.Id = reader["SRV_ID"].ToString().ToUpper();
                        if (DBNull.Value!= reader["SRV_QB_ID"])
                            tempService.QBId = Convert.ToInt64(reader["SRV_QB_ID"]);
                        tempService.isActive = reader["SRV_IS_ACTIVE"].ToString();//(int)reader["SRV_IS_ACTIVE"];
                        if (DBNull.Value != reader["SRV_PARENTREF_QB_ID"])
                        {
                            tempService.HierachyType = 1;
                            var parentQBID = Convert.ToInt64(reader["SRV_PARENTREF_QB_ID"]);
                            tempService.ParentQBID = parentQBID;

                            if (!parentList.Keys.Contains(parentQBID))
                            {
                                parentList.Add(parentQBID, new List<Guid>());
                                parentList[parentQBID].Add(new Guid(tempService.Id));
                            } else
                            {
                                parentList[parentQBID].Add(new Guid(tempService.Id));
                            }
                        }

                        toReturn.Add(tempService);
                    }
                }
            }

            foreach(var parent in toReturn.Where(p=>parentList.Keys.Contains(p.QBId)))
            {
                parent.HierachyType = 2;
                foreach(var child in parentList[parent.QBId])
                {
                    var childObj = toReturn.Single(p => new Guid(p.Id) == child);
                    childObj.ParentID = parent.Id;
                }
            }

            return toReturn;
        }
    }
}