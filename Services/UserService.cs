using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webapi.Data.DbContexts;
using webapi.Dto;
using webapi.Model;

namespace webapi.Services{
    public interface IUserService
    {
        Task<PagedResult<UsersResponse>>GetAsync(string? keyword, int Page=1,int PageSize=10);

        Task<UserResponse>GetByIdAsync(int UserId);
    }
    public class UserService : IUserService
    {
        private readonly AppDbContext db;

        public UserService(AppDbContext db)
        {
            this.db = db;
        }
        public async Task<List<User>> GetAsync()
        {
            return await db.Users.ToListAsync();
        }

        public async Task<PagedResult<UsersResponse>> GetAsync(string? keyword, int Page = 1, int PageSize = 10)
        {
            Page = Math.Max(Page,1);
            PageSize=Math.Clamp(PageSize,1,200);
            var BaseQuery=from u in db.Users.AsNoTracking()
                            join ru in db.RoleUsers.AsNoTracking() on u.UserId equals ru.UserId into rujoin
                            from ru in rujoin.DefaultIfEmpty()
                            join r in db.Roles.AsNoTracking() on ru.RoleId equals r.RoleId into rjoin
                            from r in rjoin.DefaultIfEmpty()
                            select new {u,ru,r};

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                BaseQuery = BaseQuery.Where(x => (x.u.Username != null && x.u.Username.Contains(k)) ||
                                                 (x.u.Firstname != null && x.u.Firstname.Contains(k)) ||
                                                 (x.u.Lastname != null && x.u.Lastname.Contains(k)) ||
                                                 (x.u.Email != null && x.u.Email.Contains(k)) ||
                                                 (x.r != null && x.r.RoleName != null && x.r.RoleName.Contains(k))
                );
            }
            var Total = await BaseQuery.CountAsync();
            var Items =await BaseQuery.OrderBy(x=>x.u.UserId)
            .Skip((Page-1)*PageSize)
            .Take(PageSize)
            .Select(x=>new UsersResponse
            {
                UserId= x.u.UserId,
                Name=x.u.Firstname+" "+x.u.Lastname,
                Username=x.u.Username,
                Email=x.u.Email ?? "-",
                Role=x.r.RoleName,
                IsActive=x.u.IsActive,
            }).ToListAsync();

            
            return new PagedResult<UsersResponse>
            {
                Page = Page,
                PageSize = PageSize,
                TotalItems = Total,
                Items = Items
            };
        }

        public async Task<UserResponse> GetByIdAsync(int UserId)
        {
                       var BaseQuery = from u in db.Users.AsNoTracking()
                            join ru in db.RoleUsers.AsNoTracking() on u.UserId equals ru.UserId
                            join r in db.Roles.AsNoTracking() on ru.RoleId equals r.RoleId
                            where u.UserId == UserId
                            select new UserResponse
                            {
                                UserId = u.UserId,
                                Firstname = u.Firstname,
                                Lastname = u.Lastname,
                                Username = u.Username,
                                Email = u.Email,
                                IsActive = u.IsActive,
                                RoleId = r.RoleId,
                                RoleName = r.RoleName
                            };

            return await BaseQuery.FirstOrDefaultAsync();
        }
    }
}