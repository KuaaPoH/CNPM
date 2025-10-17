using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongBaoController : Controller
    {
        private readonly DataContext _context;
        public ThongBaoController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var tblist = _context.ThongBaos.OrderBy(m => m.MaTB).ToList();
            return View(tblist);
        }

        
    }
}