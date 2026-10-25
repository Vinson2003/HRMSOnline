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

            var departments = _Context.MstDepartments.Where(d => 
                (string.IsNullOrEmpty(searchName) || (d.DepartmentName != null && d.DepartmentName.Contains(searchName))))
                .Select(d => new DepartmentList()
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    Created = d.Created,
                    CreatedBy = d.CreatedBy
                });

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

            response.Total = departments.Count();
            response.Result = departments.Skip(paging.Start).Take(paging.Length).ToList();

            return response;
        }

        public BaseResponse<bool> CreateDepartment(CreateDepartment request)
        {
            var response = new BaseResponse<bool>();

            var Department = _Context.MstDepartments.Where(d => d.IsDeleted == false);
            if (Department == null)
            {
                response.Message = "Department Not Found";
                return response;
            }

            MstDepartment newDepartment = new()
            {
                DepartmentName = request.DepartmentName,
                Created = DateTime.UtcNow,
            };
            _Context.MstDepartments.Add(newDepartment);
            _Context.SaveChanges();

            response.Result = true;
            response.Message = "AddSuccess";
            return response;
        }
    }

}