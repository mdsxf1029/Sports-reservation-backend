using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/venue")]
[ApiController]
[SwaggerTag("场地相关 API")]
public class VenueController : ControllerBase
{
    private readonly OracleDbContext context;

    public VenueController(OracleDbContext context)
    {
        this.context = context;
    }

    // 获取所有场地数据
    [HttpGet]
    [SwaggerOperation(Summary = "获取所有场地数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
    {
        try
        {
            var venues = await context.VenueSet.ToListAsync();
            return Ok(venues);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 根据主键获取指定场地数据
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取场地数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Venue>> GetVenueById(int id)
    {
        try
        {
            var venue = await context.VenueSet.FindAsync(id);
            if (venue == null)
                return NotFound($"No corresponding data found for Venue ID: {id}");

            return Ok(venue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 添加场地数据
    [HttpPost]
    [SwaggerOperation(Summary = "添加场地数据")]
    [SwaggerResponse(201, "添加数据成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PostVenue([FromBody] Venue venue)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            context.VenueSet.Add(venue);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVenueById), new { id = venue.VenueId }, venue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 更新指定 ID 的场地
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "更新指定 ID 的场地数据")]
    [SwaggerResponse(200, "更新成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PutVenue(int id, [FromBody] Venue venue)
    {
        try
        {
            if (id != venue.VenueId)
                return BadRequest("The ID in URL does not match the venue ID in the request body.");

            if (!await context.VenueSet.AnyAsync(v => v.VenueId == id))
                return NotFound($"No venue found with ID: {id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            context.Entry(venue).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok(new { code = 200, message = $"Venue with ID {id} updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 删除指定 ID 的场地
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "删除指定 ID 的场地数据")]
    [SwaggerResponse(200, "删除成功")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> DeleteVenue(int id)
    {
        try
        {
            var venue = await context.VenueSet.FindAsync(id);
            if (venue == null)
                return NotFound($"No venue found with ID: {id}");

            context.VenueSet.Remove(venue);
            await context.SaveChangesAsync();

            return Ok(new { code = 200, message = $"Venue with ID {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
