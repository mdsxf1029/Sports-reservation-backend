using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USER")]
[SwaggerSchema(Description = "用户表")]
public class User
{
    [Key]
    [Column("USER_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "用户ID", ReadOnly = true)]
    public int UserId { get; set; }
    
    [Column("USER_NAME")]
    [MaxLength(50)]
    [SwaggerSchema(Description = "用户名")]
    public string UserName { get; set; } = string.Empty;
    
    [Column("TELEPHONE")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "电话")]
    public string Telephone { get; set; } = string.Empty;
    
    [Column("EMAIL")]
    [MaxLength(30)]
    [SwaggerSchema(Description = "邮箱")]
    public string Email { get; set; } = string.Empty;
    
    [Column("REGISTER_TIME")]
    [SwaggerSchema(Description = "注册日期")]
    public DateTime RegisterTime { get; set; }
    
    [Column("POINTS")]
    [SwaggerSchema(Description = "积分值")]
    public int Points { get; set; }

    [Column("AVATAR_URL")]
    [StringLength(255)]
    [SwaggerSchema(Description = "头像")]
    public string AvatarUrl { get; set; } = string.Empty;
    
    [Column("GENDER")]
    [MaxLength(10)]
    [SwaggerSchema(Description = "性别")]
    public string Gender { get; set; } = string.Empty;
    
    [Column("BIRTHDAY")]
    [SwaggerSchema(Description = "出生日期")]
    public DateTime Birthday { get; set; }
    
    [Column("PROFILE")]
    [StringLength(512)]
    [SwaggerSchema(Description = "简介")]
    public string Profile { get; set; } = string.Empty;

    [Column("REGION")]
    [MaxLength(255)]
    [SwaggerSchema(Description = "所处地区")]
    public string Region { get; set; } = string.Empty;

    [Column("PASSWORD")]
    [MaxLength(30)]
    [SwaggerSchema(Description = "密码")]
    public string Password { get; set; } = string.Empty;
    
    [Column("ROLE")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "角色")]
    public string Role { get; set; } = string.Empty;
}