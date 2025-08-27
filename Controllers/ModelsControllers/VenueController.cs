using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers
{
    [ApiController]
    [Route("api/venues")]
    public class VenuesController : ControllerBase
    {
        private readonly OracleDbContext _db;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(OracleDbContext db, ILogger<VenuesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// 根据场馆主名称查询子场地
        /// GET /api/venues?name=xxx
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubVenuesByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Ok(new
                    {
                        success = false,
                        venues = new object[] { }
                    });
                }

                var list = await _db.VenueSet
                    .AsNoTracking()
                    .Where(v => v.VenueName == name)
                    .OrderBy(v => v.VenueSubname)
                    .Select(v => new
                    {
                        venue_id = v.VenueId,
                        venue_subname = v.VenueSubname
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    venues = list
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询子场地失败，name: {Name}", name);
                return Ok(new
                {
                    success = false,
                    venues = new object[] { }
                });
            }
        }
    }
}
