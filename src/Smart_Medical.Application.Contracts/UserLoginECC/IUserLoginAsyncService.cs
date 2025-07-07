using Smart_Medical.RBAC.Users;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Smart_Medical.UserLoginECC
{
    public interface IUserLoginAsyncService : IApplicationService
    {
        Task<ApiResult<UserDto>> LoginAsync(LoginDto loginDto);
    }
}
