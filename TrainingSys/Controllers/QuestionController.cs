using Dapper;
using FluentFTP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using TrainingSys.Models;

namespace TrainingSys.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {

        SqlConnection db;
        MySqlConnection navee;

        public QuestionController(IConfiguration confg)
        {
            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));
            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));
        }
        public IActionResult Index()
        {
            Exam exam = new Exam();
            ViewBag.Training = "active";
            exam.exam = db.Query<Exam>("sp_ExamIndex", commandType: System.Data.CommandType.StoredProcedure);
            return View(exam);
        }


        public IActionResult ReportExam()
        {
            Exam exam = new Exam();
            ViewBag.reports = "active";
            string sql = "SELECT * FROM ExamHeader ORDER BY id Desc";
            exam.exam = db.Query<Exam>(sql);
            return View(exam);
        }

        [AllowAnonymous]
        //Eval
        [Route("/Question/Evaluation/{Id}")]
        public IActionResult Evaluation(int Id)
        {
            Evaluation eval = new Evaluation();

            string sql = "SELECT Id as EId, * FROM EvaluationForm ORDER BY Part, EvalNumber";
            eval.EvaluationDetails = db.Query<EvaluationDetails>(sql).ToList();
      
            return View(eval);
        }


        [AllowAnonymous]
        public IActionResult SaveEvaluation(Evaluation evaluation)
        {


            DataTable EvaluationAnswer = new DataTable();
            EvaluationAnswer.Columns.Add("Id", typeof(int));
            EvaluationAnswer.Columns.Add("HId", typeof(int));
            EvaluationAnswer.Columns.Add("EId", typeof(int));
            EvaluationAnswer.Columns.Add("Answer", typeof(string));
            EvaluationAnswer.Columns.Add("Comment", typeof(string));


             
            foreach (var question in evaluation.EvaluationDetails)
            {
                EvaluationAnswer.Rows.Add(question.Id, question.HId, question.EId, question.strAnswer, question.strComment);
            }


            var response = db.ExecuteScalar("sp_SubmitEvaluation", new
            {
                evaluation.Id,
                evaluation.EmpId,
                evaluation.TraineeName,
                evaluation.DepartmentStore,
                evaluation.Position,
                evaluation.Trainor,
                evaluation.Venue,
                evaluation.EvaluationDate,
                EvaluationAnswer

            });
            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Examination submit failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Index", "Question");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Examination has been submitted successfully.";
            return RedirectToAction("Thankyou", "Question");

        }


        //end eval

        [AllowAnonymous]
        [Route("/Question/WrittenTest/{TEId}/{Id}")]
        public IActionResult WrittenTest(int Id, int TEId)
        {
            string sql = "";

            Exam exam = new Exam();

            //sql = @"SELECT a.*, b.Status FROM ExamTran a
            //    JOIN ScheduleHeader b
            //    ON a.PTId = b.PTId
            //    WHERE a.TEId = @TEId";
            //exam.trainHead = db.QueryFirstOrDefault<TrainHead>(sql, new { TEId });

            //if (exam.trainHead == null || exam.trainHead.Status != "Finished")
            //{
            //    return RedirectToAction("Sorry", "Question");
            //}


            sql = $"SELECT PassingScore FROM ExamHeader WHERE Id = @Id ";
            exam.PassingScore = db.QueryFirstOrDefault<int>(sql, new { Id }).ToString();


            // sql = "SELECT * FROM TrainingMaster where TId = @TId AND (IsCancel = 0 or IsCancel is null)";
            //exam.trainMaster = db.Query<TrainMaster>(sql, new { TId }).ToList();

           




            sql = @"  SELECT a.*, b.Title, b.PassingScore, c.* FROM TrainingMaster a 
                      JOIN ProjectDetails d
                      on a.TId = d.TId
                      join ExamTran e
                      on e.PTId = d.PTId
                      join ExamHeader b
                      on e.ExamId = b.Id 
                      join ExamDetails c 
                      on b.Id = c.Id 
                      where c.Id = @Id";
            exam.TrainingDetails = db.Query<TrainingDetails>(sql, new { Id }).ToList();

            sql = @"SELECT * FROM ExamDetails where Id = @Id";
            exam.examDetails = db.Query<ExamDetails>(sql, new { Id }).ToList();



            return View(exam);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SubmitAnswers(Exam exam, Dictionary<string, string> answers)
        {
            if (string.IsNullOrWhiteSpace(exam.fullName))
            {
                TempData["Error Title"] = "Examination submit failed";
                TempData["Error Message"] = "You are not scheduled for this Exam.";
                return RedirectToAction("Login", "Account");
            }

            var sql = @"SELECT a.*, b.Title, b.PassingScore, c.* 
                FROM TrainingMaster a
                JOIN ProjectDetails d ON a.TId = d.TId
                JOIN ExamTran e ON e.PTId = d.PTId
                JOIN ExamHeader b ON e.ExamId = b.Id
                JOIN ExamDetails c ON b.Id = c.Id
                WHERE c.Id = @Id and (c.IsCancel = 0 or c.IsCancel is null)";

            var questions = db.Query<TrainingDetails>(sql, new { exam.Id }).ToList();

            DataTable examAnswer = new DataTable();
            examAnswer.Columns.Add("Id", typeof(int));
            examAnswer.Columns.Add("ExamId", typeof(int));
            examAnswer.Columns.Add("ItemNo", typeof(int));
            examAnswer.Columns.Add("Answer", typeof(string));

            foreach (var question in questions)
            {
                var userAnswer = answers.ContainsKey($"answers_{question.ItemNo}") ? answers[$"answers_{question.ItemNo}"] : null;

                if (!string.IsNullOrWhiteSpace(userAnswer))
                {
                    examAnswer.Rows.Add(exam.Id, question.ExamId, question.ItemNo, userAnswer);
                }
            }

            int correctAnswers = examAnswer.AsEnumerable().Count(row =>
                questions.Any(q => q.ItemNo == (int)row["ItemNo"] && (string)row["Answer"] == q.Answer));
            double score = questions.Count > 0 ? (double)correctAnswers / questions.Count * 100 : 0;

            var parameters = new
            {
                exam.Id,
                exam.TEId,
                exam.EmpId,
                exam.fullName,
                Score = score,
                exam.PassingScore,
                ExamAnswer = examAnswer
            };

            var response = db.ExecuteScalar("sp_SubmitAnswer", parameters, commandType: CommandType.StoredProcedure);

            if (response is null || response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Examination submit failed";
                TempData["Error Message"] = response?.ToString() ?? "An error occurred during submission.";
                return RedirectToAction("Sorry", "Question");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Examination has been submitted successfully.";
            return RedirectToAction("Thankyou", "Question");
        }


        [AllowAnonymous]
        [Route("/Question/PracticalTest/{TEId}/{Id}/{EmpId}")]
        public IActionResult PracticalTest(int Id, int TEId, int EmpId)
        {
            string sql = "";
            Exam exam = new Exam();

            // Retrieve Training and Exam details
            sql = @"  SELECT a.*, b.Title, b.PassingScore, c.* 
              FROM TrainingMaster a 
              JOIN ProjectDetails d ON a.TId = d.TId
              JOIN ExamTran e ON e.PTId = d.PTId
              JOIN ExamHeader b ON e.ExamId = b.Id 
              JOIN ExamPracDetails c ON b.Id = c.Id 
              WHERE c.Id = @Id";
            exam.TrainingDetails = db.Query<TrainingDetails>(sql, new { Id }).ToList();

            sql = @"SELECT * FROM ExamPracDetails WHERE Id = @Id";
            exam.examDetails = db.Query<ExamDetails>(sql, new { Id }).ToList();

            sql = $"SELECT PassingScore FROM ExamHeader WHERE Id = @Id ";
            exam.PassingScore = db.QueryFirstOrDefault<int>(sql, new { Id }).ToString();

            // Retrieve FullName using similar logic as in the GetName method
            sql = @"
            SELECT FullName 
            FROM TrainingAttendee a
            JOIN EmployeeMaster b ON a.EmpId = b.EmpId
            JOIN ProjectDetails c ON a.PTId = c.PTId
            JOIN TrainingMaster d ON c.TId = d.TId
            JOIN ExamTran e ON c.PTId = e.PTId
            WHERE a.EmpId = @EmpId 
            AND c.TId IN (SELECT TId 
                          FROM ExamTran a
                          JOIN ProjectDetails b ON a.PTId = b.PTId
                          WHERE ExamId = @Id)";
            var fullName = db.QueryFirstOrDefault<string>(sql, new { EmpId, Id });

            // Set employee's full name and ID
            exam.EmpId = EmpId.ToString();
            exam.fullName = fullName; // Assuming Exam model has a FullName property

            return View(exam);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SubmitPractical(Exam exam)
        {
            if (exam.fullName == null)
            {
                TempData["Error Title"] = "Examination submit failed";
                TempData["Error Message"] = "You are not scheduled for this Exam";
                return RedirectToAction("Login", "Account");
            }

            // Retrieve questions from the database
            var sql = @"SELECT a.*, b.Title, b.PassingScore, c.* FROM TrainingMaster a 
                JOIN ProjectDetails d ON a.TId = d.TId
                JOIN ExamTran e ON e.PTId = d.PTId
                JOIN ExamHeader b ON e.ExamId = b.Id 
                JOIN ExamPracDetails c ON b.Id = c.Id 
                WHERE c.Id = @Id";

            var questions = db.Query<TrainingDetails>(sql, new { exam.Id }).ToList();

            // Create a DataTable to store the answers
            DataTable examAnswer = new DataTable();
            examAnswer.Columns.Add("Id", typeof(int));
            examAnswer.Columns.Add("PracId", typeof(int));
            examAnswer.Columns.Add("ItemNo", typeof(int));
            examAnswer.Columns.Add("Answer", typeof(string));
            examAnswer.Columns.Add("Grade", typeof(int));

            // Initialize the total score and count to calculate the average
            double totalScore = 0;
            int totalCount = 0;

            // Loop through the submitted form data to capture answers
            foreach (var question in questions)
            {
                string answerKey = $"answers_{question.Id}{question.ItemNo}";
                string gradeKey = $"grades_{question.Id}{question.ItemNo}";

                var userAnswer = Request.Form[answerKey];
                var userGrade = Request.Form[gradeKey];

                if (!string.IsNullOrEmpty(userAnswer))
                {
                    // Add the answer and grade to the DataTable
                    int? grade = string.IsNullOrEmpty(userGrade) ? (int?)null : int.Parse(userGrade);

                    // Add the answer to the DataTable
                    examAnswer.Rows.Add(exam.Id, question.PracId, question.ItemNo, userAnswer, grade ?? 0);

                    // If a valid grade is present, add it to the total for averaging
                    if (grade.HasValue)
                    {
                        totalScore += grade.Value;
                        totalCount++;
                    }
                }
            }

            // Compute the overall average score if there are grades
            double averageScore = totalCount > 0 ? Math.Round(totalScore / totalCount, 2) : 0;

            // Prepare parameters for the stored procedure
            var parameters = new
            {
                exam.Id,
                exam.TEId,
                exam.EmpId,
                Score = averageScore,
                exam.fullName,
                exam.PassingScore,
                ExamAnswer = examAnswer // Pass the DataTable to the stored procedure
            };

            // Save using a stored procedure
            var response = db.ExecuteScalar("sp_SubmitPracAnswer", parameters, commandType: CommandType.StoredProcedure);
            if (response == null || response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Examination submit failed";
                TempData["Error Message"] = response?.ToString() ?? "Unknown error occurred";
                return RedirectToAction("Index", "Question");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Examination has been submitted successfully.";
            return RedirectToAction("Thankyou", "Question");
        }

        [AllowAnonymous]
        public IActionResult Thankyou()
        {
            return View();
        }


        [AllowAnonymous]
        public IActionResult Sorry()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int Id)
        {
            ViewBag.Training = "active";
            Exam exam = new Exam();
            string sql = "";

            sql = "SELECT * FROM ExamHeader where Id = @Id";
            exam = db.QueryFirstOrDefault<Exam>(sql, new { Id });

       

            if (exam.IsWritten == true)
            {
                sql = @"SELECT * FROM ExamDetails a
                        join ExamSubType b
                        on a.SubId = b.SubId
                        where Id = @Id and (IsCancel is null or IsCancel = 0 ) 
                        ORDER BY ItemNo ASC";
                exam.examDetails = db.Query<ExamDetails>(sql, new { Id }).ToList();
            }
            else
            {
                sql = @"SELECT * FROM ExamPracDetails where Id = @Id and (IsCancel is null or IsCancel = 0 ) ORDER BY ItemNo ASC";
                exam.examDetails = db.Query<ExamDetails>(sql, new { Id }).ToList();
            }


            sql = "SELECT * FROM ExamSubType";
            exam.SubTypes = db.Query<SubType>(sql).ToList();


            return View(exam);
        }


        [HttpPost]
        public IActionResult AddExam(Exam exam)
        {


            var response = db.ExecuteScalar("sp_AddExam", new
            {
                exam.Title,
                exam.PassingScore,
                exam.IsWritten,
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


        [HttpPost]
        public IActionResult EditExam(Exam exam)
        {


            var response = db.ExecuteScalar("sp_EditExam", new
            {
                exam.Id,
                exam.Title,
                exam.PassingScore,
                exam.IsWritten,
                UpdtBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)

            },
            commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Update failed";
                TempData["Error Message"] = response?.ToString();

                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Training has been Updated successfully.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult DeleteExam(int Id)
        {
            var del = db.ExecuteScalar("sp_DeleteExam", new { Id, CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name) }, commandType: System.Data.CommandType.StoredProcedure);

            if (del is null)
            {
                TempData["Success Title"] = "Delete successful";
                TempData["Success Message"] = "Deletion of Exam is successful.";

                return RedirectToAction("Index");
            }



            TempData["Error Title"] = "Delete failed";
            TempData["Error Message"] = "Deletion of Exam Failed.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult SaveQuestion(Exam exam)
        {
            // If ChoiceAnswer is not null, assign corresponding value to Answer
            if (exam.ExamDetails.ChoiceAnswer != null)
            {
                if (exam.ExamDetails.ChoiceAnswer == "A")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceA;
                else if (exam.ExamDetails.ChoiceAnswer == "B")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceB;
                else if (exam.ExamDetails.ChoiceAnswer == "C")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceC;
                else if (exam.ExamDetails.ChoiceAnswer == "D")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceD;
            }

            // If TrueFalseAnswer is not null, assign corresponding value to Answer
            if (exam.ExamDetails.TrueFalseAnswer != null)
            {
                if (exam.ExamDetails.TrueFalseAnswer == "tama")
                    exam.ExamDetails.Answer = "True";
                else if (exam.ExamDetails.TrueFalseAnswer == "mali")
                    exam.ExamDetails.Answer = "False";
            }

            // If EnumarationAnswer is not null, assign it directly to Answer
            if (exam.ExamDetails.EnumarationAnswer != null)
            {
                exam.ExamDetails.Answer = exam.ExamDetails.EnumarationAnswer;
            }

            // If EnumarationAnswer is not null, assign it directly to Answer
            if (exam.ExamDetails.IdentificationAnswer != null)
            {
                exam.ExamDetails.Answer = exam.ExamDetails.IdentificationAnswer;
            }


            var response = db.ExecuteScalar("sp_AddQuestion", new
            {
                exam.ExamDetails.Id,
                exam.ExamDetails.SubType,
                exam.ExamDetails.ItemNo,
                exam.ExamDetails.Question,
                exam.ExamDetails.choiceA,
                exam.ExamDetails.choiceB,
                exam.ExamDetails.choiceC,
                exam.ExamDetails.choiceD,
                exam.ExamDetails.Answer
            },
               commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Details", new { id = exam.ExamDetails.Id });
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Question has been saved successfully.";
            return RedirectToAction("Details", new { id = exam.ExamDetails.Id });


        }






        [HttpPost]
        public IActionResult EditQuestion(Exam exam)
        {
            // If ChoiceAnswer is not null, assign corresponding value to Answer
            if (exam.ExamDetails.ChoiceAnswer != null)
            {
                if (exam.ExamDetails.ChoiceAnswer == "A")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceA;
                else if (exam.ExamDetails.ChoiceAnswer == "B")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceB;
                else if (exam.ExamDetails.ChoiceAnswer == "C")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceC;
                else if (exam.ExamDetails.ChoiceAnswer == "D")
                    exam.ExamDetails.Answer = exam.ExamDetails.choiceD;
            }

            // If TrueFalseAnswer is not null, assign corresponding value to Answer
            if (exam.ExamDetails.TrueFalseAnswer != null)
            {
                if (exam.ExamDetails.TrueFalseAnswer == "tama")
                    exam.ExamDetails.Answer = "True";
                else if (exam.ExamDetails.TrueFalseAnswer == "mali")
                    exam.ExamDetails.Answer = "False";
            }

            // If EnumarationAnswer is not null, assign it directly to Answer
            if (exam.ExamDetails.EnumarationAnswer != null)
            {
                exam.ExamDetails.Answer = exam.ExamDetails.EnumarationAnswer;
            }

            // If EnumarationAnswer is not null, assign it directly to Answer
            if (exam.ExamDetails.IdentificationAnswer != null)
            {
                exam.ExamDetails.Answer = exam.ExamDetails.IdentificationAnswer;
            }


            var response = db.ExecuteScalar("sp_EditQuestion", new
            {
                exam.IsPractical,
                exam.ExamDetails.SubType,
                exam.ExamDetails.Id,
                exam.ExamDetails.ExamId,
                exam.ExamDetails.PracId,
                exam.ExamDetails.ItemNo,
                exam.ExamDetails.Question,
                exam.ExamDetails.choiceA,
                exam.ExamDetails.choiceB,
                exam.ExamDetails.choiceC,
                exam.ExamDetails.choiceD,
                exam.ExamDetails.Answer

            },
          commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Update failed";
                TempData["Error Message"] = response?.ToString();

                return RedirectToAction("Details", new { id = exam.ExamDetails.Id });
            }

            TempData["Success Title"] = "Update successful";
            TempData["Success Message"] = "Training has been updated successfully.";
            return RedirectToAction("Details", new { id = exam.ExamDetails.Id });

        }



        [HttpPost]
        public IActionResult DeleteQuestion(Exam exam)
        {
            var del = db.ExecuteScalar("sp_DeleteQuestion", new { exam.ExamDetails.ExamId, CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name) }, commandType: System.Data.CommandType.StoredProcedure);

            if (del is null)
            {
                TempData["Success Title"] = "Delete successful";
                TempData["Success Message"] = "Deletion of Question is successful.";

                return RedirectToAction("Details", new { exam.Id });
            }



            TempData["Error Title"] = "Delete failed";
            TempData["Error Message"] = "Deletion of Question Failed.";
            return RedirectToAction("Details", new { exam.Id });
        }


        [HttpPost]
        public IActionResult SavePracQuestion(Exam exam)
        {


            var response = db.ExecuteScalar("sp_AddPracQuestion", new
            {
                exam.ExamDetails.Id,
                exam.ExamDetails.ItemNo,
                exam.ExamDetails.Question
            },
               commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Details", new { id = exam.ExamDetails.Id });
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "Question has been saved successfully.";
            return RedirectToAction("Details", new { id = exam.ExamDetails.Id });


        }






        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetName(string EmpId, int Id, int TEId)
        {
            if (string.IsNullOrEmpty(EmpId))
            {
                return Json(new { FullName = string.Empty });
            }

            var sql = @"
                       
                        SELECT FullName FROM TrainingAttendee a
                        join EmployeeMaster b
                        on a.EmpId = b.EmpId
                        JOIN ProjectDetails c
                        on a.PTId = c.PTId
                        join TrainingMaster d
                        on c.TId = d.TId
                        join ExamTran e
                        on c.PTId = e.PTId
                        WHERE a.EmpId = @EmpId and c.TId in 
                        ( Select TId from ExamTran a
					    JOIN ProjectDetails b
					    on a.PTId = b.PTId
					    where ExamId = @Id) and e.TEID = @TEId
                        ";
            var fullName = db.QueryFirstOrDefault<string>(sql, new { EmpId,Id,TEId});

            return Json(new { FullName = fullName });
        }




        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllDetails(string EmpId, int Id, int TEId)
        {
            if (string.IsNullOrEmpty(EmpId))
            {
                return Json(new { success = false, message = "Employee ID is required", data = new List<object>() });
            }

            var sql = @"
                       
                        SELECT a.PTId, 
                        a.ExamId, 
                        b.EmpId, 
                        c.* ,
                        d.FullName,
                        d.Department,
                        d.Position 
                        FROM ExamTran a
                        Join TrainingAttendee b
                        on a.PTId = b.PTId
                        JOIN ScheduleHeader c
                        ON a.PTId = c.PTID
                        join EmployeeMaster d
                        on b.EmpId = d.EmpId
                        JOIN ProjectDetails e
                        on a.PTId = e.PTId
                        WHERE b.EmpId = @EmpId and e.TId in
                        ( Select TId from ExamTran a
                        JOIN ProjectDetails b
                        on a.PTId = b.PTId
                        where ExamId = @Id) 
                        ";
            try
            {
                var result = db.Query(sql, new { EmpId, Id }).ToList();

                if (result == null || !result.Any())
                {
                    return Json(new { success = false, message = "No details found", data = new List<object>() });
                }

                return Json(new { success = true, message = "Details retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return Json(new { success = false, message = "An error occurred while fetching details", error = ex.Message });
            }
        }





        public IActionResult Finalized(int Id, string Title)
        {
            var fin = db.ExecuteScalar("sp_FinalExam", new { Id, Title }, commandType: System.Data.CommandType.StoredProcedure);
            if (fin.ToString() != "Successful")
            {
                TempData["Error Title"] = "Finalizing failed";
                TempData["Error Message"] = "Finaling of Exam Failed.";
                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Finalize successful";
            TempData["Success Message"] = "Finalize of Exam is successful.";

            return RedirectToAction("Index");


        }


        public IActionResult Revise(int Id, bool IsWritten)
        {                     
            var rev = db.ExecuteScalar("sp_RevisionExam", new { Id, IsWritten, ReviseBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)}, commandType: System.Data.CommandType.StoredProcedure);
            if (rev.ToString() != "Successful")
            {
                TempData["Error Title"] = "For Revisioning failed";
                TempData["Error Message"] = rev.ToString();
                return RedirectToAction("Index");
            }


            TempData["Success Title"] = "Examination is a now Open";
            TempData["Success Message"] = "Examination open for Revision is successful.";

            return RedirectToAction("Index");

        }



    }
}
