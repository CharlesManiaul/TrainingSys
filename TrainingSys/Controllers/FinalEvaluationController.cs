using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using TrainingSys.Models;

namespace TrainingSys.Controllers
{
    public class FinalEvaluationController : Controller
    {
        SqlConnection db;
        MySqlConnection navee;

        public FinalEvaluationController(IConfiguration confg)
        {
            db = new SqlConnection(confg.GetConnectionString("DefaultConnection"));
            navee = new MySqlConnection(confg.GetConnectionString("NaveeConnection"));
        }
        public IActionResult Index()
        {
            Evaluation eval = new Evaluation();
            ViewBag.eval = "active";
            string sql;


            sql = "SELECT * FROM EvaluationHeader";
            eval.evaluation = db.Query<Evaluation>(sql);
            return View(eval);
        }

        [Route("FinalEvaluation/Details/{HId}")]

        public IActionResult Details(int HId)
        {
            Evaluation eval = new Evaluation();
            ViewBag.eval = "active";
            string sql;


            sql = @"SELECT * FROM EvaluationHeader where HId = @HId";
            eval.Evaluations = db.QueryFirstOrDefault<Evaluation>(sql, new { HId });

            sql = @"
                SELECT DISTINCT a.*,b.*,c.*,c.Comment as strComment, a.Answer as strAnswer FROM EvaluationDetails a
                join EvaluationForm b
                on a.EId = b.Id
                JOIN EvaluationDetails c
                on a.HId = c.HId AND a.HDId = c.HDId
                where a.HId = @HId";

            eval.EvaluationDetails = db.Query<EvaluationDetails>(sql, new {HId }).ToList();
            return View(eval);
        }
    }
}
