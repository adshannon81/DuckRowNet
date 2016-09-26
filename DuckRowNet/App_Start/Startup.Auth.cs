using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using DuckRowNet.Models;
using Microsoft.Owin.Security.Facebook;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Twitter;
using Microsoft.Owin.Security;

namespace DuckRowNet
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");
            //app.UseTwitterAuthentication(new TwitterAuthenticationOptions
            //{
            //    ConsumerKey = "0YJol0l2IGr5KacFgJBdJtCSf",
            //    ConsumerSecret = "bA32alxGfth2oUy3DvKfhyY90lkTiIUthbxynhHxwPP1qbTB1E",
            //    BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(new[]
            //        {
            //            "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
            //            "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
            //            "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
            //            "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
            //            "5168FF90AF0207753CCCD9656462A212B859723B", //DigiCert SHA2 High Assurance Server C‎A 
            //            "B13EC36903F8BF4701D498261A0802EF63642BC3" //DigiCert High Assurance EV Root CA
            //        })
            //});

            //app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            //{
            //    AppId = "503903393068027",
            //    AppSecret = "cbface107ffc26e636bb5c6f2908d5bd",
            //    Scope = { "email" },
            //    Provider = new FacebookAuthenticationProvider
            //    {
            //        OnAuthenticated = context =>
            //        {
            //            context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookAccessToken", context.AccessToken));
            //            return Task.FromResult(true);
            //        }
            //    }
            //});


            // live FB
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AppId = "758922257578494",
                AppSecret = "d505ce1cab13d1b96649d120e41dffd9",
                Scope = { "email" },
                Provider = new FacebookAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookAccessToken", context.AccessToken));
                        return Task.FromResult(true);
                    }
                }
            });

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "366583201638-9rp1tdkcnq1hih6gjafk790gck4394kf.apps.googleusercontent.com",
            //    ClientSecret = "0vWZh4t3APfSPZvCaBPDysbK"
            //});

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "210379947337-40qlb958th14qubrq0qa3fqo8h1alrnt.apps.googleusercontent.com",
                ClientSecret = "2H32czRf95P3_NQ6kz34Lfu2"
            });
        }
    }
}