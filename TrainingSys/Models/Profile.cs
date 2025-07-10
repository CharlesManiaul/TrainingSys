namespace TrainingSys.Models
{
    public class Profile
    {

        public int TId { get; set; }

        public int Id { get; set; }
        public int TEId { get; set; }
        public int PTId { get; set; }

        public int ExamId { get; set; }
        public int PMId { get; set; }


        public string Title { get; set; }
        public int? TotalSessions { get; set; }
        public int? CompletedSessions { get; set; }
        public string ExamTitle { get; set; }
        public string EmpId { get; set; }
        public DateTime SchedDate { get; set; }
        public string ExamStatus { get; set; }
        public string TrainingStatus { get; set; }
        public string Status { get; set; }
        public int TimesTaken { get; set; }
        public string Location { get; set; }
        public string Trainor { get; set; }
        public bool IsWritten { get; set; }
        public bool IsPractical { get; set; }
        public Profile pfp { get; set; }    

        public string full_name { get; set; }
        public string FinalStat { get; set; }

        public string FinalScore { get; set; }

        public string TrainingName { get; set; }
        public string ExamType { get; set; }

        public int ExamCounter { get; set; }

        public IEnumerable<Profile> profile { get; set; }

        public ScheduleMaster master;

        public IEnumerable<ScheduleMaster> scheduleMaster { get; set; }


        public IEnumerable<TrainingDetails> trainingDetails { get; set; }
        public List<TrainingDetails> TrainingDetails { get; set; }


        public Exam exam { get; set; }
        public List<ExamDetails> examDetails { get; set; }

        public UserExam userExam { get; set; }

        public IEnumerable<UserExam> UserExam { get; set; }

    }





    public class UserExam
    {
        public int WEId { get; set; }
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string fullName { get; set; }
        public decimal Score { get; set; }
        public DateTime ScoreDate { get; set; }
        public string Remark { get; set; }
        public string Question { get; set; }
        public int ItemNo { get; set; }
        public string Answer { get; set; }
        public string UserAnswer { get; set; }
        public string Response { get; set; }
        public string choiceA { get; set; }
        public string choiceB { get; set; }
        public string choiceC { get; set; }
        public string choiceD { get; set; }
        public string Title { get; set; }

        public int ExamCounter { get; set; }
        public int Grade { get; set; }

    }



}
