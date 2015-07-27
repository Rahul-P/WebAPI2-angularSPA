
namespace Authentication.API.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RefreshToken
    {
        //Id column contains hashed value of the refresh token id, 
        //the API consumer will receive and send the 
        //plain refresh token Id
        [Key]
        public string Id { get; set; }

        //Subject column indicates to which user this refresh token belongs
        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }

        //Client Id column indicates to which user this refresh token belongs
        [Required]
        [MaxLength(50)]
        public string ClientId { get; set; }

        //The Issued UTC and Expires UTC columns are for 
        //displaying purpose only, I’m not building my 
        //refresh tokens expiration logic based on these values
        public DateTime IssuedUtc { get; set; }

        public DateTime ExpiresUtc { get; set; }

        //the Protected Ticket column contains magical signed 
        //string which contains a serialized representation for 
        //the ticket for specific user, in other words it 
        //contains all the claims and ticket properties 
        //for this user. The Owin middle-ware will use this 
        //string to build the new access token auto-magically
        [Required]
        public string ProtectedTicket { get; set; }
    }
}