using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/user")]
[ApiController]
[SwaggerTag("用户相关 API ")]
public class UserController(OracleDbContext context) : ControllerBase
{
    // 获得用户所有数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取所有用户数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<User>>> GetUser()
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
    public async Task<ActionResult<User>> GetUserPk(int id)
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
    [SwaggerOperation(Summary = "添加用户数据")]
    [SwaggerResponse(201, "添加数据成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PostUser([FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            context.UserSet.Add(user);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserPk), new { id = user.UserId }, user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 更新指定 ID 的用户
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "更新指定 ID 的用户数据")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PutUser(int id, [FromBody] User user)
    {
        try
        {
            if (id != user.UserId)
            {
                return BadRequest("The ID in URL does not match the user ID in the request body.");
            }

            if (!await context.UserSet.AnyAsync(u => u.UserId == id))
            {
                return NotFound($"No user found with ID: {id}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok($"User with ID {id} changed successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 删除指定 ID 的用户
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "删除指定 ID 的用户数据")]
    [SwaggerResponse(200, "删除成功")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await context.UserSet.FindAsync(id);
            if (user == null)
            {
                return NotFound($"No user found with ID: {id}");
            }

            context.UserSet.Remove(user);
            await context.SaveChangesAsync();

            return Ok(new { code = 200, message = $"User with ID {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}