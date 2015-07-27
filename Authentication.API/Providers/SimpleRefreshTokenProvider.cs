
namespace Authentication.API.Providers
{
    using Microsoft.Owin.Security.Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Threading.Tasks;
    using Authentication.API.Entities;


    //Now we need to generate the Refresh Token and Store it in our 
    //database inside the table “RefreshTokens”, to do the following 
    //we need to add new class named “SimpleRefreshTokenProvider” 
    //under folder “Providers” which implements the 
    //interface “IAuthenticationTokenProvider”

    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

            if (string.IsNullOrEmpty(clientid))
            {
                return;
            }

            //We are generating a unique identifier for the refresh token, I’m using Guid here which 
            //is enough for this or you can use your own unique string generation algorithm.
            var refreshTokenId = Guid.NewGuid().ToString("n");

            using (AuthRepository _repo = new AuthRepository())
            {
                //Then we are reading the refresh token life time value from the Owin context 
                //where we set this value once we validate the client, this value will 
                //be used to determine how long the refresh token will be valid for, this should be in minutes.

                var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");

                var token = new RefreshToken()
                {
                    Id = Helper.GetHash(refreshTokenId),
                    ClientId = clientid,
                    Subject = context.Ticket.Identity.Name,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
                };

                //Then we are setting the IssuedUtc, and ExpiresUtc values for the ticket, 
                //setting those properties will determine how long the refresh token will be valid for.

                context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

                //After setting all context properties we are calling method “context.SerializeTicket();” 
                //which will be responsible to serialize the ticket content and we’ll be able to store 
                //this magical serialized string on the database.
                token.ProtectedTicket = context.SerializeTicket();

                //After this we are building a token record which will be saved in RefreshTokens table, 
                //note that I’m checking that the token which will be saved on the database is unique 
                //for this Subject (User) and the Client, if it not unique I’ll delete the existing 
                //one and store new refresh token. It is better to hash the refresh token identifier 
                //before storing it, so if anyone has access to the database he’ll not see the 
                //real refresh tokens.
                var result = await _repo.AddRefreshToken(token);

                if (result)
                {
                    //Lastly we will send back the refresh token id (without HASHING it) in the response body.
                    context.SetToken(refreshTokenId);
                }
            }
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            //We need to set the “Access-Control-Allow-Origin” header by getting the value 
            //from Owin Context, I’ve spent more than 1 hour figuring out why my requests 
            //to issue access token using a refresh token returns 405 status code and it 
            //turned out that we need to set this header in this method because the 
            //method “GrantResourceOwnerCredentials” where we set this header is never 
            //get executed once we request access token using refresh tokens (grant_type=refresh_token)

            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", 
                new[] { allowedOrigin });

            //We get the refresh token id from the request, then hash this id and look 
            //for this token using the hashed refresh token id in table “RefreshTokens”, 
            //if the refresh token is found, we will use the magical signed string which 
            //contains a serialized representation for the ticket to build the ticket 
            //and identities for the user mapped to this refresh token.


            string hashedTokenId = Helper.GetHash(context.Token);

            using (AuthRepository _repo = new AuthRepository())
            {
                var refreshToken = await _repo.FindRefreshToken(hashedTokenId);

                if (refreshToken != null)
                {
                    //Get protectedTicket from refreshToken class
                    context.DeserializeTicket(refreshToken.ProtectedTicket);

                    //We’ll remove the existing refresh token from tables “RefreshTokens” 
                    //because in our logic we are allowing only one refresh token per user and client.
                    var result = await _repo.RemoveRefreshToken(hashedTokenId);
                }
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}