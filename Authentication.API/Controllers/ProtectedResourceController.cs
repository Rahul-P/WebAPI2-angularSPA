using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Authentication.API.Controllers
{
    [RoutePrefix("api/ProtectedResource")]
    public class ProtectedResourceController : ApiController
    {
        [Authorize]
        [Route("")]
        public IHttpActionResult Get()
        {
            //ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

            //var Name = ClaimsPrincipal.Current.Identity.Name;
            //var Name1 = User.Identity.Name;

            //var userName = principal.Claims.Where(c => c.Type == "sub").Single().Value;

            return Ok(ProtectedResource.CreateProtectedResource());
        }
    }

    #region Helpers

    public class ProtectedResource
    {
        public int ProtectedResourceID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCity { get; set; }
        public Boolean IsCurrentUser { get; set; }

        public static List<ProtectedResource> CreateProtectedResource()
        {
            List<ProtectedResource> ProtectedResourceList = new List<ProtectedResource> 
            {
                new ProtectedResource {ProtectedResourceID = 10248, CustomerName = "Rahul Pawar ", CustomerCity = "Melbourne", IsCurrentUser = true },
                new ProtectedResource {ProtectedResourceID = 10249, CustomerName = "George St Piere", CustomerCity = "Montreal", IsCurrentUser = false},
                new ProtectedResource {ProtectedResourceID = 10250,CustomerName = "Jose Aldo", CustomerCity = "Rio", IsCurrentUser = false },
                new ProtectedResource {ProtectedResourceID = 10251,CustomerName = "Anderson Silva", CustomerCity = "Forteleza", IsCurrentUser = false},
                new ProtectedResource {ProtectedResourceID = 10252,CustomerName = "Dan Hardy", CustomerCity = "London", IsCurrentUser = true}
            };

            return ProtectedResourceList;
        }
    }

    #endregion
}
