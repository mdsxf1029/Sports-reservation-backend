using Microsoft.EntityFrameworkCore;

using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Data;

public class OracleDbContext(DbContextOptions<OracleDbContext> options) : DbContext(options)
{
    // 配置数据库上下文实体集
    public DbSet<Appointment> AppointmentSet { get; set; }
    public DbSet<CheckIn> CheckInSet { get; set; }
    public DbSet<Comment> CommentSet { get; set; }
    public DbSet<CommentLike> CommentLikeSet { get; set; }
    public DbSet<CommentReport> CommentReportSet { get; set; }
    public DbSet<Venue> FacilitySet { get; set; }
    public DbSet<Post> PostSet { get; set; }
    public DbSet<PostComment> PostCommentSet { get; set; }
    public DbSet<PostLike> PostLikeSet { get; set; }
    public DbSet<PostReport> PostReportSet { get; set; }
    public DbSet<User> UserSet { get; set; }
    public DbSet<UserCollection> UsersCollectionSet { get; set; }
    public DbSet<UserComment> UsersCommentSet { get; set; }
    public DbSet<UserPost> UsersPostSet { get; set; }
    
    // 重写 OnModelCreating 方法
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
    }
}