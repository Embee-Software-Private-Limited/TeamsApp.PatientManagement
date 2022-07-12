using TeamsApp.PatientManagement.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Common.Providers
{
    public interface IDepartmentStorageProvider
    {
        Task<bool> AddEntityAsync(DepartmentEntity entity);
        Task<bool> DeleteEntityAsync(DepartmentEntity entity);
        Task<IList<DepartmentEntity>> GetAllAsync();
        Task<DepartmentEntity> GetAsync(string departmentId);
    }
}