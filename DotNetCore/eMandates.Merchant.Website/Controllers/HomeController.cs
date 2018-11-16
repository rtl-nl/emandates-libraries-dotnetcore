using Microsoft.AspNetCore.Mvc;

namespace eMandates.Merchant.Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}