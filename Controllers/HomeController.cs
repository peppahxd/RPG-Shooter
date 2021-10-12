using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPG_Shooter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebSocketSharp.Server;

namespace RPG_Shooter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            

        }


        public IActionResult Index()
        {


            return View();
        }

    }
}
