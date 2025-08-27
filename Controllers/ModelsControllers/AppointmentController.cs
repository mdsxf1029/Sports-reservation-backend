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

        //订单详情页
        [Authorize]
        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetAppointmentDetail(int appointmentId)
        {
            try
            {
                // 1. 查询 Appointment
                var appointment = await _db.AppointmentSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                    return Ok(new { success = false, message = "预约不存在" });

                // 2. 查询 VenueAppointment + Venue
                var venueInfo = await (from va in _db.VenueAppointmentSet
                                       join v in _db.VenueSet on va.VenueId equals v.VenueId
                                       where va.AppointmentId == appointmentId
                                       select new
                                       {
                                           venue_id = v.VenueId,
                                           venue_name = v.VenueName,
                                           venue_subname = v.VenueSubname,
                                           venue_type = v.VenueType,
                                           venue_location = v.VenueLocation,
                                           venue_capacity = v.VenueCapacity,
                                           venue_status = v.VenueStatus,
                                           venue_picture_url = v.VenuePictureUrl
                                       }).FirstOrDefaultAsync();

                // 3. 查询 Bill
                var billInfo = await _db.BillSet
                    .Where(b => b.AppointmentId == appointmentId)
                    .Select(b => new
                    {
                        bill_id = b.BillId,
                        bill_status = b.BillStatus,
                        bill_amount = b.BillAmount,
                        begin_time = b.BeginTime
                    }).FirstOrDefaultAsync();

                // 4. 查询 User
                var userAppointment = await _db.UserAppointmentSet
                    .Where(ua => ua.AppointmentId == appointmentId)
                    .Select(ua => ua.User)
                    .FirstOrDefaultAsync();

                var userInfo = userAppointment != null ? new
                {
                    user_id = userAppointment.UserId,
                    user_name = userAppointment.UserName
                } : null;

                // 5. 返回
                return Ok(new
                {
                    appointment = new
                    {
                        appointment_id = appointment.AppointmentId,
                        appointment_status = appointment.AppointmentStatus,
                        apply_time = appointment.ApplyTime,
                        begin_time = appointment.BeginTime,
                        end_time = appointment.EndTime
                    },
                    venue = venueInfo,
                    bill = billInfo,
                    user = userInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取预约详情失败，appointmentId: {AppointmentId}", appointmentId);
                return Ok(new { success = false, message = "获取预约详情失败" });
            }
        }

    }
}
