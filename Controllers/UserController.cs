using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Dto;
using webapi.Services;

namespace webapi.Controllers{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService service;
        public UserController(IUserService service)
        {
            this.service=service;
        }
        [HttpGet]
        public async Task<IActionResult> GetPage(string? keyword =null,int Page=1, int PageSize=10)
        {
            return Ok(await service.GetAsync(keyword,Page,PageSize));
        }
        [HttpGet("{Id:int}")]
        public async Task<IActionResult>GetById(int Id)
        {
            var result =await service.GetByIdAsync(Id);
            return result is null ? NotFound(new {message="ไม่พบข้อมูล"}): Ok(result);
        }

        [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest Request)
        {
            var Created = await service.CreateAsync(Request);
            return CreatedAtAction(nameof(GetById), new { id = Created.UserId }, Created);
        }
        [HttpPut("{Id:int}")]
    public async Task<IActionResult> Update([FromRoute] int Id, [FromBody] UserUpdateRequest Request)
        {
            var Updated = await service.UpdateAsync(Id, Request);
            return Updated is null ? NotFound() : Ok(Updated);
        }
        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int Id)
        {
            var ok = await service.DeleteAsync(Id);
            return ok ? NoContent() : NotFound();
        }
        [HttpGet("role")]
        public async Task<IActionResult> GetRole()
        {
            var Result = await service.GetRoleList();
            return Ok(Result);
        }
    }
}