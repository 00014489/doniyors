using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Services.Common;

namespace backend.Services.UserAdminService
{
    public interface IUserAdminService
    {
        Task<IReadOnlyList<UserAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<UserAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserRoleDto>> GetRolesAsync(
            CancellationToken cancellationToken = default);

        Task<SaveResult<UserAdminDto>> CreateAsync(
            UserSaveDto request,
            CancellationToken cancellationToken = default);

        Task<SaveResult<UserAdminDto>> UpdateAsync(
            int id,
            UserSaveDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft delete: flips <c>Status</c> to <c>false</c> instead of removing the row.
        /// </summary>
        Task<UserAdminDto?> DisableAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
