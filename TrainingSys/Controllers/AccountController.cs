using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Data;

namespace TrainingSys.Controllers
{
    public class AccountController : Controller
    {
        SqlConnection db;

        public AccountController(IConfiguration configuration)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        
        public IActionResult Login()
        {

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Password)
        {

            var result = db.QueryFirstOrDefault("sp_online_login", new { Username = Username, Password = Password }, commandType: CommandType.StoredProcedure);



            if (result is null)

            {
                TempData["error"] = "INVALID USERNAME OR PASSWORD";
                TempData["invalid"] = "is-invalid";
                return RedirectToAction("Login", "Account");

            }

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, result.Name));
            claims.Add(new Claim("def_dept", result.def_dept));
            claims.Add(new Claim("username", Username));







            if (result.HumanResources == 1)
            {
                claims.Add(new Claim("HumanResources", "HumanResources"));
            }

            if (result.Training == 1)
            {
                claims.Add(new Claim("Training", "Training"));
            }

            // Add claims for CBC and CBR if true
            if (result.CBC == 1 && result.CBR == 1)
            {
                claims.Add(new Claim("Company", "%"));
            }

            if (result.CBR == 1 && result.CBC != 1)
            {
                claims.Add(new Claim("Company", "CBR"));
            }
            if (result.CBR != 1 && result.CBC == 1)
            {
                claims.Add(new Claim("Company", "CBC"));
            }
            if (result.isrtphead is true)
            {
                claims.Add(new Claim("isrtphead", "isrtphead"));
            }


            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };




        

                if (result.HumanResources == 1 || result.Training == 1)
                {

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // If either HumanResources or Training is 1, redirect to the "Index" action in the "Home" controller
                return RedirectToAction("Index", "Home");
                }
                else
                {
              
                // If neither condition is met, set a warning message and stay on the login page
                TempData["siteWarning"] = "You do not have access on this site. Please try again later.";
                    return RedirectToAction("Login", "Account"); // Adjust "Account" to your login controller if different
                }
            

        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            return RedirectToAction("Index", "Home");
        }
    }
}
