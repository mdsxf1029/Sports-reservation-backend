using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/venue")]
[ApiController]
[SwaggerTag("场地相关 API ")]
public class VenueController(OracleDbContext context) : ControllerBase
{
    // 获得场地表所有数据
    [HttpGet]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Venue>>> GetVenue()
    {
        try
        {
            return Ok(await context.VenueSet.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // 获得指定主键的场地数据
    [HttpGet("{id:int}")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Venue>> GetVenueByPk(int id)
    {
        try
        {
            var venue = await context.VenueSet.FindAsync(id);
            if (venue == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            return Ok(venue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // 更新指定主键的场地数据
    [HttpPut("{id:int}")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "未找到数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult> UpdateVenue(int id,[FromBody] Venue venue)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (id != venue.VenueId)
        {
            return BadRequest("ID mismatch");
        }
        
        var curVenue = await context.VenueSet.FindAsync(id);
        if (curVenue == null)
        {
            return NotFound($"No corresponding data found for ID: {id}");
        }
        
        context.Entry(curVenue).CurrentValues.SetValues(venue);
        context.Entry(curVenue).State = EntityState.Modified;
        
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        
        return Ok($"Data with ID: {id} has been updated successfully.");
    }
    
    // 添加场地数据
    [HttpPost]
    
    public async Task<IActionResult> PostVenue([FromBody] Venue venue)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        context.VenueSet.Add(venue);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(PostVenue), new { id = venue.VenueId }, venue);
    }
    
    // 删除场地数据
    [HttpDelete("{id:int}")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Venue>> DeleteVenueByPk(int id)
    {
        try
        {
            var venue = await context.VenueSet.FindAsync(id);
            if (venue == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            
            context.VenueSet.Remove(venue);
            await context.SaveChangesAsync();
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // 根据场地类型筛选
    
    
    // 根据场地名称筛选


    // 根据场地位置筛选
    
    
}