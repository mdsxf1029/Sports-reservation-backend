using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("FACILITY")]
[SwaggerSchema(Description = "场地表")]
public class Facility
{
    // 属性定义
    [Key]
    [Column("FACILITY_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "场地ID")]
    public int FacilityId { get; set; }
    
    [Required]
    [Column("FACILITY_NAME")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "场地名称")]
    public string FacilityName { get; set; }=string.Empty;
    
    [Required]
    [Column("FACILITY_TYPE")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "场地类型")]
    public string FacilityType { get; set; }=string.Empty;
    
    [Required]
    [Column("FACILITY_LOCATION")]
    [MaxLength(50)]
    [SwaggerSchema(Description = "场地地点")]
    public string FacilityLocation { get; set; }=string.Empty;
    
    [Required]
    [Column("FACILITY_MAX_APPOINTMENT")]
    [SwaggerSchema(Description = "最大人数")]
    public int FacilityMaxAppointment { get; set; }
    
    [Required]
    [Column("FACILITY_PUBLISH_STATUS")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "场地发布状态")]
    public string FacilityPublishStatus { get; set; } = string.Empty;
    
}