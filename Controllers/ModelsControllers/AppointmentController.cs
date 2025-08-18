using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly OracleDbContext _db;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(OracleDbContext db, ILogger<AppointmentsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// 获取某用户的预约列表（分页）
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAppointments(
            [FromQuery] int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (userId <= 0)
                {
                    return Ok(new
                    {
                        code = 1001,
                        msg = "失败",
                        data = (object?)null
                    });
                }

                // 用户预约关联查询
                var query = from ua in _db.UserAppointmentSet
                            join a in _db.AppointmentSet on ua.AppointmentId equals a.AppointmentId
                            join va in _db.VenueAppointmentSet on a.AppointmentId equals va.AppointmentId
                            join v in _db.VenueSet on va.VenueId equals v.VenueId
                            where ua.UserId == userId
                            orderby a.BeginTime descending
                            select new
                            {
                                appointmentId = a.AppointmentId,
                                appointmentStatus = a.AppointmentStatus,
                                beginTime = a.BeginTime,
                                endTime = a.EndTime,
                                venueName = v.VenueName
                            };

                var total = await query.CountAsync();

                var list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    code = 0,
                    msg = "成功",
                    data = new
                    {
                        list,
                        total,
                        page,
                        pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取预约信息失败");
                return Ok(new
                {
                    code = 1001,
                    msg = "失败",
                    data = (object?)null
                });
            }
        }
    }
}
