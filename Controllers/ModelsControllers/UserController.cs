using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/user")]
[ApiController]
[SwaggerTag("用户相关 API ")]
public class UserController(OracleDbContext context): ControllerBase
{
    // 获得场用户所有数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取所有用户数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Venue>>> GetUser()
    {
        try
        {
            return Ok(await context.UserSet.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // 获得指定主键的用户数据
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取用户数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Venue>> GetUserPk(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // 添加用户数据
    [HttpPost]
    [SwaggerResponse(201, "添加数据成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PostUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        context.UserSet.Add(user);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(PostUser), new { id = user.UserId }, user);
    }
}