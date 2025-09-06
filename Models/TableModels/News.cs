using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("NEWS")]
[SwaggerSchema(Description = "新闻表")]
public class News
{
    [Key]
    [Column("NEWS_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "新闻ID")]
    public int NewsId { get; set; }

    [Column("NEWS_CATEGORY")]
    [SwaggerSchema(Description = "新闻类别")]
    [StringLength(64)]
    public string? NewsCategory { get; set; } = string.Empty;
    
    [Column("NEWS_TITLE")]
    [SwaggerSchema(Description = "新闻标题")]
    [StringLength(128)]
    public string? NewsTitle { get; set; } = string.Empty;
    
    [Column("NEWS_CONTENT", TypeName = "clob")]
    [SwaggerSchema(Description = "新闻内容")]
    public string? NewsContent { get; set; } = string.Empty;

    [Column("NEWS_STATUS")]
    [StringLength(64)]
    [SwaggerSchema(Description = "新闻状态")]
    public string? NewsStatus { get; set; } = string.Empty;

    [Column("NEWS_TIME", TypeName = "date")]
    [SwaggerSchema(Description = "新闻发布时间")]
    public DateTime NewsTime { get; set; }

    [Column("COVER_URL")]
    [SwaggerSchema(Description = "封面图片URL")]
    [StringLength(256)]
    public string? CoverUrl { get; set; } = string.Empty;

    [Column("NEWS_SOURCE")]
    [SwaggerSchema(Description = "新闻来源")]
    [StringLength(128)]
    public string? NewsSource { get; set; } = string.Empty;

    [Column("PUBLISHED_BY")]
    [SwaggerSchema(Description = "发布管理员")]
    [StringLength(128)]
    public string? PublishedBy { get; set; } = string.Empty;
}