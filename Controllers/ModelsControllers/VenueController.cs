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

        /// <summary>
        /// 获取场馆列表
        /// GET /api/venues/venuelist?campus=四平校区&type=羽毛球&search=二二九
        /// </summary>
        [HttpGet("venuelist")]
        public async Task<IActionResult> GetVenueList([FromQuery] string? campus, [FromQuery] string? type, [FromQuery] string? search)
        {
            var query = _db.VenueSet.AsQueryable();

            // 按类型过滤
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(v => v.VenueType.Contains(type));
            }

            // 按搜索关键词过滤（模糊匹配 name 和 address）
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v => v.VenueName.Contains(search) || v.VenueLocation.Contains(search));
            }

            var venues = await query.ToListAsync();

            // 转换成返回格式
            var result = venues.Select(v => new
            {
                id = v.VenueId,
                name = v.VenueName,
                address = v.VenueLocation,
                hours = "08:00-21:00",
                campus = v.VenueLocation.Contains("四平") ? "四平校区" : "嘉定校区",
                type = v.VenueType,
                image = v.VenuePictureUrl
            });

            // 按 campus 参数过滤（基于地址推算的结果）
            if (!string.IsNullOrEmpty(campus))
            {
                result = result.Where(r => r.campus == campus);
            }

            return Ok(new
            {
                code = 200,
                msg = "success",
                data = result
            });
        }

        /// <summary>
        /// 根据场馆 ID 获取场馆详情
        /// GET /api/venues/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenueById(int id)
        {
            try
            {
                var venue = await _db.VenueSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.VenueId == id);

                if (venue == null)
                {
                    return Ok(new
                    {
                        code = 404,
                        msg = "场馆不存在",
                        data = (object?)null
                    });
                }

                var result = new
                {
                    id = venue.VenueId,
                    name = venue.VenueName,
                    address = venue.VenueLocation,
                    hours = "09:00 - 21:00", // 先写死，后面可改成数据库字段
                    image = venue.VenuePictureUrl
                };

                return Ok(new
                {
                    code = 200,
                    msg = "success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据ID查询场馆失败，id: {Id}", id);
                return Ok(new
                {
                    code = 500,
                    msg = "查询失败",
                    data = (object?)null
                });
            }
        }

        [HttpGet("{id}/reservations")]
        public async Task<IActionResult> GetVenueReservations(int id)
        {
            try
            {
                // 查询该场馆所有时间段
                var venueTimeSlots = await _db.VenueTimeSlotSet
                    .Include(vts => vts.TimeSlot)
                    .Where(vts => vts.VenueId == id && vts.TimeSlot != null && vts.TimeSlot.BeginTime.HasValue)
                    .ToListAsync();

                if (!venueTimeSlots.Any())
                {
                    return Ok(new
                    {
                        code = 404,
                        msg = "该场馆未设置时间段",
                        data = new List<object>()
                    });
                }

                // 按日期分组
                var groupedByDate = venueTimeSlots
                    .GroupBy(vts => vts.TimeSlot!.BeginTime!.Value.Date)
                    .OrderBy(g => g.Key);

                var result = new List<object>();

                foreach (var group in groupedByDate)
                {
                    var date = group.Key;
                    // 判断当天是否有可预约时间段
                    var hasAvailable = group.Any(vts => vts.TimeSlotStatus != "busy");

                    result.Add(new
                    {
                        weekday = GetChineseWeekday(date.DayOfWeek),
                        date = date.ToString("MM-dd"),
                        status = hasAvailable ? "可预约" : "已订满"
                    });
                }

                return Ok(new
                {
                    code = 200,
                    msg = "success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询场馆预约情况失败 id: {Id}", id);
                return Ok(new
                {
                    code = 500,
                    msg = "查询失败",
                    data = new List<object>()
                });
            }
        }

        private string GetChineseWeekday(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Monday => "周一",
            DayOfWeek.Tuesday => "周二",
            DayOfWeek.Wednesday => "周三",
            DayOfWeek.Thursday => "周四",
            DayOfWeek.Friday => "周五",
            DayOfWeek.Saturday => "周六",
            DayOfWeek.Sunday => "周日",
            _ => ""
        };


    }
}
