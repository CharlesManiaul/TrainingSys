namespace TrainingSys.Models
{
    public class TrainMaster
    {

        public int TId { get; set; }
        public int Id { get; set; }
        public int TEId { get; set; }
        public int PTId { get; set; }
        public int PMId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsWritten { get; set; }
        public bool IsPractical { get; set; }
        public int Validity { get; set; }
        public int? Amount { get; set; }
        public string crtdBy { get; set; }
        public int ExamId { get; set; }
        public string Status { get; set; }
        public DateTime crtdDate { get; set; }
        public int TotAttendee { get; set; }
        public bool HasExam { get; set; }

        public IEnumerable<TrainMaster> trainMaster { get; set; }

        public WrittenQuestion WrittenQuestion { get; set; }

        public List<Exam> exams { get; set; }

        public IEnumerable<Exam> Exam { get; set; }

        public IEnumerable<TrainingDetails> TrainingDetails { get; set; }
        public TrainingDetails trainingDetails { get; set; }


        public IEnumerable<ExamDetails> examDetails { get; set; }

        public ExamDetails ExamDetails { get; set; }
    }


    public class WrittenQuestion
    {
        public int WId { get; set; }
        public int TId { get; set; }
        public int ItemNo { get; set; }
        public string choiceA { get; set; }
        public string choiceB { get; set; }
        public string choiceC { get; set; }
        public string choiceD { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

    }

    public class TrainingDetails
    {
        public int ExamId { get; set; }
        public int TDId { get; set; }
        public int TId { get; set; }
        public int Id { get; set; }
        public int PracId { get; set; }
        public string Title { get; set; }

        public string CrtdBy { get; set; }

        public int Revision { get; set; }

        public int ItemNo { get; set; }
        public string choiceA { get; set; }
        public string choiceB { get; set; }
        public string choiceC { get; set; }
        public string choiceD { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }


    }



}
