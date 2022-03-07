using MyTestWebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MyTestWebApp.Controllers
{
    public class EmployeeController : Controller
    {
        private MyTestDBEntities db = new MyTestDBEntities();
        public ActionResult Index()
        {
            var employees = db.Employees.ToList();
            List<EmployeeViewModel> modelList = new List<EmployeeViewModel>();
            foreach (var item in employees)
            {
                EmployeeViewModel modelObj = new EmployeeViewModel();
                modelObj.EmployeeId = item.EmployeeId;
                modelObj.EmployeeName = item.EmployeeName;
                modelObj.Gender = item.Gender;
                modelObj.ProfileImage = item.ProfileImage;
                modelObj.MobileNumber = item.MobileNumber;
                var employeeDepartments = db.EmployeeDepartments.Where(x => x.EmployeeId == item.EmployeeId).Select(y => y.Department).ToList();
                if(employeeDepartments.Count > 0)
                {
                    modelObj.DepartmentNameList = employeeDepartments.Select(x => x.DepartmentName).ToList();
                }
                modelList.Add(modelObj);
            }
            return View(modelList);
        }
        public ActionResult Create(int? id)
        {
            EmployeeViewModel model = new EmployeeViewModel();
            var departmentData = db.Departments.ToList();
            List<SelectListItem> departmentSelectList = new List<SelectListItem>();
            for (int i = 0; i < departmentData.Count; i++)
            {
                SelectListItem items = new SelectListItem();
                items.Value = departmentData[i].DepartmentId.ToString();
                items.Text = departmentData[i].DepartmentName;
                items.Selected = false;
                departmentSelectList.Add(items);
            }
            model.Departments = departmentSelectList;
            if (id != null)
            {
                Employee employee = db.Employees.Find(id);
                List<int> empDeptList = db.EmployeeDepartments.Where(x => x.EmployeeId == id).Select(y => y.DepartmentId).ToList();
                model.EmployeeId = employee.EmployeeId;
                model.EmployeeName = employee.EmployeeName;
                model.MobileNumber = employee.MobileNumber;
                model.Gender = employee.Gender;
                model.ProfileImage = employee.ProfileImage;
                model.DepartmentIds = empDeptList.ToArray();
            }
            return View(model);
        }

        [HttpPost]
        public string CreateOrEdit(HttpPostedFileBase[] file)
        {
            var valid = "";
            Employee empObj = new Employee();
            var fileName = "";
            var employeeName = Request.Params["employeeName"];
            var mobileNumber = Request.Params["mobileNumber"];
            var gender = Request.Params["gender"];
            var departmentIds = Request.Params["departmentIds"];
            var departmentIdArrays = departmentIds.Split(',');
            var employeeId = Request.Params["employeeId"];
            var oldFilePath = Request.Params["oldFilePath"];
            HttpPostedFileBase RetrievedFile;
            empObj.EmployeeName = employeeName;
            empObj.MobileNumber = mobileNumber;
            empObj.Gender = gender;
            empObj.ProfileImage = "";
            if (String.IsNullOrEmpty(employeeId) || employeeId == "0")
            {
                empObj.EmployeeId = 0;
                db.Employees.Add(empObj);
                db.SaveChanges();
                int id = empObj.EmployeeId;
                if (!string.IsNullOrEmpty(departmentIds))
                {
                    foreach (var item in departmentIdArrays)
                    {
                        EmployeeDepartment empDept = new EmployeeDepartment();
                        empDept.DepartmentId = Convert.ToInt32(item);
                        empDept.EmployeeId = id;
                        db.EmployeeDepartments.Add(empDept);
                        db.SaveChanges();
                    }
                }
                valid = "Success";
            }
            else
            {
                empObj.EmployeeId = Convert.ToInt32(employeeId);
                db.Entry(empObj).State = EntityState.Modified;
                db.SaveChanges();
                if (!string.IsNullOrEmpty(departmentIds))
                {
                    List<EmployeeDepartment> employeeDepartmentList = db.EmployeeDepartments.ToList();
                    var listDelete = employeeDepartmentList.Where(x => x.EmployeeId == Convert.ToInt32(employeeId)).ToList();
                    db.EmployeeDepartments.RemoveRange(listDelete);
                    db.SaveChanges();
                    foreach (var item in departmentIdArrays)
                    {
                        EmployeeDepartment empDept = new EmployeeDepartment();
                        empDept.DepartmentId = Convert.ToInt32(item);
                        empDept.EmployeeId = Convert.ToInt32(employeeId);
                        db.EmployeeDepartments.Add(empDept);
                        db.SaveChanges();
                    }
                }
                valid = "Success";
            }
            if (file != null)
            {
                RetrievedFile = file[0];
                fileName = RetrievedFile.FileName.ToString();
                var directoryPath = Server.MapPath("~/Images/ProfileImages/" + empObj.EmployeeId);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var dbPath = "/Images/ProfileImages/" + empObj.EmployeeId + "/" + fileName;
                var filePath = directoryPath +"/"+ fileName;
                empObj.ProfileImage = dbPath;
                RetrievedFile.SaveAs(filePath);
            }
            else
            {
                empObj.ProfileImage = oldFilePath;
            }
            db.Entry(empObj).State = EntityState.Modified;
            db.SaveChanges();
            return valid;
        }
        public string DeleteConfirmed(int id)
        {
            var valid = "";
            try
            {
                List<EmployeeDepartment> employeeDepartmentList = db.EmployeeDepartments.ToList();
                var listDelete = employeeDepartmentList.Where(x => x.EmployeeId == id).ToList();
                db.EmployeeDepartments.RemoveRange(listDelete);
                db.SaveChanges();
                Employee employee = db.Employees.Find(id);
                db.Employees.Remove(employee);
                db.SaveChanges();
                if (!string.IsNullOrEmpty(employee.ProfileImage))
                {
                    var folderPath = Path.GetDirectoryName(employee.ProfileImage);
                    var currentFolderPath = Server.MapPath(folderPath + "/");
                    System.IO.DirectoryInfo folderInfo = new DirectoryInfo(currentFolderPath);

                    foreach (FileInfo file in folderInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    if (Directory.Exists(currentFolderPath))
                    {
                        Directory.Delete(currentFolderPath);
                    }
                }
                valid = "Success";
            }
            catch (Exception ex)
            {
                valid = ex.Message;
                throw;
            }
            return valid;
        }
    }
}