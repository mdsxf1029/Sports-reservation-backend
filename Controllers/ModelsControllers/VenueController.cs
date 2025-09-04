using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Sports_reservation_backend.Models.RequestModels;

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
                hours = v.OpeningHours,
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
                    hours = venue.OpeningHours,
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

                // 获取今天和未来6天的日期（共7天）
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(6);

                // 创建7天的日期范围
                var dateRange = Enumerable.Range(0, 7)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList();

                var result = new List<object>();

                foreach (var date in dateRange)
                {
                    // 查找该日期的时间段
                    var dateSlots = venueTimeSlots
                        .Where(vts => vts.TimeSlot!.BeginTime!.Value.Date == date)
                        .ToList();

                    // 判断当天是否有时间段数据
                    if (!dateSlots.Any())
                    {
                        // 如果没有设置时间段，显示"未设置"
                        result.Add(new
                        {
                            weekday = GetChineseWeekday(date.DayOfWeek),
                            date = date.ToString("MM-dd"),
                            status = "未设置"
                        });
                    }
                    else
                    {
                        // 判断当天是否有可预约时间段
                        var hasAvailable = dateSlots.Any(vts => vts.TimeSlotStatus != "busy");

                        result.Add(new
                        {
                            weekday = GetChineseWeekday(date.DayOfWeek),
                            date = date.ToString("MM-dd"),
                            status = hasAvailable ? "可预约" : "已订满"
                        });
                    }
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

        [HttpGet("get")]
        public async Task<IActionResult> GetVenues(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? keyword = null,
    [FromQuery] string? status = null,
    [FromQuery] string? type = null)
        {
            try
            {
                var query = _db.VenueSet.AsQueryable();

                // 关键字搜索 (模糊匹配场馆名称)
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(v => v.VenueName.Contains(keyword));
                }

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(v => v.VenueStatus == status);
                }

                // 类型筛选
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(v => v.VenueType == type);
                }

                // 总数
                var total = await query.CountAsync();

                // 分页
                var venues = await query
                    .OrderBy(v => v.VenueId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 转换成返回格式
                var list = venues.Select(v => new
                {
                    id = v.VenueId,
                    name = v.VenueName,
                    type = v.VenueType,
                    price = v.Price,
                    price_unit = v.PriceUnit,
                    location = v.VenueLocation,
                    maxOccupancy = v.VenueCapacity,
                    status = v.VenueStatus == "开放" ? "开放" : "关闭",
                    openingHours = v.OpeningHours,
                    bookingHours = v.BookingHours
                });

                // 统计信息基于当前筛选条件
                var summary = new
                {
                    open_venues = await query.CountAsync(v => v.VenueStatus == "开放"),
                    closed_venues = await query.CountAsync(v => v.VenueStatus == "关闭"),
                    venue_types = await query
                        .Select(v => v.VenueType)
                        .Distinct()
                        .ToListAsync()
                };


                return Ok(new
                {
                    code = 200,
                    message = "获取场地列表成功",
                    data = new
                    {
                        list,
                        total,
                        summary
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取场地列表失败");
                return Ok(new
                {
                    code = 500,
                    message = "获取场地列表失败",
                    data = new { list = new object[] { }, total = 0, summary = new { } }
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateVenue([FromBody] VenueCreateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Type))
                {
                    return Ok(new
                    {
                        code = 400,
                        message = "场地名称和类型不能为空",
                        data = (object?)null
                    });
                }

                var venue = new Venue
                {
                    VenueName = request.Name,
                    VenueType = request.Type,
                    VenueSubname = request.Subname,
                    VenuePictureUrl = request.Pictureurl,
                    Price = request.Price,
                    PriceUnit = "小时", // 默认值
                    VenueLocation = request.Location,
                    OpeningHours = request.OpeningHours,
                    BookingHours = request.BookingHours,
                    VenueCapacity = request.MaxOccupancy,
                    VenueStatus = request.Status
                };

                _db.VenueSet.Add(venue);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    code = 200,
                    message = "发布成功",
                    data = new
                    {
                        id = venue.VenueId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建场地失败");
                return Ok(new
                {
                    code = 500,
                    message = "创建失败",
                    data = (object?)null
                });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVenue(int id, [FromBody] VenueCreateRequest request)
        {
            try
            {
                // 检查必要字段
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Type))
                {
                    return Ok(new
                    {
                        code = 400,
                        message = "场地名称和类型不能为空",
                        data = (object?)null
                    });
                }

                // 查找要修改的场地
                var venue = await _db.VenueSet.FindAsync(id);
                if (venue == null)
                {
                    return Ok(new
                    {
                        code = 404,
                        message = "未找到该场地",
                        data = (object?)null
                    });
                }

                // 更新字段
                venue.VenueName = request.Name;
                venue.VenueType = request.Type;
                venue.VenueSubname = request.Subname;
                venue.VenuePictureUrl = request.Pictureurl;
                venue.Price = request.Price;
                venue.PriceUnit = "小时";
                venue.VenueLocation = request.Location;
                venue.OpeningHours = request.OpeningHours;
                venue.BookingHours = request.BookingHours;
                venue.VenueCapacity = request.MaxOccupancy;
                venue.VenueStatus = request.Status;

                _db.VenueSet.Update(venue);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    code = 200,
                    message = "修改成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改场地失败");
                return Ok(new
                {
                    code = 500,
                    message = "修改失败",
                    data = (object?)null
                });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. 查询场地是否存在
                var venue = await _db.VenueSet.FindAsync(id);
                if (venue == null)
                {
                    return Ok(new { code = 404, message = "场地不存在" });
                }

                // 2. 删除关联的时间段
                var timeSlots = _db.VenueTimeSlotSet.Where(vts => vts.VenueId == id);
                _db.VenueTimeSlotSet.RemoveRange(timeSlots);

                // 3. 删除场地
                _db.VenueSet.Remove(venue);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { code = 200, message = "删除成功" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "删除场地失败");
                return Ok(new { code = 500, message = "删除失败" });
            }
        }

        [HttpPut("batch-status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchStatusRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0 || string.IsNullOrEmpty(request.Status))
            {
                return Ok(new
                {
                    code = 400,
                    message = "请求参数不完整"
                });
            }

            // 只允许“开放”和“关闭”
            if (request.Status != "开放" && request.Status != "关闭")
            {
                return Ok(new
                {
                    code = 400,
                    message = "状态值非法，只能是“开放”或“关闭”"
                });
            }

            try
            {
                var venues = await _db.VenueSet
                    .Where(v => request.Ids.Contains(v.VenueId))
                    .ToListAsync();

                foreach (var venue in venues)
                {
                    venue.VenueStatus = request.Status;
                }

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    code = 200,
                    message = "批量更新成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新场地状态失败");
                return Ok(new
                {
                    code = 500,
                    message = "批量更新失败，请稍后重试"
                });
            }
        }
    }
}
