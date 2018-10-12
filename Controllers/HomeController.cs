using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private YourContext dbContext;
        public HomeController(YourContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("RegisterInfo")]
        public IActionResult RegisterInfo (User user, int id)
        {
            User ReturnedUser = dbContext.Users.FirstOrDefault(u => u.idUsers == id);

            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                User newUser = new User {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Password = user.Password
                };
                dbContext.Add(newUser);
                dbContext.SaveChanges();

                HttpContext.Session.SetInt32("id", newUser.idUsers);
                HttpContext.Session.SetString("Name", newUser.FirstName);
                int? registered_id = HttpContext.Session.GetInt32("id");

                System.Console.WriteLine(registered_id);

                return Redirect($"Account/{registered_id}");
            }
            else 
            {
                return View("Index");
            }
        }

        [HttpGet]
        [Route("login")]
        public IActionResult login()
        {
            return View("Login");
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("loginuser")]
        public IActionResult loginuser(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if (userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email");
                    return View("login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Invalid Password");
                    return View("login");
                }

            HttpContext.Session.SetInt32("id", userInDb.idUsers);
            HttpContext.Session.SetString("Name", userInDb.FirstName);
            int? id = HttpContext.Session.GetInt32("id");

            System.Console.WriteLine("3843098408230");
            return Redirect($"Account/{id}");
            }
            else 
            {
                return View("login");
            }
        }

        [HttpGet]
        [Route("Account/{id}")]
        public IActionResult Account(int id)
        {
            if (HttpContext.Session.GetInt32("id") == null)
            {
                return RedirectToAction("login");
            }
    
            User user = dbContext.Users.Where(u => u.idUsers == id).FirstOrDefault();

            int logged_in_id = (int)HttpContext.Session.GetInt32("id");
            List<Transaction> allTransaction = dbContext.Transactions.Include(t => t.creator).Where(t => t.creator.idUsers == id).ToList();
            allTransaction.Reverse();

            @ViewBag.Acct = allTransaction;
    
            decimal sum = 0;
            foreach(Transaction transaction in allTransaction)
            {
                sum += transaction.Amount;
            }
            @ViewBag.Balance = sum; 

            User ReturnedUser = dbContext.Users.FirstOrDefault(userx => userx.idUsers == id);
            Transaction bob = new Transaction();
            return View("Account", bob);
        }

        [HttpPost]
        [Route("ProcessAccount")]
        public IActionResult ProcessAccount(Transaction test)
        {   
            test.Users_idUsers = (int)HttpContext.Session.GetInt32("id");

            User trans = dbContext.Users.Include(user => user.transactions).SingleOrDefault(u => u.idUsers == (int)HttpContext.Session.GetInt32("id"));
            
            decimal sum = 0;
            foreach(var t in trans.transactions)
            {
                sum+=t.Amount;
            }
            System.Console.WriteLine("**************************");
            System.Console.WriteLine(sum);
            System.Console.WriteLine(test.Amount);

            if(ModelState.IsValid)
            {        
    
                if (test.Amount + sum >= 0)
                {
                    dbContext.Add(test);
                    dbContext.SaveChanges();  

                }
                else
                {
                    TempData["ErrorMsg"] = "You cannot withdraw more than your balance";
                }
                    return Redirect($"Account/{test.Users_idUsers}");
            }
            else
            {
                return View("Account", test);
            }

        }
    }
}
