
namespace Authentication.API.Controllers
{
    using Authentication.API.Models;
    using System.Net.Mail;
    using System.Text;
    using System.Web.Http;

    [AllowAnonymous]
    [RoutePrefix("api/Email")]
    public class EmailController : ApiController
    {
        [HttpPostAttribute]
        [Route("")]
        public IHttpActionResult Post(EmailModel email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("rahul.logicmonk@gmail.com", "@ cOOL sUMMER 09 @");

            MailMessage mm = new MailMessage("donotreply@domain.com", "rahulpawar@y7mail.com", 
                email.Subject,
                "This email is from: " + email.From + " \r\n\r\n Message/Enquiry: \r\n" + email.Body);

            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);           

            return Ok();
        }
    }
}
