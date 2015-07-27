

// Some Notes

//Using Refresh Tokens
//The idea of using refresh token is to issue short lived access token at the first 
//place then use the refresh token to obtain new access token and so on, so the 
//user needs to authenticate him self by providing username and password along 
//with client info (we’ll talk about clients later in this post), and if the 
//information provided is valid a response contains a short lived access token 
//is obtained along with long lived refresh token (This is not an access token, 
//it is just identifier to the refresh token). Now once the access token 
//expires we can use the refresh token identifier to try to obtain 
//another short lived access token and so on.


//Why not to issue long lived access tokens from the first place?

//In my own opinion there are three main benefits to use refresh tokens which they are:

//Updating access token content: 
//as you know the access tokens are self contained tokens, they contain all the 
//claims (Information) about the authenticated user once they are generated, 
//now if we issue a long lived token (1 month for example) for a 
//user named “Alex” and enrolled him in role “Users” then this information 
//get contained on the token which the Authorization server generated. 
//If you decided later on (2 days after he obtained the token) to add 
//him to the “Admin” role then there is no way to update this information 
//contained in the token generated, you need to ask him to re-authenticate 
//him self again so the Authorization server add this information to this 
//newly generated access token, and this not feasible on most of the cases. 
//You might not be able to reach users who obtained long lived access tokens. 
//So to overcome this issue we need to issue short lived 
//access tokens (30 minutes for example) and use the refresh token to 
//obtain new access token, once you obtain the new access token, 
//the Authorization Server will be able to add new claim for 
//user “Alex” which assigns him to “Admin” role once the new access 
//token being generated.

//Revoking access from authenticated users: 
//Once the user obtains long lived access token he’ll be able to access 
//the server resources as long as his access token is not expired, there 
//is no standard way to revoke access tokens unless the Authorization Server 
//implements custom logic which forces you to store generated access token 
//in database and do database checks with each request. But with refresh tokens, 
//a system admin can revoke access by simply deleting the refresh token identifier 
//from the database so once the system requests new access token using the 
//deleted refresh token, the Authorization Server will reject this request 
//because the refresh token is no longer available (we’ll come into this with more details).

//No need to store or ask for username and password: 
//Using refresh tokens allows you to ask the user for his username and 
//password only one time once he authenticates for the first time, then 
//Authorization Server can issue very long lived refresh token (1 year for example) 
//and the user will stay logged in all this period unless system admin tries 
//to revoke the refresh token. You can think of this as a way to do 
//offline access to server resources, this can be useful if you are 
//building an API which will be consumed by front end application where 
//it is not feasible to keep asking for username/password frequently.


//Refresh Tokens and Clients

//In order to use refresh tokens we need to bound the refresh token with a Client, 
//a Client means the application the is attempting communicate with the back-end API, 
//so you can think of it as the software which is used to obtain the token. 
//Each Client should have Client Id and Secret, usually we can obtain the 
//Client Id/Secret once we register the application with the back-end API.

//The Client Id is a unique public information which identifies your application 
//among other apps using the same back-end API. The client id can be included in 
//the source code of your application, but the client secret must stay confidential 
//so in case we are building JavaScript apps there is no need to include the secret 
//in the source code because there is no straight way to keep this 
//secret confidential on JavaScript application. In this case we’ll be using the 
//client Id only for identifying which client is requesting the refresh token 
//so it can be bound to this client.

//In our case I’ve identified clients to two types (JavaScript – Nonconfidential) 
//and (Native-Confidential) which means that for confidential clients we can 
//store the client secret in 
//confidential way (valid for desktop apps, mobile apps, server side web apps) 
//so any request coming from this client asking for access token should 
//include the client id and secret.

//Bounding the refresh token to a client is very important, we do not want any 
//refresh token generated from our Authorization Server to be used in another 
//client to obtain access token. Later we’ll see how we will make sure that 
//refresh token is bounded to the same client once it used to generate new access token.

using Authentication.API.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Authentication.API.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private AuthRepository _repo = null;

        public AccountController()
        {
            _repo = new AuthRepository();
        }

        #region Register Method


        //“Register” method you will notice that we’ve configured the endpoint for this 
        //method to be “/api/account/register” so any user wants to register into our 
        //system must issue HTTP POST request to this URI and the pay load for this 
        //request will contain the JSON object as below:
        // JSON OBJECT ->
        //{
        //    "userName": "RahulP",
        //    "password": "SuperPass",
        //    "confirmPassword": "SuperPass"
        //}

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        #endregion

        #region Interface Methods Implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Helpers

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        #endregion
    }
}
