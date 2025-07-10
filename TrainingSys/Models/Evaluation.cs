namespace TrainingSys.Models
{
    public class Evaluation
    {
        public int TId { get; set; }
        public int ExamId { get; set; }
        public int Id { get; set; }
        public int HId { get; set; }
        public string EmpId { get; set; }
        public string TraineeName { get; set; }
        public string Position { get; set; }
        public string DepartmentStore { get; set; }
        public string TrainingCourse { get; set; }
        public string Venue { get; set; }
        public string Trainor { get; set; }
        public DateTime? EvaluationDate { get; set; }

        public IEnumerable<Evaluation> evaluation{ get; set; }
        public List<EvaluationForm> EvaluationForm { get; set; }

        public EvaluationForm evaluationForm { get; set; }

        public EvaluationDetails evaluationDetails { get; set; }
        public Evaluation Evaluations { get; set; }
        public List<EvaluationDetails> EvaluationDetails { get; set; }

        public IEnumerable<EvaluationDetails> Evaluationdetails { get; set; }

        public IEnumerable<SubType> SubTypes { get; set; }

    }



    public class EvaluationDetails 
    {
        public string EmpId { get; set; }
        public string TraineeName { get; set; }
        public string Position { get; set; }
        public string DepartmentStore { get; set; }
        public string TrainingCourse { get; set; }
        public string Venue { get; set; }
        public string Trainor { get; set; }

        public DateTime? EvaluationDate { get; set; }
        public int Id { get; set; }
        public int EId { get; set; }
        public int HId { get; set; }
        public string strAnswer { get; set;}
        public string strComment { get; set;}
        public int Part { get; set; }
        public int EvalNumber { get; set; }
        public string Criteria { get; set; }
        public string Question { get; set; }
        public string Translate { get; set; }
        public string Answer { get; set; }
        public string RateType { get; set; }
        public string Comment { get; set; }
        public string Additionnal { get; set; }

    }


    public class EvaluationForm 
    {
        public int Id { get; set; }
        public int Part { get; set; }
        public int EvalNumber { get; set; }
        public string Criteria { get; set; }
        public string Question { get; set; }
        public string Translate { get; set; }
        public string Answer { get; set; }
        public string RateType { get; set; }
        public bool Comment { get; set; }
        public string Additionnal { get; set; }


    }
   
}

