
namespace Authentication.API.Entities
{
    using Authentication.API.Models;
    using System.ComponentModel.DataAnnotations;

    public class Client
    {
        [Key]
        public string Id { get; set; }

        //The Secret column is hashed
        [Required]
        public string Secret { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ApplicationTypeEnums ApplicationType { get; set; }

        //The Active column is very useful; if the system admin decided 
        //to deactivate this client, so any new requests asking for 
        //access token from this deactivated client will be rejected
        public bool Active { get; set; }

        //The Refresh Token Life Time column is used to set when the 
        //refresh token (not the access token) will expire in minutes! 
        //it is 
        //nice feature because now you can control the expiry for 
        //refresh tokens for each client.
        public int RefreshTokenLifeTime { get; set; }

        //the Allowed Origin column is used configure CORS and to set 
        //“Access-Control-Allow-Origin” on the back-end API. 
        //It is only useful for JavaScript applications using 
        //XHR requests, so in my case I’ m setting the 
        //allowed origin for client id “ngAuthApp” to 
        //origin “http://logicmonks-authenticationweb.azurewebsites.net/” 
        //and this turned out to be very useful, so if any malicious 
        //user obtained my client id from my JavaScript app which 
        //is very trivial to do, he will not be able to use this 
        //client to build another JavaScript application using the 
        //same client id because all preflighted  requests will 
        //fail and return 405 HTTP status (Method not allowed) 
        //All XHR requests coming for his JavaScript app will be 
        //from different domain. This is valid for JavaScript 
        //application types only, for other application types 
        //you can set this to “*”
        [MaxLength(100)]
        public string AllowedOrigin { get; set; }
    }
}