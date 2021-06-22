using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tvl.Web.Controllers
{
    public class ElementsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles ="RappUser")]
        public IActionResult ElementsTab()
        {
            return View();
        }
    }
}
