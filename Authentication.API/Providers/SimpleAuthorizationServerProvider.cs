
namespace Authentication.API.Providers
{
    using Authentication.API.Entities;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OAuth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;

    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {

        //The first method is responsible for validating the “Client”, in our case we have only one 
        //client so we’ll always return that its validated successfully.
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {

            //1.
            // We are trying to get the Client id and secret from the authorization 
            //header using a basic scheme so one way to send the client_id/client_secret 
            //is to base64 encode the (client_id:client_secret) and send it in the Authorization header. 
            //The other way is to sent the client_id/client_secret as “x-www-form-urlencoded”. 
            //In my case I’m supporting the both approaches so client can set those values 
            //using any of the two available options.

            //2.
            // We are checking if the consumer didn’t set client information at all, 
            //so if you want to enforce setting the client id always then you need to 
            //invalidate the context. In my case I’m allowing to send requests without 
            //client id for the sake of keeping old post and demo working correctly.

            //3.
            // After we receive the client id we need to check our database if the client 
            //is already registered with our back-end API, if it is not registered we’ll 
            //invalidate the context and reject the request.

            string clientId = string.Empty;
            string clientSecret = string.Empty;
            Client client = null;

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                //Remove the comments from the below line context.SetError, and invalidate context 
                //if you want to force sending clientId/secrects once obtain access tokens. 
                context.Validated();
                //context.SetError("invalid_clientId", "ClientId should be sent.");
                return Task.FromResult<object>(null);
            }

            using (AuthRepository _repo = new AuthRepository())
            {
                client = _repo.FindClient(context.ClientId);
            }

            if (client == null)
            {
                context.SetError("invalid_clientId", string.Format
                    ("Client '{0}' is not registered in the system.", context.ClientId));
                return Task.FromResult<object>(null);
            }

            // 4.
            // If the client is registered we need to check his application type, so if 
            //it was “JavaScript – Non Confidential” client we’ll not check or ask for the secret. 
            //If it is Native – Confidential app then the client secret is mandatory and it will 
            //be validated against the secret stored in the database.

            // Native Application
            if (client.ApplicationType == Models.ApplicationTypeEnums.NativeConfidential)
            {
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    context.SetError("invalid_clientId", "Client secret should be sent.");
                    return Task.FromResult<object>(null);
                }
                else
                {
                    if (client.Secret != Helper.GetHash(clientSecret))
                    {
                        context.SetError("invalid_clientId", "Client secret is invalid.");
                        return Task.FromResult<object>(null);
                    }
                }
            }

            // 5.
            // Then we’ll check if the client is active, if it is not the case then 
            //we’ll invalidate the request.
            if (!client.Active)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
                return Task.FromResult<object>(null);
            }

            // 6.
            // Lastly we need to store the client allowed origin and refresh token life time value on the 
            // Owin context so it will be available once we generate the refresh token and set 
            // its expiry life time.
            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

            //7.
            //If all is valid we mark the context as valid context which means that client 
            //check has passed and the code flow can proceed to the next step.
            context.Validated();

            return Task.FromResult<object>(null);
        }

        //The second method “GrantResourceOwnerCredentials” is responsible to validate the username and 
        //password sent to the authorization server’s token endpoint, so we’ll use the “AuthRepository” 
        //class we created earlier and call the method “FindUser” to check if the username and password are valid.

        // Now we need to modify the method “GrantResourceOwnerCredentials” to validate that 
        //resource owner username/password is correct and bound the client id to the access token generated
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            //To allow CORS on the token middleware provider we need to add the 
            //header “Access-Control-Allow-Origin” to Owin context, if you forget this, 
            //generating the token will fail when you try to call it from your browser. 

            //Not that this allows CORS for token middleware provider not for ASP.NET Web API 
            //which we’ll add on the next step.
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });


            //Reading the allowed origin value for this client from the Owin context, then we 
            //use this value to add the header “Access-Control-Allow-Origin” to Owin context response, 
            //by doing this and for any JavaScript application we’ll prevent using the same client id 
            //to build another JavaScript application hosted on another domain; because the origin 
            //for all requests coming from this app will be from a different domain and the 
            //back-end API will return 405 status.
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

            if (allowedOrigin == null) allowedOrigin = "*";

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });


            //We’ll check the username/password for the resource owner if it is valid, 
            //and if this is the case we’ll generate set of claims for this user along 
            //with authentication properties which contains the client id and userName, 
            //those properties are needed for the next steps.
            using (AuthRepository _repo = new AuthRepository())
            {
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }

            //If the credentials are valid we’ll create “ClaimsIdentity” class and pass the authentication 
            //type to it, in our case “bearer token”, then we’ll add two claims (“sub”,”role”) and those 
            //will be included in the signed token. You can add different claims here but the token 
            //size will increase for sure.

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            var properties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { 
                        "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId
                    },
                    { 
                        "userName", context.UserName
                    }
                });

            //the access token will be generated behind the scenes when we call “context.Validated(ticket)”
            var ticket = new AuthenticationTicket(identity, properties);
            context.Validated(ticket);

            //Now generating the token happens behind the scenes when we call “context.Validated(identity)”.
            //context.Validated(identity);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            //We are reading the client id value from the original ticket, 
            //this is the client id which get stored in the magical signed string, 
            //then we compare this client id against the client id sent with the request, 
            //if they are different we’ll reject this request because we need to make 
            //sure that the refresh token used here is bound to the same client when it was generated.

            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return Task.FromResult<object>(null);
            }

            //We have the chance now to add new claims or remove existing claims, 
            //this was not achievable without refresh tokens, then we call 
            //“context.Validated(newTicket)” which will generate new access 
            //token and return it in the response body.


            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newClaim = newIdentity.Claims.Where(c => c.Type == "newClaim").FirstOrDefault();
            if (newClaim != null)
            {
                newIdentity.RemoveClaim(newClaim);
            }
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);

            //Lastly after this method executes successfully, the flow for the code 
            //will hit method “CreateAsync” in class “SimpleRefreshTokenProvider” 
            //and a new refresh token is generated and returned in the response 
            //along with the new access token.
        }
    }
}