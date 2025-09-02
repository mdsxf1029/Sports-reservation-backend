using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[ApiController]
[Route("api/news")]
public class NewsController(OracleDbContext context, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet("{newsId:int}")]
    [SwaggerOperation(Summary = "获取新闻表信息", Description = "获取新闻表信息")]
    public async Task<ActionResult<IEnumerable<News>>> GetNewsByPk(int newsId)
    {
        try
        {
            var news = await context.NewsSet.FindAsync(newsId);

            if (news is null)
            {
                return NotFound($"No corresponding data found for ID: {newsId}");
            }
            
            return Ok(news);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet]
    [SwaggerOperation(Summary = "获取新闻表信息", Description = "获取新闻表信息")]
    public async Task<ActionResult<IEnumerable<News>>> GetNews(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if(page < 1) page = 1;
        
        if(pageSize < 10) pageSize = 10;
        
        try
        {
            var query = context.NewsSet.AsQueryable();
            
            query = query.Where(n => (n.NewsStatus == "published" || n.NewsStatus == "updated"));

            var totalCount = await query.CountAsync();
            
            var news = await query
                .OrderByDescending(n => n.NewsTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = news
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("category/{category}")]
    [SwaggerOperation(Summary = "获取新闻表信息", Description = "获取新闻表信息")]
    public async Task<ActionResult<IEnumerable<News>>> GetNewsByCategory(
        string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if(page < 1) page = 1;
        
        if(pageSize < 10) pageSize = 10;
        
        try
        {
            var query = context.NewsSet.Where(n => (n.NewsStatus == "published" || n.NewsStatus == "updated")).AsQueryable();
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(n => n.NewsCategory == category);
            }
            
            var totalCount = await query.CountAsync();

            var news = await query
                .OrderByDescending(n => n.NewsTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = news
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<News>>> GetNewsByStatus(
        string status, [FromQuery] string category = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if(page < 1) page = 1;
        
        if(pageSize < 10) pageSize = 10;

        try
        {
            var query = context.NewsSet.AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.NewsStatus == status);
            }
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(n => n.NewsCategory == category);
            }
            
            var totalCount = await query.CountAsync();
            
            var news = await query
                .OrderByDescending(n => n.NewsTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = news
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost]
    [SwaggerOperation(Summary = "新增新闻表信息", Description = "新增新闻表信息")]
    public async Task<ActionResult<IEnumerable<News>>> PostNews([FromBody] News news)
    {
        try
        {
            news.NewsTime = DateTime.Now;
            
            context.NewsSet.Add(news);
            await context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(PostNews), new { id = news.NewsId }, news);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("upload-cover")]
    [SwaggerOperation(Summary = "上传新闻封面", Description = "上传新闻封面")]
    public async Task<IActionResult> UploadCover(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }
        
        var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest("File format error.");
        }
        
        var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "news_cover");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        var coverUrl = $"{Request.Scheme}://{Request.Host}/uploads/news_cover/{fileName}";
        
        return Ok(new { coverUrl });
    }
    
    [HttpDelete("{newsId:int}")]
    [SwaggerOperation(Summary = "删除新闻表信息", Description = "删除新闻表信息")]
    public async Task<ActionResult<IEnumerable<News>>> DeleteNews(int newsId)
    {
        try
        {
            var news = await context.NewsSet.FindAsync(newsId);

            if (news is null)
            {
                return NotFound($"No corresponding data found for ID: {newsId}");
            }
            
            news.NewsStatus = "deleted";
            
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {newsId} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPut("{newsId:int}")]
    [SwaggerOperation(Summary = "更新新闻表信息", Description = "更新新闻表信息")]
    public async Task<ActionResult<IEnumerable<News>>> UpdateNews(int newsId, [FromBody] News news)
    {
        try
        {
            var currNews = await context.NewsSet.FindAsync(newsId);

            if (currNews is null)
            {
                return NotFound($"No corresponding data found for ID: {newsId}");
            }

            if (!string.IsNullOrEmpty(news.NewsCategory))
            {
                currNews.NewsCategory = news.NewsCategory;
            }
            
            if (!string.IsNullOrEmpty(news.NewsTitle))
            {
                currNews.NewsTitle = news.NewsTitle;
            }
            
            if (!string.IsNullOrEmpty(news.NewsContent))
            {
                currNews.NewsContent = news.NewsContent;
            }
            
            if (!string.IsNullOrEmpty(news.NewsStatus))
            {
                currNews.NewsStatus = news.NewsStatus;
            }
            
            if (!string.IsNullOrEmpty(news.CoverUrl))
            {
                currNews.CoverUrl = news.CoverUrl;
            }
            
            context.Entry(currNews).State = EntityState.Modified;
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {newsId} has been updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.NewsSet.Any(e => e.NewsId == newsId))
            {
                return NotFound($"No corresponding data found for ID: {newsId}");
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
