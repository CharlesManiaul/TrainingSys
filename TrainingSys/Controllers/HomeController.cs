using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrainingSys.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace TrainingSys.Controllers
{
    [Authorize]


    public class HomeController : Controller
    {
        SqlConnection db;
        MySqlConnection navee;
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, IConfiguration confg)
        {
            _logger = logger;

            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));

            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));
        }

        public IActionResult Index()
        {
            ScheduleMaster sched = new ScheduleMaster();
            string sql = "";
            ViewBag.Home = "active";

            sql = @"
                    
                    SELECT a.TAId, f.ProjectName, a.EmpId, b.FullName, c.Location,c.SchedDate,c.Trainor,c.crtdBy,c.crtdDate 
                    FROM TrainingAttendee a
                    JOIN EmployeeMaster b
                    on a.EmpId = b.EmpId
                    join ScheduleHeader c
                    on c.PTID = a.PTId
                    Join ExamTran d
                    on a.PTId = d.PTId
                    JOIN ProjectDetails e
                    on d.PTId = e.PTId
                    Join ProjectMaster f
                    on e.PMId = f.PMId
                    ORDER BY a.TAId asc
                    ";
            sched.trainHead = db.Query<TrainHead>(sql);


            return View(sched);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Handle general errors
        [Route("Home/Error")]
        public IActionResult Error()
        {
            var statusCode = HttpContext.Response.StatusCode;
            return View("Error", statusCode);
        }

        // Handle status codes
        [Route("Home/StatusCode")]
        public IActionResult StatusCode(int code)
        {
            // Customize the view name based on the status code
            return code switch
            {
                404 => View("NotFound"),
                500 => View("ServerError"),
                400 => View("BadRequest"),
                _ => View("Error", code)
            };
        }

    }
}
