using Microsoft.EntityFrameworkCore;

using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Data;

public class OracleDbContext(DbContextOptions<OracleDbContext> options) : DbContext(options)
{
    // 配置数据库上下文实体集
    public DbSet<Appointment> AppointmentSet { get; set; }
    public DbSet<Bill> BillSet { get; set; }
    public DbSet<BlackList> BlackListSet { get; set; }
    public DbSet<CheckIn> CheckInSet { get; set; }
    public DbSet<Comment> CommentSet { get; set; }
    public DbSet<CommentDislike> CommentDislikeSet { get; set; }
    public DbSet<CommentLike> CommentLikeSet { get; set; }
    public DbSet<CommentReply> CommentReplySet { get; set; }
    public DbSet<CommentReport> CommentReportSet { get; set; }
    public DbSet<Maintenance> MaintenanceSet { get; set; }
    public DbSet<ManagerCommentReport> ManagerCommentReportSet { get; set; }
    public DbSet<ManagerPostReport> ManagerPostReportSet { get; set; }
    public DbSet<PointChange> PointChangeSet { get; set; }
    public DbSet<Post> PostSet { get; set; }
    public DbSet<PostComment> PostCommentSet { get; set; }
    public DbSet<PostCollection> PostCollectionSet { get; set; }
    public DbSet<PostDislike> PostDislikeSet { get; set; }
    public DbSet<PostLike> PostLikeSet { get; set; }
    public DbSet<PostReport> PostReportSet { get; set; }
    public DbSet<TimeSlot> TimeSlotSet { get; set; }
    public DbSet<User> UserSet { get; set; }
    public DbSet<UserAppointment> UserAppointmentSet { get; set; }
    public DbSet<UserComment> UserCommentSet { get; set; }
    public DbSet<UserPost> UserPostSet { get; set; }
    public DbSet<UserViolation> UserViolationSet { get; set; }
    public DbSet<Venue> VenueSet { get; set; }
    public DbSet<VenueAppointment> VenueAppointmentSet { get; set; }
    public DbSet<VenueManager> VenueManagerSet { get; set; }
    public DbSet<VenueTimeSlot> VenueTimeSlotSet { get; set; }
    public DbSet<Violation> ViolationSet { get; set; }

    // 重写 OnModelCreating 方法
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


    }
}