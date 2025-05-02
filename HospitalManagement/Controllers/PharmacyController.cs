using BELibrary.Core.Entity;
using BELibrary.DbContext;
using System.Linq;
using System.Web.Mvc;

namespace HospitalManagement.Controllers
{
    public class PharmacyController : Controller
    {
        // GET: Pharmacy
        public ActionResult Index()
        {
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var medicines = workScope.Medicines.GetAll().ToList();
                return View(medicines);
            }
        }
    }
}