namespace Sports_reservation_backend.Models.ResponseModels;

public class UserResponse
{
    public int userId { get; set; }        
    
    public required string username { get; set; }    
    
    public int points { get; set; }        
    
    public required string avatarUrl { get; set; }   
    
    public required string gender { get; set; }      
    
    public required string profile { get; set; }     
    
    public required string region { get; set; }      
}