using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Sports_reservation_backend.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;


namespace Sports_reservation_backend.Controllers
{
    [ApiController]
    [Route("api/courtreservation")]
    public class CourtReservationController : ControllerBase
    {
        private readonly OracleDbContext _db;
        private readonly ILogger<CourtReservationController> _logger;

        public CourtReservationController(OracleDbContext db, ILogger<CourtReservationController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有开放时间段
        /// GET /api/courtreservation/time-slots
        /// </summary>
        [HttpGet("time-slots")]
        public async Task<IActionResult> GetTimeSlots()
        {
            try
            {
                var slots = await _db.TimeSlotSet
                    .AsNoTracking()
                    .OrderBy(ts => ts.BeginTime)
                    .Select(ts => new
                    {
                        time_slot_id = ts.TimeSlotId,
                        begin_time = ts.BeginTime.HasValue ? ts.BeginTime.Value.ToString("yyyy/MM/dd HH:mm") : "",
                        end_time = ts.EndTime.HasValue ? ts.EndTime.Value.ToString("yyyy/MM/dd HH:mm") : ""
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    slots
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取时间段失败");
                return Ok(new
                {
                    success = false,
                    slots = new object[] { }
                });
            }
        }

        /// <summary>
        /// 获取某日期的锁定场地时间段
        /// GET /api/courtreservation/get-locked-cells?date=YYYY-MM-DD
        /// </summary>
        [HttpGet("get-locked-cells")]
        public async Task<IActionResult> GetLockedCells([FromQuery] string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime targetDate))
                {
                    return Ok(new
                    {
                        success = false,
                        locked = new object[] { },
                        msg = "日期格式错误"
                    });
                }

                // 查询指定日期锁定（busy）的场地时间段
                var locked = await _db.VenueTimeSlotSet
                    .Include(vts => vts.Venue)
                    .Include(vts => vts.TimeSlot)
                    .Where(vts => vts.TimeSlotStatus == "busy"
                                  && vts.TimeSlot.BeginTime.HasValue
                                  && vts.TimeSlot.BeginTime.Value.Date == targetDate.Date)
                    .Select(vts => new
                    {
                        venue_id = vts.VenueId,
                        venue_subname = vts.Venue != null ? vts.Venue.VenueSubname : "",
                        time_slot_id = vts.TimeSlotId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    locked
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询锁定场地时间段失败，date: {Date}", date);
                return Ok(new
                {
                    success = false,
                    locked = new object[] { },
                    msg = "查询失败"
                });
            }
        }

        /// <summary>
        /// 检查时间段是否被占用
        /// POST /api/courtreservation/check
        /// </summary>
        [HttpPost("check")]
        public async Task<IActionResult> Check([FromBody] CheckAndLockRequest request)
        {
            try
            {
                if (request.VenueId <= 0 || request.TimeSlotId <= 0 || string.IsNullOrEmpty(request.Date))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "参数无效"
                    });
                }

                if (!DateTime.TryParse(request.Date, out DateTime targetDate))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "日期格式错误"
                    });
                }

                // 查询对应的 VenueTimeSlot
                var vts = await _db.VenueTimeSlotSet
                    .Include(v => v.TimeSlot)
                    .FirstOrDefaultAsync(v =>
                        v.VenueId == request.VenueId &&
                        v.TimeSlotId == request.TimeSlotId &&
                        v.TimeSlot != null &&
                        v.TimeSlot.BeginTime.HasValue &&
                        v.TimeSlot.BeginTime.Value.Date == targetDate.Date
                    );

                if (vts == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "时间段不存在"
                    });
                }

                // 已经被占用
                if (vts.TimeSlotStatus == "busy")
                {
                    return Ok(new
                    {
                        success = false,
                        message = "已被占用"
                    });
                }

                // 可用
                return Ok(new
                {
                    success = true,
                    message = "可用"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查场地时间段失败，VenueId: {VenueId}, TimeSlotId: {TimeSlotId}, Date: {Date}",
                    request.VenueId, request.TimeSlotId, request.Date);

                return Ok(new
                {
                    success = false,
                    message = "检查失败"
                });
            }
        }


        [Authorize]
        [HttpPost("confirm-booking")]
        public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequest request)
        {
            if (!request.Success || request.Appointments == null || request.Appointments.Count == 0)
                return Ok(new { success = false, message = "预约请求为空" });

            var userIdStr = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Ok(new { success = false, message = "Token 无效" });

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Appointments)
                {
                    // 1. 查询 TimeSlot
                    var timeslot = await _db.TimeSlotSet.FirstOrDefaultAsync(ts => ts.TimeSlotId == item.TimeSlotId);
                    if (timeslot == null)
                        return Ok(new { success = false, message = $"时间段 {item.TimeSlotId} 无效" });

                    // 2. 查询 VenueTimeSlot
                    var vts = await _db.VenueTimeSlotSet
                        .FirstOrDefaultAsync(v => v.VenueId == item.VenueId && v.TimeSlotId == item.TimeSlotId);

                    if (vts == null)
                        return Ok(new { success = false, message = $"场地 {item.VenueId} 无效" });

                    if (vts.TimeSlotStatus == "busy")
                        return Ok(new { success = false, message = $"场地 {item.VenueId} 时间段 {item.TimeSlotId} 已被锁定" });

                    // 3. 计算预约开始/结束时间
                    var date = DateTime.Parse(item.Date).Date;
                    var beginDateTime = date + timeslot.BeginTime!.Value.TimeOfDay;
                    var endDateTime = date + timeslot.EndTime!.Value.TimeOfDay;

                    // 4. 创建 Appointment
                    var appointment = new Appointment
                    {
                        ApplyTime = DateTime.Now,
                        FinishTime = null,
                        BeginTime = beginDateTime,
                        EndTime = endDateTime,
                        AppointmentStatus = item.Status
                    };
                    _db.AppointmentSet.Add(appointment);
                    await _db.SaveChangesAsync(); // 获取 AppointmentId

                    // 5. 更新 VenueTimeSlot 状态
                    vts.TimeSlotStatus = "busy";

                    // 6. 创建 UserAppointment
                    _db.UserAppointmentSet.Add(new UserAppointment
                    {
                        AppointmentId = appointment.AppointmentId,
                        UserId = userId
                    });

                    // 7. 创建 VenueAppointment
                    _db.VenueAppointmentSet.Add(new VenueAppointment
                    {
                        AppointmentId = appointment.AppointmentId,
                        VenueId = item.VenueId
                    });

                    // 8. 创建 Bill（默认 pending，金额可自定义）
                    _db.BillSet.Add(new Bill
                    {
                        AppointmentId = appointment.AppointmentId,
                        UserId = userId,
                        BillStatus = "pending",
                        BillAmount = 1, // 可根据场地/时段价格计算
                        BeginTime = DateTime.Now
                    });
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "预约失败");
                return Ok(new { success = false, message = "预约失败，请稍后重试" });
            }
        }


    }
}
