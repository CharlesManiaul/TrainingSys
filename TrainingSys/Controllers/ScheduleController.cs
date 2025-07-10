using Microsoft.AspNetCore.Mvc;

using Microsoft.Data.SqlClient;
using TrainingSys.Models;
using Dapper;
using FluentFTP;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MySqlX.XDevAPI;

namespace TrainingSys.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {

        SqlConnection db;
        MySqlConnection navee;

        public ScheduleController(IConfiguration confg)
        {
            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));

            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));

        }

        public IActionResult Index()
        {
            ScheduleMaster sched = new ScheduleMaster();
            string sql = "";


            sql = @"SELECT a.*, b.Title FROM ScheduleHeader a
                    join TrainingMaster b
                    On a.TId = b.TId
                    ORDER BY TTId desc 
                    ";
            sched.trainHead = db.Query<TrainHead>(sql);




            sql = @"SELECT * FROM TrainingMaster ";
            sched.training = db.Query<TrainMaster>(sql);


            sql = "SELECT  employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            sched.profiles = navee.Query<Profile>(sql);



            return View(sched);
        }



        public IActionResult ScheduleReport()
        {
            ScheduleMaster sched = new ScheduleMaster();
            string sql = "";
            ViewBag.reports = "active";

            sql = @"SELECT  d.Title,a.* FROM ScheduleHeader a
                    join ExamTran b
                    On a.PTID = b.PTID
                    JOIN ProjectDetails c
                    on b.PTId = c.PTId
                    join TrainingMaster d
                    on c.TId = d.TId
                    ORDER BY TTId desc 
                                        ";
            sched.trainHead = db.Query<TrainHead>(sql);


            sql = @"SELECT * FROM TrainingMaster";
            sched.training = db.Query<TrainMaster>(sql);




            return View(sched);
        }


        public IActionResult AddSchedule()
        {

            ScheduleMaster sched = new ScheduleMaster();
            string sql = "";

            sql = "SELECT  employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            sched.scheduleMaster = navee.Query<ScheduleMaster>(sql);



            sql = "SELECT * FROM TrainingMaster where IsCancel = 0 or IsCancel is null";
            sched.training = db.Query<TrainMaster>(sql);




            return View(sched);
        }

        [Route("Schedule/Details/{TTId}")]
        public IActionResult Details(int TTId)
        {

            ScheduleMaster sched = new ScheduleMaster();
            string sql = "";

            sql = "SELECT employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            sched.scheduleMaster = navee.Query<ScheduleMaster>(sql);

            sql = "SELECT * FROM TrainingAttendee where TTId = @TTId";
            sched.AttendeeDetails = db.Query<Attendee>(sql, new { TTId }).ToList();

            sql = @"
                    SELECT a.*, c.IsWritten As IsWritten FROM ScheduleHeader a
                    join TrainingMaster b
                    on a.TId = b.TId
                    join ExamHeader c
                    on b.Id = c.Id 
                    Where a.TTId = @TTId";
            sched.TrainHead = db.QueryFirstOrDefault<TrainHead>(sql, new { TTId });

            sql = @"SELECT * FROM TrainingMaster ";
            sched.training = db.Query<TrainMaster>(sql);

            sql = "SELECT  employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            sched.profiles = navee.Query<Profile>(sql);

            return View(sched);
        }






        [HttpPost]
        public IActionResult SaveSchedule(ScheduleMaster schedule)
        {

            if (schedule.TrainHead.TId == 0)
            {
                // Handle the case where assetMovementDetails is null
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = "Please select Training.";
                return RedirectToAction("Index");
            }



            var response = db.ExecuteScalar("sp_AddSchedule", new
            {
                schedule.TrainHead.TId,
                schedule.TrainHead.Id,
                schedule.TrainHead.SchedDate,
                schedule.TrainHead.Location,
                schedule.TrainHead.Trainor,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)


            });


            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Saving of schedule failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "You have successfully Scheduled a Training.";
            return RedirectToAction("Index");

        }









        [HttpPost]
        public IActionResult FinishSched(ScheduleMaster schedule)
        {

            using (DataTable AttendeeDetails = new DataTable())
            {
                AttendeeDetails.Columns.Add("TTId", typeof(int));
                AttendeeDetails.Columns.Add("PId", typeof(int));
                AttendeeDetails.Columns.Add("EmpId", typeof(string));
                AttendeeDetails.Columns.Add("fullName", typeof(string));
                AttendeeDetails.Columns.Add("position", typeof(string));
                AttendeeDetails.Columns.Add("department", typeof(string));
                AttendeeDetails.Columns.Add("IsAttended", typeof(bool));
                AttendeeDetails.Columns.Add("IsFinished", typeof(bool));

                foreach (var row in schedule.AttendeeDetails)
                {
                    AttendeeDetails.Rows.Add(row.TTId, row.PId, row.EmpId, row.fullName, row.position, row.department, row.IsAttanded, row.IsFinished);
                }


                var response = db.ExecuteScalar("sp_FinishSchedule", new
                {
                    schedule.TrainHead.TTId,
                    AttendeeDetails
                });


                if (response.ToString() != "Successful")
                {
                    TempData["Error Title"] = "Finishing  of schedule failed";
                    TempData["Error Message"] = response?.ToString();
                    return RedirectToAction("Index");
                }

                TempData["Success Title"] = "Save successful";
                TempData["Success Message"] = "You have successfully Scheduled a Training.";
                return RedirectToAction("Index");
            }

        }


        [HttpPost]
        public IActionResult UpdateSchedule(ScheduleMaster schedule)
        {
            if (schedule.AttendeeDetails == null)
            {
                // Handle the case where assetMovementDetails is null
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = "Attendee list details are missing.";
                return RedirectToAction("Index");
            }

            if (schedule.TrainHead.TId == 0)
            {
                // Handle the case where assetMovementDetails is null
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = "Please select Training.";
                return RedirectToAction("Index");
            }

            using (DataTable AttendeeDetails = new DataTable())
            {
                AttendeeDetails.Columns.Add("TTId", typeof(int));
                AttendeeDetails.Columns.Add("TId", typeof(int));
                AttendeeDetails.Columns.Add("EmpId", typeof(string));
                AttendeeDetails.Columns.Add("fullName", typeof(string));
                AttendeeDetails.Columns.Add("position", typeof(string));
                AttendeeDetails.Columns.Add("department", typeof(string));
                AttendeeDetails.Columns.Add("IsAttended", typeof(bool));
                AttendeeDetails.Columns.Add("IsFinished", typeof(bool));

                foreach (var row in schedule.AttendeeDetails)
                {
                    AttendeeDetails.Rows.Add(row.TTId, row.TId, row.EmpId, row.fullName, row.position, row.department, row.IsAttanded, row.IsFinished);
                }




                var response = db.ExecuteScalar("sp_UpdateSchedule", new
                {
                    schedule.TrainHead.TTId,
                    schedule.TrainHead.TId,
                    schedule.TrainHead.SchedDate,
                    schedule.TrainHead.Location,
                    schedule.TrainHead.Trainor,
                    AttendeeDetails

                });


                if (response.ToString() != "Successful")
                {
                    TempData["Error Title"] = "Update of Scheduled Training failed";
                    TempData["Error Message"] = response?.ToString();
                    return RedirectToAction("Index");
                }

                TempData["Success Title"] = "Save successful";
                TempData["Success Message"] = "You have successfully updated the Scheduled Training.";
                return RedirectToAction("Index");
            }
        }











        [HttpPost]
        public IActionResult DeleteSchedule(int TTId)
        {
            var del = db.ExecuteScalar("sp_DeleteSchedule", new { TTId, CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name) }, commandType: System.Data.CommandType.StoredProcedure);

            if (del is null)
            {
                TempData["Success Title"] = "Delete successful";
                TempData["Success Message"] = "Deletion of schedule is successful.";

                return RedirectToAction("Index");
            }



            TempData["Error Title"] = "Delete failed";
            TempData["Error Message"] = "Deletion of schedule Failed.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult StartSchedule(int TTId)
        {
            var del = db.ExecuteScalar("sp_StartSchedule", new { TTId, StartBy = HttpContext.User.FindFirstValue(ClaimTypes.Name) }, commandType: System.Data.CommandType.StoredProcedure);

            if (del is null)
            {
                TempData["Success Title"] = "Start Success";
                TempData["Success Message"] = "Starting of schedule is successful.";

                return RedirectToAction("Index");
            }


            TempData["Error Title"] = "Failed to start schedule";
            TempData["Error Message"] = "Starting of schedule Failed";

            return RedirectToAction("Index");
        }





        public IActionResult Reschedule(ScheduleMaster schedule)
        {

            var response = db.ExecuteScalar("sp_Reschedule", new
            {
                schedule.TrainHead.TTId,
                schedule.TrainHead.SchedDate,
                RescheduledBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)
            });

            if (response is null)
            {
                TempData["Success Title"] = "Rescheduling successful";
                TempData["Success Message"] = "You have Rescheduled a Training.";
                return RedirectToAction("Details", new { schedule.TrainHead.TTId });


            }

            TempData["Error Title"] = "Rescheduling failed";
            TempData["Error Message"] = response?.ToString();
            return RedirectToAction("Details", new { schedule.TrainHead.TTId });

        }





        public IActionResult TestTaker()
        {
            TestTaker test = new TestTaker();
            string sql = "";
            ViewBag.reports = "active";

            sql = @"                 
                   sp_ExamResult_Summary
                    ";
            test.testTaker = db.Query<TestTaker>(sql);

            return View(test);
        }




        [Route("/Schedule/EmpTook/{TTId}/{TId}")]
        public IActionResult EmpTook(int TTId, int TId)
        {
            ShowTaker aynasho = new ShowTaker();
            aynasho.showTaker = db.Query<ShowTaker>("sp_WrittenExamAnswer", new { TId, TTId });

            return View(aynasho);
        }

    }
}
