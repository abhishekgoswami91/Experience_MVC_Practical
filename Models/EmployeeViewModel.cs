using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyTestWebApp.Models
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Gender { get; set; }
        public string MobileNumber { get; set; }
        public string ProfileImage { get; set; }
        public int[] DepartmentIds { get; set; }
        public List<string> DepartmentNameList { get; set; }
        public List<SelectListItem> DepartmentSelectList { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }
    }

}