using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using TrainingSys.Models;

namespace TrainingSys.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {

        SqlConnection db;
        MySqlConnection navee;
        public ProfileController(IConfiguration confg)
        {
            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));

            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));

        }


        public IActionResult Index()
        {
            Profile profile = new Profile();
            string sql;
            ViewBag.Profile = "active";
            sql = "SELECT  employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 ORDER BY name ASC";
            profile.scheduleMaster = navee.Query<ScheduleMaster>(sql);

            return View(profile);
        }


        [Route("Profile/Details/{EmpId}")]
        public IActionResult Details(int EmpId)
        {
            ViewBag.Profile = "active";
            Profile profile = new Profile();
            string sql;
            string Employee;

            sql = "SELECT  employee_id,full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 and employee_id = @EmpId ORDER BY name ASC";
            profile.master = navee.QueryFirstOrDefault<ScheduleMaster>(sql, new { EmpId });

            var parameters = new
            {
                Employee = EmpId.ToString(),  // Adjust if `Employee` in SP expects `EmpId` as a string
                TrainingName = "",  // Optional, leave blank if not required
                ProjectName = ""    // Optional, leave blank if not required
            };


            profile.profile = db.Query<Profile>("sp_getEmpDetails", parameters, commandType: System.Data.CommandType.StoredProcedure);

            profile.pfp = db.QueryFirstOrDefault<Profile>("sp_getEmpDetails", parameters, commandType: System.Data.CommandType.StoredProcedure);

            return View(profile);
        }

        [Route("/Profile/Details/ProfileExamDetails/{EmpId}/{TEId}/{PTId}/{ExamId}/{PMId}")]
        public IActionResult ProfileExamDetails(int EmpId, int TEId, int PTId, int ExamId, int PMId)
        {
            ViewBag.Profile = "active";
            Profile profile = new Profile();
            string sql;

            sql = "SELECT  employee_id, full_name, position_title, department FROM `tabEmployee` WHERE is_active = 1 and employee_id = @EmpId ORDER BY name ASC";
            profile.master = navee.QueryFirstOrDefault<ScheduleMaster>(sql, new { EmpId });

            sql = @"SELECT * 
                    FROM TrainingAttendee a
                    JOIN EmployeeMaster b ON a.EmpId = b.EmpId
                    JOIN ExamTran c ON a.PTId = c.PTId
                    Left JOIN ExamHeader d ON c.ExamId = d.id
                    WHERE c.TEId = @TEId
                    AND (a.IsRemoved IS NULL OR a.IsRemoved = 0) and a.EmpId = @EmpId";

            profile.pfp = db.QueryFirstOrDefault<Profile>(sql, new { TEId, EmpId });





            if (profile.pfp.IsWritten == true)
            {
                sql = @"SELECT DISTINCT 
                        a.Score, 
                        a.ExamCounter,
                        a.ScoreDate,
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
                        Where EmpId = @EmpId and a.TEId = @TEId and e.ExamId = @ExamId";
                profile.UserExam = db.Query<UserExam>(sql, new { EmpId, TEId, ExamId });
            }
           else 
            {
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
                       Where EmpId = @EmpId and a.TEId = @TEId and e.ExamId = @ExamId";
                profile.UserExam = db.Query<UserExam>(sql, new { EmpId, TEId, ExamId });
            }

            return View(profile);
        }



    }
}
