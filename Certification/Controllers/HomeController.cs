using Certification.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.DataProtection;
//using Microsoft.Extensions.Configuration;

namespace Certification.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataProtector _provider;
        private readonly TodoContext _todoContext;
        

        public HomeController(ILogger<HomeController> logger, 
            TodoContext todoContext,
            IDataProtectionProvider provider)
        {
            _todoContext = todoContext;
            _logger = logger;
            _provider = (IDataProtector)provider;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            Login login = _todoContext.Login.SingleOrDefault(l => l.Username == username);
            if (login != null)
            {
                if (BC.Verify(password, login.Password))
                {
                    //Er logget ind
                    HttpContext.Session.SetInt32("userId", login.Id);
                    return Redirect("/home/todolist");
                }
                else ViewBag.Message = "Password er forkert"; //Password er forkert
            }
            else ViewBag.Message = "Brugernavn er forkert"; // ingen bruger


            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            Login knownLogin = _todoContext.Login.SingleOrDefault(l => l.Username == username);
            if (knownLogin == null)
            {
                Login login = new Login
                {
                    Username = username,
                    Email = email,
                    Password = BC.HashPassword(password)
                };

                _todoContext.Login.Add(login);
                _todoContext.SaveChanges();
                ViewBag.Message = "Bruger er nu oprettet";
            }
            else ViewBag.Message = "Bruger findes allerede";



            return View();
        }


        [HttpGet]
        public IActionResult todoList()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }

            ViewBag.userId = userId;
            List<TodoItem> todos = _todoContext.TodoItem.Where(t => t.loginId == userId).ToList();

            foreach(TodoItem item in todos)
            {
                item.Title = _provider.Unprotect(item.Title);
                item.Description = _provider.Unprotect(item.Description);
            }
            ViewBag.Todos = todos;
            
            return View();
        }
        
        [HttpPost]
        public IActionResult ToDoList(string itemTitle, string itemDescription)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if(userId == null)
            {
                Redirect("/");
            }

            _todoContext.TodoItem.Add(new TodoItem {

                Title = _provider.Protect(itemTitle),
                Description = _provider.Protect(itemDescription),
                Added = DateTime.Now,
                loginId = (int)userId
            });
            _todoContext.SaveChanges();
            return Redirect("/Home/TodoList");
        }


        [HttpGet]
        public IActionResult DeleteTodo(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                Redirect("/");
            }

            var todoItem = _todoContext.TodoItem.SingleOrDefault(t => t.Id == id && t.loginId == userId);

            _todoContext.TodoItem.Remove(todoItem);
            _todoContext.SaveChanges();
            return Redirect("/Home/TodoList");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
