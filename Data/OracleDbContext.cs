using Microsoft.EntityFrameworkCore;

using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Data;

public class OracleDbContext(DbContextOptions<OracleDbContext> options) : DbContext(options)
{
    // 配置数据库上下文实体集
    public DbSet<Appointment> AppointmentSet { get; set; }
    public DbSet<Comment> CommentSet { get; set; }
    
}