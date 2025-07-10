using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrainingSys.Models
{
    public class ScheduleMaster
    {
        public int TTId { get; set; }
        public int TId { get; set; }

        public string full_name { get; set; }
        public string employee_id { get; set; }
        public string department { get; set; }
        public string position_title { get; set; }

        public bool? IsWritten { get; set; }


        public IEnumerable<ScheduleMaster> scheduleMaster { get; set; }
        public TrainHead TrainHead { get; set; }
        public Attendee Attendee { get; set; }

        public TrainMaster TrainMaster { get; set; }
        public IEnumerable<TrainHead> trainHead { get; set; }
        public IEnumerable<TrainMaster> training { get; set; }

        public IEnumerable<TrainGroup> trainGroup { get; set; }

        public IEnumerable<Profile> profiles { get; set; }


        public IEnumerable<Attendee> attendee { get; set; }
        public List<Attendee> AttendeeDetails { get; set; }

    }

    public class TrainHead
    {
        public int PTId { get; set; }
        public int TTId { get; set; }
        public int TId { get; set; }
        public int PMId { get; set; }
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string ProjectName { get; set; }
        public string fullName { get; set; }
        public string title { get; set; }
        public string EmpId { get; set; }
        public DateTime? SchedDate { get; set; }
        public string Location { get; set; }
        public string Trainor { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCancel { get; set; }
        public string CancelBy { get; set; }
        public DateTime CancelDate { get; set; }
        public string crtdBy { get; set; }
        public DateTime crtdDate { get; set; }
        public string updtBy { get; set; }
        public DateTime updtDate { get; set; }
        public string Status { get; set; }

        public string RescheduledBy { get; set; }

        public bool? IsWritten { get; set; }


    }


    public class Attendee
    {
        public int TAId { get; set; }
        public int TTId { get; set; }
        public int TEId { get; set; }
        public int PId { get; set; }
        public int TId { get; set; }
        public int EXamId { get; set; }
        public int EmpId { get; set; }
        public string fullName { get; set; }
        public string position { get; set; }
        public string department { get; set; }
        public bool IsAttanded { get; set; }
        public bool IsFinished { get; set; }

        public bool IsWritten { get; set; }

    }


    public class ShowTaker
    {
        public int EmpId { get; set; }
        public string fullName { get; set; }
        public string Title { get; set; }
        public string Try1 { get; set; }
        public string Try2 { get; set; }
        public string Try3 { get; set; }

        public IEnumerable<ShowTaker> showTaker { get; set; }

    }


    public class TestTaker
    {
        public int WEId { get; set; }
        public int ExamId { get; set; }
        public string EmpId { get; set; }
        public string fullName { get; set; }
        public decimal Score { get; set; }
        public DateTime ScoreDate { get; set; }
        public string Remark { get; set; }
        public int ItemNo { get; set; }
        public string Answer { get; set; }
        public string Response { get; set; }
        public string Title { get; set; }

        public DateTime ExamDate { get; set; }

        public string ProjectName { get; set; }
        public string TrainingName { get; set; }
        public string FinalStat { get; set; }

        public string ExamType { get; set; }
        public string Position { get; set; }
        public int ExamCounter { get; set; }
        public int FinalScore { get; set; }
        public IEnumerable<TestTaker> testTaker { get; set; }


    }

    public class WrittenExamDetails
    {
        public int ADId { get; set; }
        public int WEId { get; set; }
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int ItemNo { get; set; }
        public string Answer { get; set; }
        public string Response { get; set; }
    }

}
