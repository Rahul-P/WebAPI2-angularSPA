namespace Authentication.API.Migrations
{
    using Authentication.API.Entities;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public class Configuration : DbMigrationsConfiguration<Authentication.API.AuthContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Authentication.API.AuthContext context)
        {
            if (context.Clients.Count() > 0)
            {
                return;
            }

            context.Clients.AddRange(BuildClientsList());
            context.SaveChanges();
        }

        private static List<Client> BuildClientsList()
        {

            List<Client> ClientsList = new List<Client> 
            {
                new Client
                { Id = "ngAuthApp", 
                    Secret= Helper.GetHash("lm@123456"), 
                    Name="AngularJS front-end Application", 
                    ApplicationType =  Models.ApplicationTypeEnums.JavaScript, 
                    Active = true, 
                    RefreshTokenLifeTime = 7200, 
                    AllowedOrigin = "http://logicmonk-SPA.azurewebsites.net"
                },
                new Client
                { Id = "consoleApp", 
                    Secret=Helper.GetHash("123456@lm"), 
                    Name="Console Application", 
                    ApplicationType = Models.ApplicationTypeEnums.NativeConfidential, 
                    Active = true, 
                    RefreshTokenLifeTime = 14400, 
                    AllowedOrigin = "*"
                }
            };

            return ClientsList;
        }
    }
}