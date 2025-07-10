namespace TrainingSys.Models
{
    public class Exam
    {
        public int Id { get; set; }

        public string EmpId { get; set; }

        public int ExamId { get; set; }

        public int TEId { get; set; }

        public string fullName { get; set; }

        public string CrtdBy { get; set; }
        public DateTime CrtdDate { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }
        public int Revision { get; set; }

        public string ReviseBy { get; set; }

        public DateTime ReviseDate { get; set; }

        public DateTime FinalDate { get; set; }

        public string PassingScore { get; set; }

        public string Grade { get; set; }

        public bool IsWritten { get; set; }
        public bool IsPractical { get; set; }
        public IEnumerable<Exam> exam { get; set; }


        public List<ExamDetails> examDetails { get; set; }



        public List<TrainingDetails> TrainingDetails { get; set; }

        public TrainHead trainHead { get; set; }

        public ExamDetails ExamDetails { get; set; }


        public IEnumerable<SubType> SubTypes { get; set; }


    }

    public class SubType
    { 
        public int SubId { get; set; }
        public string Type { get; set; }
    }

    public class ExamDetails 
    {
        public int ExamId { get; set; }
        public int Id { get; set; }
        public int SubId { get; set; }
        public int PracId { get; set; }
        public int TId { get; set; }
        public int ItemNo { get; set; }
        public string choiceA { get; set; }
        public string choiceB { get; set; }
        public string choiceC { get; set; }
        public string choiceD { get; set; }
        public string Question { get; set; }
        public string Type { get; set; }
        public string EnumarationAnswer { get; set; }
        public string IdentificationAnswer { get; set; }
        public string TrueFalseAnswer { get; set; }
        public string ChoiceAnswer { get; set; }
        public string Answer { get; set; }
        public string SubType { get; set; }


    }
}
