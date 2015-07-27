using Authentication.API.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Data.Entity;
using System.Web.Http;

//  the “assembly” attribute dictates which 
// class to fire on start-up
[assembly: OwinStartup(typeof(Authentication.API.Startup))]
namespace Authentication.API
{
    public class Startup
    {
        //The “Configuration” method accepts parameter of type “IAppBuilder” 
        //this parameter will be supplied by the host at run-time.

        //This “app” parameter is an interface which will be used to 
        //compose the application for our Owin server

        public void Configuration(IAppBuilder app)
        {
            //The “HttpConfiguration” object is used to configure API routes, 
            //so we’ll pass this object to method “Register” 
            //in “WebApiConfig” class.
            HttpConfiguration config = new HttpConfiguration();

            ConfigureOAuth(app);

            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            //Lastly, we’ll pass the “config” object to the extension method “UseWebApi” 
            //which will be responsible to wire up ASP.NET Web API to our Owin server pipeline.
            app.UseWebApi(config);

            Database.SetInitializer(new
                MigrateDatabaseToLatestVersion<AuthContext,
                Authentication.API.Migrations.Configuration>());
        }

        // What is OAuth????
        //OAuth is an open standard for authorization. OAuth provides client applications 
        //a 'secure delegated access' to server resources on behalf of a resource owner. 
        //It specifies a process for resource owners to authorize third-party access to their 
        //server resources without sharing their credentials.
        public void ConfigureOAuth(IAppBuilder app)
        {
            //Here we’ve created new instance from class “OAuthAuthorizationServerOptions” 
            //and set its option as the below:

            //1.
            //The path for generating tokens will be as :”http://localhost:port/token”. 
            //We’ll see how we will issue HTTP POST request to generate token in the next steps.

            //2.
            //We’ve specified the expiry for token to be 24 hours, so if the user tried 
            //to use the same token for authentication after 24 hours from the issue time, 
            //his request will be rejected and HTTP status code 401 is returned.

            //3.
            //We’ve specified the implementation on how to validate the credentials for 
            //users asking for tokens in custom class named “SimpleAuthorizationServerProvider”.

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}