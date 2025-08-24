namespace Sports_reservation_backend.Models.ResponseModels;

public class UserResponse
{
    public int UserId { get; set; }        
    
    public required string UserName { get; set; }    
    
    public int Points { get; set; }        
    
    public required string AvatarUrl { get; set; }   
    
    public required string Gender { get; set; }      
    
    public required string Profile { get; set; }     
    
    public required string Region { get; set; }      
}