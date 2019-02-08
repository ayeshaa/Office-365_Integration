// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

using dotnet_tutorial.TokenStorage;
using dotnet_tutorial.Helpers;
using dotnet_tutorial.Models;
using PagedList;
using System.Collections.Generic;

namespace dotnet_tutorial.Controllers
{
    public class HomeController : Controller
    {
        GraphService graphService = new GraphService();
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                string userName = ClaimsPrincipal.Current.FindFirst("name").Value;
                string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userId))
                {
                    // Invalid principal, sign out
                    return RedirectToAction("SignOut");
                }

                // Since we cache tokens in the session, if the server restarts
                // but the browser still has a cached cookie, we may be
                // authenticated but not have a valid token cache. Check for this
                // and force signout.
                SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);
                if (!tokenCache.HasData())
                {
                    // Cache is empty, sign out
                    return RedirectToAction("SignOut");
                }

                ViewBag.UserName = userName;
            }
            return View();
        }

        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                // Signal OWIN to send an authorization request to Azure
                HttpContext.GetOwinContext().Authentication.Challenge(
                  new AuthenticationProperties { RedirectUri = "/" },
                  OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            if (Request.IsAuthenticated)
            {
                string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get the user's token cache and clear it
                    SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);
                    tokenCache.Clear();
                }
            }
            // Send an OpenID Connect sign-out request. 
            HttpContext.GetOwinContext().Authentication.SignOut(
              CookieAuthenticationDefaults.AuthenticationType);
            Response.Redirect("/");
        }

        public async Task<string> GetAccessToken()
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

        public async Task<ActionResult> Inbox(int? page)
        {
            string token = await GetAccessToken();
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

                int pageSize = 10;
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                IPagedList<Microsoft.Graph.Message> emails = null;

                var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,hasAttachments,receivedDateTime,from,body")
                                    .Top(100)
                                    .GetAsync();




                ViewBag.Folder = "Inbox";
                emails = mailResults.CurrentPage.ToPagedList(pageIndex, pageSize);
                return View(mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        public async Task<ActionResult> Draft()
        {
            string token = await GetAccessToken();
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
                var mailResults = await client.Me.MailFolders.Drafts.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from")
                                    .Top(10)
                                    .GetAsync();

                ViewBag.Folder = "Draft";
                return View("Inbox", mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> Sent()
        {
            string token = await GetAccessToken();
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
                var mailResults = await client.Me.MailFolders.SentItems.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from")
                                    .Top(10)
                                    .GetAsync();

                ViewBag.Folder = "Sent Items";
                return View("Inbox", mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        public async Task<ActionResult> Detail(string id)
        {
            string token = await GetAccessToken();
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
                                    .GetAsync();
                var detail = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault();
                ViewBag.Body = detail.Body.Content;
                ViewBag.fromEName = detail.From.EmailAddress.Name;
                ViewBag.fromEmail = detail.From.EmailAddress.Address;
                ViewBag.recievedTime = detail.ReceivedDateTime.Value.ToString("dd-MM-yyyy");
                ViewBag.id = detail.Id;
                return View();
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> SentDetail(string id)
        {
            string token = await GetAccessToken();
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
                var mailResults = await client.Me.MailFolders.SentItems.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from,body")
                                    .GetAsync();
                var detail = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault();
                ViewBag.Body = detail.Body.Content;
                ViewBag.fromEName = detail.From.EmailAddress.Name;
                ViewBag.fromEmail = detail.From.EmailAddress.Address;
                ViewBag.recievedTime = detail.ReceivedDateTime.Value.ToString("dd-MM-yyyy");
                ViewBag.id = detail.Id;
                return View();
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> Reply(string id)
        {
            string token = await GetAccessToken();
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
                                    .GetAsync();
                var detail = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault();
                ViewBag.Body = detail.Body.Content;
                ViewBag.fromEName = detail.From.EmailAddress.Name;
                ViewBag.fromEmail = detail.From.EmailAddress.Address;
                ViewBag.recievedTime = detail.ReceivedDateTime.Value.ToString("dd-MM-yyyy");
                ViewBag.id = detail.Id;
                ViewBag.subject = detail.Subject;

                return View();
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }


        public async Task<ActionResult> ComposeDraft(string id)
        {
            string accessToken = await GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);

                        return Task.FromResult(0);
                    }));

            try
            {
                var mailResults = await client.Me.MailFolders.Drafts.Messages.Request()
                    .OrderBy("receivedDateTime DESC")
                    .Select("subject,receivedDateTime,from,body")
                    .GetAsync();
                var detail = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault();
                ViewBag.Body = detail.Body.Content;

                if (detail.From != null)
                {
                    ViewBag.fromEName = detail.From.EmailAddress.Name;
                    ViewBag.fromEmail = detail.From.EmailAddress.Address;
                }

                ViewBag.recievedTime = detail.ReceivedDateTime.Value.ToString("dd-MM-yyyy");
                ViewBag.id = detail.Id;
                ViewBag.subject = detail.Subject;
                return View();
                //return View("~/Views/Home/Compose.cshtml");
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        public async Task<ActionResult> Delete(string id)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }


            try
            {

                var temp = await graphService.DeleteEmail(token, id);

                GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));
                var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                   .OrderBy("receivedDateTime DESC")
                                   .Select("subject,receivedDateTime,from,body")
                                   .Top(10)
                                   .GetAsync();

                ViewBag.Folder = "Inbox";

                return View("Inbox", mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        public async Task<ActionResult> DeleteSelectedEmails(IEnumerable<string> checkedEmails)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }

            try
            {

                foreach (var emailId in checkedEmails)
                {
                    var temp = await graphService.DeleteEmail(token, emailId);
                }

                //GraphServiceClient client = new GraphServiceClient(
                //new DelegateAuthenticationProvider(
                //    (requestMessage) =>
                //    {
                //        requestMessage.Headers.Authorization =
                //            new AuthenticationHeaderValue("Bearer", token);

                //        return Task.FromResult(0);
                //    }));
                //var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                //                   .OrderBy("receivedDateTime DESC")
                //                   .Select("subject,receivedDateTime,from,body")
                //                   .Top(10)
                //                   .GetAsync();

                //ViewBag.Folder = "Inbox";

                return Json(true);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }



        public async Task<ActionResult> TestReply(string id)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }


            try
            {

                var temp = await graphService.ReplyEmail(token, id, "Test Reply");

                GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));
                var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                   .OrderBy("receivedDateTime DESC")
                                   .Select("subject,receivedDateTime,from,body")
                                   .Top(10)
                                   .GetAsync();

                ViewBag.Folder = "Inbox";
                return View("Inbox", mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        public async Task<ActionResult> ForwardEmail()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }
            string id = Request.Form["id"];
            string recipent = Request.Form["recipients"];

            try
            {

                var temp = await graphService.ForwardEmail(token, id, recipent, "");

                GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));
                var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
                                   .OrderBy("receivedDateTime DESC")
                                   .Select("subject,receivedDateTime,from,body")
                                   .Top(10)
                                   .GetAsync();

                ViewBag.Folder = "Inbox";
                return View("Inbox", mailResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        public async Task<ActionResult> Forward(string id)
        {
            string token = await GetAccessToken();
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
                                    .GetAsync();
                var detail = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault();
                ViewBag.Body = detail.Body.Content;
                ViewBag.fromEName = detail.From.EmailAddress.Name;
                ViewBag.fromEmail = detail.From.EmailAddress.Address;
                ViewBag.recievedTime = detail.ReceivedDateTime.Value.ToString("dd-MM-yyyy");
                ViewBag.id = detail.Id;
                ViewBag.subject = detail.Subject;

                return View();
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
        public async Task<ActionResult> Calendar()
        {
            string token = await GetAccessToken();
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
                var eventResults = await client.Me.Events.Request()
                                    .OrderBy("start/dateTime DESC")
                                    .Select("subject,start,end")
                                    .Top(10)
                                    .GetAsync();

                return View(eventResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving events", debug = ex.Message });
            }
        }

        public async Task<ActionResult> Contacts()
        {
            string token = await GetAccessToken();
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
                var contactResults = await client.Me.Contacts.Request()
                                    .OrderBy("displayName")
                                    .Select("displayName,emailAddresses,mobilePhone")
                                    .Top(10)
                                    .GetAsync();

                return View(contactResults.CurrentPage);
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving contacts", debug = ex.Message });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public async Task<ActionResult> Compose()
        {
            string accessToken = await GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }


            ViewBag.Email = await graphService.GetMyEmailAddress(accessToken);

            return View();
        }

        public async Task<ActionResult> SendEmail()
        {
            // Get an access token.
            string accessToken = await GetAccessToken();

            //string accessToken = await GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                ViewBag.Message = "Access Token  Failed";
                return View("Compose");
            }

            if (string.IsNullOrEmpty(Request.Form["email-address"]))
            {
                ViewBag.Message = "Please enter the email";
                return View("Compose");
            }

            try
            {



                // Build the email message. 
                // The message contains a file attachment and embeds a sharing link to the file in the message body.
                Models.MessageRequest email = await graphService.BuildEmailMessage(accessToken,
                Request.Form["recipients"], Request.Form["subject"], Request.Form["body"]);

                // Send the email.
                ViewBag.Message = await graphService.SendEmail(accessToken, email);

                // Reset the current user's email address and the status to display when the page reloads.
                ViewBag.Email = Request.Form["email-address"];
                return View("Compose");
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR sending email", debug = ex.Message });
            }
        }
        public ActionResult Error(string message, string debug)
        {
            ViewBag.Message = message;
            ViewBag.Debug = debug;
            return View("Error");
        }

        public async Task<ActionResult> TrashDetail(string id)
        {
            string token = await GetAccessToken();
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
                var mailResults = await client.Me.MailFolders.DeletedItems.Messages.Request()
                                    .OrderBy("receivedDateTime DESC")
                                    .Select("subject,receivedDateTime,from,body")
                                    .GetAsync();
                var detail = mailResults.CurrentPage.Where(e => e.Id == id).FirstOrDefault();
                ViewBag.Body = detail.Body.Content;
                ViewBag.fromEName = detail.From.EmailAddress.Name;
                ViewBag.fromEmail = detail.From.EmailAddress.Address;
                ViewBag.recievedTime = detail.ReceivedDateTime.Value.ToString("dd-MM-yyyy");
                ViewBag.id = detail.Id;
                return View();
            }
            catch (ServiceException ex)
            {
                return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }
    }
}