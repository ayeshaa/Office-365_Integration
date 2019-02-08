using dotnet_tutorial.Models;
using dotnet_tutorial.TokenStorage;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace dotnet_tutorial.Controllers
{
    public class MailController : Controller
    {
        GraphService graphService = new GraphService();
        public async Task<string> GetAccessToken()
        {
            try
            {
                string accessToken = null;

                // Load the app config from web.config
                string appId = ConfigurationManager.AppSettings["ida:AppId"];
                string appPassword = ConfigurationManager.AppSettings["ida:AppPassword"];

                string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
                string[] scopes = ConfigurationManager.AppSettings["ida:AppScopes"]
                    .Replace(' ', ',').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // Get the current user's ID
                string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get the user's token cache
                    SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);

                    ConfidentialClientApplication cca = new ConfidentialClientApplication(
                        appId, redirectUri, new ClientCredential(appPassword), tokenCache.GetMsalCacheInstance(), null);

                    // Call AcquireTokenSilentAsync, which will return the cached
                    // access token if it has not expired. If it has expired, it will
                    // handle using the refresh token to get a new one.
                    AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes, cca.Users.First());

                    accessToken = result.AccessToken;
                }

                return accessToken;
            }
            catch (Exception ex)
            {
                return "";

            }

        }
        
        public async Task<ActionResult> Index()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        // GET: Mail
        public async Task<string> Inbox()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "";
            }

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));

            try
            {
                var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from,body")
                                    .Top(10)
                                    .GetAsync();
                //return Json(mailResults.CurrentPage, JsonRequestBehavior.AllowGet);
                //ViewBag.Folder = "Inbox";
                //return View(mailResults.CurrentPage);
                var output = JsonConvert.SerializeObject(mailResults.CurrentPage);
                return output;
            }
            catch (ServiceException ex)
            {
                return "";
                //return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> Trash()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return View();
            }

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));

            try
            {
                var mailResults = await client.Me.MailFolders.DeletedItems.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from,replyto,torecipients,body")
                                    .Top(10)
                                    .GetAsync();
                //return Json(mailResults.CurrentPage, JsonRequestBehavior.AllowGet);
                var output = JsonConvert.SerializeObject(mailResults.CurrentPage);
                // return output;
                return View(mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return View();
                //return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> Sent()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return View();
            }

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));

            try
            {
                var mailResults = await client.Me.MailFolders.SentItems.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,replyto,torecipients,body")
                                    .Top(10)
                                    .GetAsync();
                //return Json(mailResults.CurrentPage, JsonRequestBehavior.AllowGet);
                var output = JsonConvert.SerializeObject(mailResults.CurrentPage);
                //return output;
                return View(mailResults.CurrentPage);
                //ViewBag.Folder = "Inbox";
                //return View(mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return View();
                //return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> Draft()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return View();
            }

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));

            try
            {
                var mailResults = await client.Me.MailFolders.Drafts.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,replyto,torecipients,body")
                                    .Top(10)
                                    .GetAsync();
                //return Json(mailResults.CurrentPage, JsonRequestBehavior.AllowGet);
               // var output = JsonConvert.SerializeObject(mailResults.CurrentPage);
                //return output;
                //ViewBag.Folder = "Inbox";
                return View(mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return View();
                //return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<string> Detail(string id)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "";
            }

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));

            try
            {
                var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from,body")
                                    .GetAsync();
                //ViewBag.Body = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault().Body.Content;
                //return Json(mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault(), JsonRequestBehavior.AllowGet);
                var output = JsonConvert.SerializeObject(mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault());
                return output;

            }
            catch (ServiceException ex)
            {
                return "";
                //return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }


        [ValidateInput(false)]
        public async Task<string> SendEmail([FromBody]EmailForm data, [Bind(Prefix = "EmailBody")] string EmailBody, string testbody)
        {
            //string name = Request["EmailBody"];
            // Get an access token.
            string accessToken = await GetAccessToken();

            //string accessToken = await GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                //ViewBag.Message = "Access Token  Failed";
                return "Access Token  Failed";
            }

            
            try
            {



                // Build the email message. 
                // The message contains a file attachment and embeds a sharing link to the file in the message body.
                Models.MessageRequest email = await graphService.BuildEmailMessage(accessToken,
                data.EmailTo, data.EmailSubject,data.EmailBody);

                // Send the email.
                var output = await graphService.SendEmail(accessToken, email);

                
                return "Success";
            }
            catch (ServiceException ex)
            {
                return ex.Message;
                //return RedirectToAction("Error", "Home", new { message = "ERROR sending email", debug = ex.Message });
            }
        }

        public async Task<string> ReplyEmail([FromBody]EmailForm data)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "Access Token  Failed";
            }
            
            try
            {
                var temp = await graphService.ReplyEmail(token, data.EmailToId, data.EmailBody);
                return "Success";

            }
            catch (ServiceException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ForwardEmail([FromBody]EmailForm data)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "Access Token  Failed";
            }
            try
            {
                var temp = await graphService.ForwardEmail(token, data.EmailToId, data.EmailTo,data.EmailBody);
                return "Success";
            }
            catch (ServiceException ex)
            {
                return  ex.Message;
            }
        }
        public async Task<ActionResult> Delete(List<string> id)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
               // return "Access token failed";
            }


            try
            {

                foreach (var item in id)
                {
                    var temp = await graphService.DeleteEmail(token, item);
                }
                if (string.IsNullOrEmpty(token))
                {
                    // If there's no token in the session, redirect to Home
                    return Redirect("/");
                }

                GraphServiceClient client = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", token);

                            return Task.FromResult(0);
                        }));

                try
                {
                    var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                        .OrderBy("receivedDateTime DESC")
                                        .Select("subject,receivedDateTime,from,body")
                                        .Top(10)
                                        .GetAsync();

                    ViewBag.Folder = "Inbox";
                    return RedirectToAction("Index", "Home", mailResults.CurrentPage);
                }
                catch (ServiceException ex)
                {
                    return View();
                }
                
                
            }
            catch (ServiceException ex)
            {
                return View();
            }
        }
    }
}