using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TrainingSys.Models;
using Dapper;
using FluentFTP;
using MySql.Data.MySqlClient;
using SixLabors.ImageSharp.Formats;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;



namespace TrainingSys.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {

        SqlConnection db;
        MySqlConnection navee;

        public TrainingController(IConfiguration confg)
        {
            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));
            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));
        }

        //for Adding Training and editing
        public IActionResult Index()
        {
            TrainMaster train = new TrainMaster();
            ViewBag.Training = "active";
            string sql = "SELECT * FROM TrainingMaster where IsCancel = 0 or IsCancel is null";
            train.trainMaster = db.Query<TrainMaster>(sql);

            sql = @"SELECT * FROM ExamHeader WHERE Id in (SELECT MAX(Id)FROM ExamHeader where status= 'Finalized' GROUP BY Title ) ORDER BY Title";
            train.exams = db.Query<Exam>(sql).ToList();


            return View(train);
        }


        [HttpPost]
        public IActionResult AddTraining(TrainMaster trainMaster)
        {


            var response = db.ExecuteScalar("sp_AddTraining", new
            {
         
                trainMaster.Title,
                trainMaster.Description,
                trainMaster.Validity,
                trainMaster.Amount,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)

            },
            commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = response?.ToString();

                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Training has been saved successfully.";
            return RedirectToAction("Index");
        }


        [Route("Training/Details/{TId}/{Id}")]
        [HttpGet]
        public IActionResult Details(int TId, int Id)
        {
            TrainMaster train = new TrainMaster();
            string sql = "";

            sql = "SELECT * FROM TrainingMaster where TId = @TId";
            train = db.QueryFirstOrDefault<TrainMaster>(sql, new { TId });

            sql = "SELECT * FROM ExamDetails where Id = @Id and (IsCancel is null or IsCancel = 0 ) ORDER BY ItemNo ASC";
            train.examDetails = db.Query<ExamDetails>(sql, new { Id });

            sql = @"SELECT * FROM ExamHeader WHERE Id in (SELECT MAX(Id)FROM ExamHeader where status= 'Finalized' GROUP BY Title ) ORDER BY Title";
            train.exams = db.Query<Exam>(sql, new { TId }).ToList();


            sql = @"SELECT * FROM TrainingMaster a 
                    join ExamHeader b  
                    on a.Id = b.Id 
                    where a.TId = @TId and (a.IsCancel is null or a.IsCancel = 0)";
            train.TrainingDetails = db.Query<TrainingDetails>(sql, new { TId }).ToList();


            return View(train);
        }


        [HttpPost]

        public IActionResult UpdateTraining(TrainMaster trainMaster)
        {


            var response = db.ExecuteScalar("sp_UpdateTraining", new
            {
                trainMaster.TId,
                trainMaster.Title,
                trainMaster.Description,
                trainMaster.Validity,
                trainMaster.Amount,
                UpdtBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)

            },
            commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Update failed";
                TempData["Error Message"] = response?.ToString();

                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Update successful";
            TempData["Success Message"] = "Training has been updated successfully.";
            return RedirectToAction("Index");
        }






        public IActionResult DeleteTraining(int TId)
        {
            var del = db.ExecuteScalar("sp_DeleteTraining", new { TId, CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name) }, commandType: System.Data.CommandType.StoredProcedure);

            if (del is null)
            {
                TempData["Success Title"] = "Delete successful";
                TempData["Success Message"] = "Deletion of Training is successful.";

                return RedirectToAction("Index");
            }



            TempData["Error Title"] = "Delete failed";
            TempData["Error Message"] = "Deletion of Training Failed.";
            return RedirectToAction("Index");
        }






        [HttpPost]
        public IActionResult SaveExam(TrainMaster train, List<int> exams)
        {
            if (exams == null || !exams.Any())
            {
                TempData["Error Title"] = "No exams selected";
                TempData["Error Message"] = "Please select at least one exam.";
                return RedirectToAction("Index");
            }


            DataTable ExamList = new DataTable();
            ExamList.Columns.Add("Id", typeof(int));


            foreach (var exam in exams)
            {
                ExamList.Rows.Add(exam);
            }




            var response = db.ExecuteScalar("sp_AddTrainingDetails", new
            {
                train.TId,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
                ExamList
            }, commandType: System.Data.CommandType.StoredProcedure);

            // Check the response and handle accordingly
            if (response == null || response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Add of Exam failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Add of Exam successful";
            TempData["Success Message"] = "Examination has been added successfully.";
            return RedirectToAction("Index");

        }





        [HttpPost]
        public IActionResult DeleteExam(TrainMaster train)
        {
            var del = db.ExecuteScalar("sp_EditTrainingDetails", new { train.ExamDetails.Id, CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name) }, commandType: System.Data.CommandType.StoredProcedure);

            if (del.ToString() != "Successful")
            {
                TempData["Error Title"] = "Delete failed";
                TempData["Error Message"] = "Deletion of Examination Failed.";
                return RedirectToAction("Details", new { train.ExamDetails.Id });
            }


            TempData["Success Title"] = "Delete successful";
            TempData["Success Message"] = "Deletion of Examination is successful.";
            return RedirectToAction("Details", new { train.ExamDetails.Id });


        }



        public IActionResult TrainingReport()
        {
            TrainMaster train = new TrainMaster();
            ViewBag.reports = "active";
            string sql = "SELECT * FROM TrainingMaster where IsCancel = 0 or IsCancel is null";
            train.trainMaster = db.Query<TrainMaster>(sql);


            return View(train);
        }






    }


}

