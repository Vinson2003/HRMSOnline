using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.LIB;
using HRMS.Service.Base;
using HRMS.Service.Interface;
using HRMS.Service.Models;
using System.Linq.Expressions;
using System.Net.NetworkInformation;


namespace HRMS.Service.Service
{
    public class DepartmentService(HRMSContext context) : IDepartmentService
    {
        private readonly HRMSContext _Context = context;

        public BaseResponse<List<DepartmentList>> GetAllDepartments(BasePaging paging, string searchName)
        {
            var response = new BaseResponse<List<DepartmentList>>();

            // CRUCIAL: IQueryable defers execution. The query is NOT sent to the SQL database yet.
            // It only builds the SQL command dynamically based on your conditions.
            var departments = _Context.MstDepartments.Where(d => d.IsDeleted == false &&
                (string.IsNullOrEmpty(searchName) || (d.DepartmentName != null && d.DepartmentName.Contains(searchName))))
                .Select(d => new DepartmentList()
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    Created = d.Created,
                    CreatedBy = d.CreatedBy
                });

            // CRUCIAL: Dictionary mapping a string to a LINQ Expression. 
            // This is a secure and efficient way to handle dynamic sorting based on user input.
            var columnMappings = new Dictionary<string, Expression<Func<DepartmentList, object?>>>
            {
                { "Id", i => i.Id },
                { "DepartmentName", i => i.DepartmentName },
                { "Created", i => i.Created },
                { "CreatedBy", i => i.CreatedBy }
            };

            if (paging.Column != null && columnMappings.ContainsKey(paging.Column))
            {
                var keySelector = columnMappings[paging.Column];
                departments = Const.PAGING_SORT_ASC == paging.SortBy ? departments.OrderBy(keySelector) : departments.OrderByDescending(keySelector);
            }

            // CRUCIAL: Methods like Count(), ToList(), and Take() trigger the actual execution to the database.
            // Skip() and Take() translate directly into SQL OFFSET and FETCH to handle pagination efficiently.
            response.Total = departments.Count();
            response.Result = [.. departments.Skip(paging.Start).Take(paging.Length)];

            return response;
        }

        public BaseResponse<bool> CreateDepartment(CreateDepartment request)
        {
            var response = new BaseResponse<bool>();

            bool isExists = _Context.MstDepartments.Any(d => d.DepartmentName == request.DepartmentName && d.IsDeleted == false);
            if (isExists)
            {
                response.Message = "Department Name already exists";
                response.Result = false;
                return response;
            }

            MstDepartment newDepartment = new()
            {
                DepartmentName = request.DepartmentName,
                Created = DateTime.UtcNow,
                CreatedBy = "System", 
            };

            _Context.MstDepartments.Add(newDepartment);
            _Context.SaveChanges();

            response.Result = true;
            response.Message = "Add Success";
            return response;
        }

        public BaseResponse<bool> UpdateDepartment(UpdateDepartment request)
        {
            var response = new BaseResponse<bool>();

            // CRUCIAL: Retrieve the existing entity from the database first to ensure it exists and is not deleted.
            var department = _Context.MstDepartments.FirstOrDefault(d => d.Id == request.Id && !d.IsDeleted);

            if (department == null)
            {
                response.Message = "Department not found.";
                response.Result = false;
                return response;
            }

            // CRUCIAL: Check for duplicate name, but exclude the current department's ID from the check.
            bool isDuplicate = _Context.MstDepartments.Any(d => d.DepartmentName == request.DepartmentName &&
                d.Id != request.Id && !d.IsDeleted);

            if (isDuplicate)
            {
                response.Message = "Department Name already exists.";
                response.Result = false;
                return response;
            }

            // CRUCIAL: Update only the necessary fields. Do not overwrite 'Created' or 'CreatedBy' fields.
            department.DepartmentName = request.DepartmentName;
            department.Updated = DateTime.UtcNow;
            department.UpdatedBy = "System"; // Akan diganti saat sistem login sudah siap

            _Context.MstDepartments.Update(department);
            _Context.SaveChanges();

            response.Result = true;
            response.Message = "Update Success";
            return response;
        }

        public BaseResponse<bool> DeleteDepartment(int id)
        {
            var response = new BaseResponse<bool>();

            // CRUCIAL: Find the department that is not yet deleted
            var department = _Context.MstDepartments.FirstOrDefault(d => d.Id == id && !d.IsDeleted);

            if (department == null)
            {
                response.Message = "Department not found.";
                response.Result = false;
                return response;
            }

            // CRUCIAL: Soft Delete implementation. We only update the flag, we do NOT use _Context.Remove()
            department.IsDeleted = true;
            department.Updated = DateTime.UtcNow;
            department.UpdatedBy = "System"; // Akan diganti saat sistem login sudah siap

            _Context.MstDepartments.Update(department);
            _Context.SaveChanges();

            response.Result = true;
            response.Message = "Delete Success";
            return response;
        }
    }

}