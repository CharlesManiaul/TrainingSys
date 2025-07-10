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
using FluentFTP.Helpers;
using System.Diagnostics;
using MySqlX.XDevAPI;
using static System.Runtime.InteropServices.JavaScript.JSType;
using NuGet.ContentModel;





namespace TrainingSys.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        SqlConnection db;
        MySqlConnection navee;

        public GroupController(IConfiguration confg)
        {
            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));
            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));
        }

        //INDEX
        public IActionResult Index()
        {
            TrainGroup group = new TrainGroup();
            ViewBag.Training = "active";
            string sql = "";

            group.trainGroup = db.Query<TrainGroup>("sp_ReportProjectStatus_All");


            sql = @"Select * FROM TrainingMaster Where (IsCancel = 0 or IsCancel is null)";
            group.trainMaster = db.Query<TrainMaster>(sql).ToList();

            return View(group);
        }

        public IActionResult addGroup(TrainGroup trainGroup)
        {
            if (trainGroup.SelectedTrainings == null || !trainGroup.SelectedTrainings.Any())
            {
                TempData["Error Title"] = "No Training selected";
                TempData["Error Message"] = "Please select at least one Training.";
                return RedirectToAction("Index");
            }

            DataTable Trainings = new DataTable();
            Trainings.Columns.Add("TId", typeof(int));
            Trainings.Columns.Add("Id", typeof(int));

            // Split the SelectedIds string into an array of integers
            var selectedIdsArray = trainGroup.SelectedIds?.Split(',').Select(int.Parse).ToArray();

            for (int i = 0; i < trainGroup.SelectedTrainings.Count; i++)
            {
                Trainings.Rows.Add(trainGroup.SelectedTrainings[i], selectedIdsArray[i]);
            }

            var response = db.ExecuteScalar("sp_AddGroup", new
            {
                trainGroup.GroupTitle,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
                Trainings
            }, commandType: System.Data.CommandType.StoredProcedure);

            // Check the response and handle accordingly
            if (response == null || response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Creating of Group Failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Success!";
            TempData["Success Message"] = "Group Training created successfully";
            return RedirectToAction("Index");
        }

        public IActionResult DeleteGroup(int PMId)
        {
            var response = db.ExecuteScalar("sp_DeleteTrainGroup", new
            {
                PMId,
                CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            }, commandType: System.Data.CommandType.StoredProcedure);

            // Check the response and handle accordingly
            if (response == null || response.ToString() != "Successful")
            {
                TempData["Success Title"] = "Success!";
                TempData["Success Message"] = "Deletion of Examination is successful.";

                return RedirectToAction("Index");
            }

            TempData["Error Title"] = "Delete Failed";
            TempData["Error Message"] = "Deletion of Examination Failed.";
            return RedirectToAction("Index");
        }

        //DETAILS
        [Route("Group/Details/{PMId}")]
        public IActionResult Details(int PMId)
        {
            ViewBag.Training = "active";
            TrainGroup group = new TrainGroup();

            string sql = "";
            group.TrainMaster = db.Query<TrainMaster>("sp_ProjectDetails", new { PMId });



            sql = @"Select * FROM TrainingMaster Where (IsCancel = 0 or IsCancel is null)";
            group.trainMaster = db.Query<TrainMaster>(sql).ToList();


            sql = @"Select * FROM TrainingMaster Where (IsCancel = 0 or IsCancel is null)";
            group.TMaster = db.QueryFirstOrDefault<TrainMaster>(sql);

            sql = "SELECT employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            group.scheduleMaster = navee.Query<ScheduleMaster>(sql);

            sql = @"SELECT * FROM ExamHeader WHERE Id in (SELECT MAX(Id)FROM ExamHeader where status= 'Finalized' AND (IsCancel = 0 OR IsCancel IS NULL) GROUP BY Title ) ORDER BY Title";
            group.exam = db.Query<Exam>(sql).ToList();


            sql = "SELECT  employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            group.profiles = navee.Query<Profile>(sql);

            return View(group);
        }

        [Route("Group/Details/Information/{PMId}/{PTId}")]
        public IActionResult Information(TrainGroup trainGroup)
        {
            ViewBag.Training = "active";
            string sql = "";

            sql = "SELECT employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            trainGroup.scheduleMaster = navee.Query<ScheduleMaster>(sql);

            sql = @"
                    SELECT  d.Title,a.*, c.PMId, c.PTId
                    FROM ScheduleHeader a
                    join ExamTran b
                    On a.PTID = b.PTID
                    JOIN ProjectDetails c
                    on b.PTId = c.PTId
                    join TrainingMaster d
                    on c.TId = d.TId
                    where (c.PMId = @PMId and a.PTId = @PTId) and (b.ExamId = 0 or b.ExamId is not null)
                    ORDER BY TTId desc
                                        ";
            trainGroup.trainHead = db.Query<TrainHead>(sql, new { trainGroup.PMId, trainGroup.PTId });


            sql = @"
                    SELECT  d.Title,a.*, c.PMId, c.PTId
                    FROM ScheduleHeader a
                    join ExamTran b
                    On a.PTID = b.PTID
                    JOIN ProjectDetails c
                    on b.PTId = c.PTId
                    join TrainingMaster d
                    on c.TId = d.TId
                    where (c.PMId = @PMId and a.PTId = @PTId) and (b.ExamId = 0 or b.ExamId is not null)
                    ORDER BY TTId desc
                                        ";
            trainGroup.TrainHead = db.QueryFirstOrDefault<TrainHead>(sql, new { trainGroup.PMId, trainGroup.PTId });



            sql = @"SELECT * 
                    FROM TrainingAttendee a
                    JOIN EmployeeMaster b ON a.EmpId = b.EmpId
                    JOIN ExamTran c ON a.PTId = c.PTId
                    Left JOIN ExamHeader d ON c.ExamId = d.id
                    WHERE a.PTId = @PTId
                    AND (a.IsRemoved IS NULL OR a.IsRemoved = 0) and (d.IsCancel IS NULL OR d.IsCancel = 0)";

            trainGroup.AttendeeDetails = db.Query<Attendee>(sql, new { trainGroup.PTId }).ToList();

            sql = @"SELECT * FROM TrainingAttendee a
                    JOIN EmployeeMaster b
                    on a.EmpId = b.EmpId
                    JOIN ExamTran c
                    on a.PTId = c.PTId
                    Left join ExamHeader d
                    on c.ExamId = d.id
                    WHERE a.PTId  = @PTId and (a.IsRemoved is null or a.IsRemoved = 0)";
            trainGroup.Attendee = db.QueryFirstOrDefault<Attendee>(sql, new { trainGroup.PTId });

            sql = @"SELECT * FROM TrainingAttachment";
            trainGroup.attachments = db.Query<Attachment>(sql ).ToList();




            return View(trainGroup);

        }

        public IActionResult addMore(TrainGroup trainGroup)
        {
            if (trainGroup.SelectedTrainings == null || !trainGroup.SelectedTrainings.Any())
            {
                TempData["Error Title"] = "No Training selected";
                TempData["Error Message"] = "Please select at least one Training.";
                return RedirectToAction("Index");
            }


            DataTable Trainings = new DataTable();
            Trainings.Columns.Add("TId", typeof(int));
            Trainings.Columns.Add("Id", typeof(int));

            // Split the SelectedIds string into an array of integers
            var selectedIdsArray = trainGroup.SelectedIds?.Split(',').Select(int.Parse).ToArray();

            for (int i = 0; i < trainGroup.SelectedTrainings.Count; i++)
            {
                Trainings.Rows.Add(trainGroup.SelectedTrainings[i], selectedIdsArray[i]);
            }




            var response = db.ExecuteScalar("sp_AddMoreGroup", new
            {
                trainGroup.PMId,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
                Trainings
            }, commandType: System.Data.CommandType.StoredProcedure);

            // Check the response and handle accordingly
            if (response == null || response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Failed to add Training";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Details", new { trainGroup.PMId });
            }

            TempData["Success Title"] = "Success!";
            TempData["Success Message"] = "Training Added  successfully";
            return RedirectToAction("Details", new { trainGroup.PMId });
        }

        public IActionResult DeleteTrainingDetails(TrainGroup trainGroup)
        {
            var response = db.ExecuteScalar("sp_DeleteDTrainGroup", new
            {
                trainGroup.TMaster.TId,
                CancelBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            }, commandType: System.Data.CommandType.StoredProcedure);

            // Check the response and handle accordingly
            if (response == null || response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Delete Failed";
                TempData["Error Message"] = "Deletion of Examination Failed.";
                return RedirectToAction("Details", new { trainGroup.PTId });
            }

            TempData["Success Title"] = "Success!";
            TempData["Success Message"] = "Deletion of Examination is successful.";
            return RedirectToAction("Details", new { trainGroup.PTId });
        }

        //SCHEDULING
        [HttpPost]
        public IActionResult SaveSchedule(TrainGroup trainGroup)
        {

            //if (trainGroup.TrainHead.PTId == 0)
            //{
            //    // Handle the case where assetMovementDetails is null
            //    TempData["Error Title"] = "Save failed";
            //    TempData["Error Message"] = "There was a problem";
            //    return RedirectToAction("Index");
            //}


            if (trainGroup.TrainHead.Status == "Scheduled")
            {
                // Handle the case where assetMovementDetails is null
                TempData["Error Title"] = "Save failed";
                TempData["Error Message"] = "This training is scheduled.";
                return RedirectToAction("Index");
            }


            var response = db.ExecuteScalar("sp_AddSchedule", new
            {
                trainGroup.TrainHead.PTId,
                trainGroup.TrainHead.SchedDate,
                trainGroup.TrainHead.Location,
                trainGroup.TrainHead.Trainor,
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
        public IActionResult Reschedule(TrainGroup trainGroup)
        {

            var response = db.ExecuteScalar("sp_Reschedule", new
            {
                trainGroup.TrainHead.TTId,
                trainGroup.TrainHead.SchedDate,
                RescheduledBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)
            });

            if (response is null)
            {
                TempData["Success Title"] = "Rescheduling successful";
                TempData["Success Message"] = "You have Rescheduled a Training.";
                return RedirectToAction("Information", new { trainGroup.TrainHead.PMId, trainGroup.TrainHead.PTId });


            }

            TempData["Error Title"] = "Rescheduling failed";
            TempData["Error Message"] = response?.ToString();
            return RedirectToAction("Information", new { trainGroup.TrainHead.PMId, trainGroup.TrainHead.PTId });

        }

        [HttpPost]
        public IActionResult FinishSchedule(TrainGroup trainGroup)
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

                foreach (var row in trainGroup.AttendeeDetails)
                {
                    AttendeeDetails.Rows.Add(row.TTId, row.PId, row.EmpId, row.fullName, row.position, row.department, row.IsAttanded, row.IsFinished);
                }


                var response = db.ExecuteScalar("sp_FinishSchedule", new
                {
                    trainGroup.TrainHead.TTId,
                    trainGroup.PTId,
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
        public IActionResult AddAttendee(TrainGroup trainGroup)
        {
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

                foreach (var empId in trainGroup.attendee)
                {
                    // Assuming you have a method to get details by empId
                    var attendeeDetails = GetAttendeeDetails(empId);
                    if (attendeeDetails != null)
                    {
                        AttendeeDetails.Rows.Add(
                            attendeeDetails.TTId,
                            attendeeDetails.TId,
                            attendeeDetails.EmpId,
                            attendeeDetails.fullName,
                            attendeeDetails.position,
                            attendeeDetails.department,
                            true,  // Set defaults for IsAttended and IsFinished
                            false
                        );
                    }
                }

                var response = db.ExecuteScalar("sp_AddAttendee", new
                {
                    trainGroup.TrainHead.PTId,
                    AttendeeDetails
                });

                if (response.ToString() != "Successful")
                {
                    TempData["Error Title"] = "Saving of Attendee failed";
                    TempData["Error Message"] = response?.ToString();
                    return RedirectToAction("Index");
                }

                TempData["Success Title"] = "Save successful";
                TempData["Success Message"] = "You have successfully Added Attendee.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult RemoveAttendee(int TAId)

        {

            var response = db.ExecuteScalar("sp_RemoveAttendee", new
            {

                TAId,
                RemoveBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)

            });

            if (response.ToString() != "Successful")
            {
                TempData["Error Title"] = "Removing Attendee failed";
                TempData["Error Message"] = response?.ToString();
                return RedirectToAction("Index");
            }

            TempData["Success Title"] = "Save successful";
            TempData["Success Message"] = "You have successfully remove attendee.";
            return RedirectToAction("Index");


        }

        // Method to fetch attendee details by employee ID
        private Attendee GetAttendeeDetails(string EmpId)
        {
            string sql = @"SELECT employee_id As EmpId, full_name As fullName, position_title AS Position, department 
                   FROM `tabEmployee` 
                   WHERE is_active = 1 AND employee_id = @EmpId 
                   ORDER BY full_name ASC";

            var attendee = navee.Query<Attendee>(sql, new { EmpId }).FirstOrDefault();
            return attendee; // This will return null if no attendee is found
        }

        [HttpPost]
        public IActionResult AddExam(TrainGroup trainGroup)
        {

            var response = db.ExecuteScalar("sp_AddProjExam", new
            {
                trainGroup.ExamId,
                trainGroup.PTId,
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

        public IActionResult ProjectReport()
        {
            TrainGroup group = new TrainGroup();
            ViewBag.reports = "active";

            string sql = "";

            group.trainGroup = db.Query<TrainGroup>("sp_ReportProjectStatus_All");

            return View(group);
        }

        public IActionResult FinishedProjectReport()
        {
            TrainGroup group = new TrainGroup();
            ViewBag.reports = "active";

            string sql = "";

            group.trainGroup = db.Query<TrainGroup>("sp_ReportProjectFinished");

            return View(group);
        }

        [Route("Group/ViewAnswers/{TEId}/{EmpId}")]
        public IActionResult ViewAnswers(int EmpId, int TEId)
        {
            TrainGroup group = new TrainGroup();
            string sql;
            sql = @"SELECT * 
                    FROM TrainingAttendee a
                    JOIN EmployeeMaster b ON a.EmpId = b.EmpId
                    JOIN ExamTran c ON a.PTId = c.PTId
                    Left JOIN ExamHeader d ON c.ExamId = d.id
                    WHERE c.TEId = @TEId
                    AND (a.IsRemoved IS NULL OR a.IsRemoved = 0) and a.EmpId = @EmpId";

            group.pfp = db.QueryFirstOrDefault<Profile>(sql, new { TEId, EmpId });


            if (group.pfp.IsWritten == true)
            {
                sql = @"SELECT a.Score, 
                        a.ScoreDate,
                        a.ExamCounter,
                        a.Remark,
                        b.ItemNo, 
                        b.Answer As UserAnswer, 
                        b.Response,
                        c.*,
                        d.Title
                        FROM ExamAnswerReport a
                        join WrittenExamDetails b
                        on a.WEId = b.WEId
                        join ExamDetails c
                        on b.WrittenId = c.ExamId
                        join ExamHeader d
                        on c.Id = d.Id
                        join ExamTran e
                        on a.TEId = e.TEId
                        Where EmpId = @EmpId and a.TEId = @TEId";
                group.UserExam = db.Query<UserExam>(sql, new { EmpId, TEId });
            }
            else
            {
                sql = @"
                     SELECT 
                     a.TEId, 
                     a.ExamCounter,
                     a.Score, 
                     a.ScoreDate,
                     a.Remark,
                     b.ItemNo,
                     b.Grade,
                     b.Answer As UserAnswer, 
                     b.Response,
                     c.*,
                     d.Title
                        FROM ExamAnswerReport a
                         join ExamPracDetails_Answer b
                            on a.WEId = b.WEId
                         join ExamPracDetails c
                            on b.PracId = c.PracId
                         join ExamHeader d
                            on c.Id = d.Id
                         join ExamTran e
                        on a.TEId = e.TEId
                     Where EmpId = @EmpId and a.TEId = @TEId";
                group.UserExam = db.Query<UserExam>(sql, new { EmpId, TEId });
            }
            return View(group);

        }



        [Route("Group/ViewEvaluation/{TEId}/{EmpId}")]
        public IActionResult ViewEvaluation(int EmpId, int TEId)
        {
            TrainGroup group = new TrainGroup();
            string sql;
            sql = @"SELECT * 
                    FROM TrainingAttendee a
                    JOIN EmployeeMaster b ON a.EmpId = b.EmpId
                    JOIN ExamTran c ON a.PTId = c.PTId
                    Left JOIN ExamHeader d ON c.ExamId = d.id
                    WHERE c.TEId = @TEId
                    AND (a.IsRemoved IS NULL OR a.IsRemoved = 0) and a.EmpId = @EmpId";

            group.pfp = db.QueryFirstOrDefault<Profile>(sql, new { TEId, EmpId });




            sql = @"
                     SELECT DISTINCT
                     a.TEId, 
                     a.ExamCounter,
                     a.Score, 
                     a.ScoreDate,
                     a.Remark,
                     b.ItemNo,
                     b.Grade,
                     b.Answer As UserAnswer, 
                     b.Response,
                     c.*,
                     d.Title
                        FROM ExamAnswerReport a
                         join ExamPracDetails_Answer b
                            on a.WEId = b.WEId
                         join ExamPracDetails c
                            on b.PracId = c.PracId
                         join ExamHeader d
                            on c.Id = d.Id
                         join ExamTran e
                        on a.TEId = e.TEId
                     Where EmpId = @EmpId and a.TEId = @TEId";
            group.UserExam = db.Query<UserExam>(sql, new { EmpId, TEId });

            return View(group);
        }


        [HttpPost]
        public IActionResult UploadFile(TrainGroup tGroup)
        {


            DataTable Attachment = new DataTable();
            Attachment.Columns.Add("FileName", typeof(string));
            if (tGroup.Attachment is not null)
            {
                foreach (var item in tGroup.Attachment)
                {
                    string fileName = $"{Path.GetFileNameWithoutExtension(item.FileName)}{DateTime.Now.ToString("yyyyMMddhhmmss")}{Path.GetExtension(item.FileName)}";


                    var client = new FtpClient("172.16.0.12", "ftpadmin", "welcome");
                    client.Connect();
                    FtpStatus stat = client.UploadStream(item.OpenReadStream(), $"TrainSys/Attachments/{fileName}");
                    client.Disconnect();

                    Attachment.Rows.Add(fileName);
                }
            }

            var affectedRow = db.ExecuteScalar("sp_UploadFile", new { tGroup.PMId, tGroup.TTId, tGroup.PTId, tGroup.EmpId, Attachment }, commandType: System.Data.CommandType.StoredProcedure);
            if (affectedRow?.ToString() != "Successful")
            {
                TempData["Error Title"] = "Update failed";
                TempData["Error Message"] = affectedRow?.ToString();
                return RedirectToAction("Details", new { tGroup.PMId, tGroup.PTId });
            }

            TempData["Success Title"] = "Update successful";
            TempData["Success Message"] = "Order has been updated successfully.";
            return RedirectToAction("Details", new { tGroup.PMId, tGroup.PTId });

        }



        public IActionResult GetAttachmentModal(int TTId, int PTId, int PMId, int EmpId)
        {
            // Retrieve the attachment data based on the parameters
            var attachments = db.Query<Attachment>(@"
        SELECT * FROM TrainingAttachment 
        WHERE PMId = @PMId AND PTId = @PTID AND TTId = @TTId AND EmpId = @EmpId",
                new { TTId, PTId, PMId, EmpId }).ToList();

            // Return the partial view with the attachments data
            return PartialView("_AttachmentModal", attachments);
        }



        public IActionResult GetAttachment(TrainGroup tGroup,string fileName)
        {
            var model = db.QueryFirstOrDefault<Attachment>(@"  SELECT * FROM TrainingAttachment 
                                                                Where 
                                                                FileName = @fileName
                                                                ", new { fileName });

            var client = new FtpClient("172.16.0.12", "ftpadmin", "welcome");
            try
            {
                client.Connect();

                client.DownloadBytes(out byte[] bytes, $"TrainSys/Attachments/{model.FileName}");

                client.Disconnect();

                // Determine content type based on file extension
                string contentType;
                switch (Path.GetExtension(model.FileName).ToLowerInvariant())
                {
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".xlsx":
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    case ".xls":
                        contentType = "application/vnd.ms-excel";
                        break;
                    // Add more cases for other file types as needed
                    default:
                        // If file type is unknown, return as application/octet-stream
                        contentType = "application/octet-stream";
                        break;
                }

                return File(bytes, contentType);
            }
            catch
            {
                return File("~/Images/noimage.png", "image/png");
            }
        }


    }
}
