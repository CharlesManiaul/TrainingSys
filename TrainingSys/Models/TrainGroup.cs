using System.Net.Mail;

namespace TrainingSys.Models
{
    public class TrainGroup
    {
        public int PMId { get; set; }
        public int PTId { get; set; }
        public int ExamId { get; set; }
        public int TId { get; set; }
        public int TTId { get; set; }
        public int TEId { get; set; }
        public int TAId { get; set; }
        public int EmpId { get; set; }
        public bool IsWritten { get; set; }

        public string GroupTitle { get; set; }

        public string ProjectName { get; set; }

        public string CrtdBy { get; set; }
        public DateTime CrtdDate { get; set; }
        public string CancelBy { get; set; }

        public DateTime CancelDate { get; set; }

        public bool IsCancel { get; set; }

        public int? TotalTraining { get; set; }
        public int? Finished { get; set; }

        public int Scheduled { get; set; }
        public int UnScheduled { get; set; }

        public int numTrain { get; set; }


      
        public IEnumerable<TrainGroup> trainGroup { get; set; }

        public List<TrainMaster> trainMaster { get; set; }
        public IEnumerable<TrainMaster> TrainMaster { get; set; }

        public Attendee Attendee { get; set; }
        public IEnumerable<ScheduleMaster> scheduleMaster { get; set; }

        public TrainMaster TMaster { get; set; }
        public TrainHead TrainHead { get; set; }
        public IEnumerable<TrainHead> trainHead { get; set; }
        public List<Attendee> AttendeeDetails { get; set; }

        public string[] attendee { get; set; }
        public IEnumerable<Profile> profiles { get; set; }

        public IEnumerable<TrainMaster> training { get; set; }

        public List<Exam> exam { get; set; }
        public IEnumerable<UserExam> UserExam { get; set; }
        public Profile pfp { get; set; }
        public List<int> SelectedTrainings { get; set; } // For TIds
        public string SelectedIds { get; set; } // For Ids as a comma-separated string

        public List<Attachment> attachments { get; set; }
        public IFormFileCollection? Attachment { get; set; }




        //for new tabs

        public List<TrainGroup> OpenGroups { get; set; }
        public List<TrainGroup> CompletedGroups { get; set; }
        public List<TrainGroup> CancelGroups { get; set; }
        public List<TrainMaster> TrainMasterList { get; set; }


    }

    public class Attachment 
    {
        public int PMId { get; set; }
        public int PTId { get; set; }
        public int TAId { get; set; }
        public int TTId { get; set; }
        public int EmpId { get; set; }
        public int TId { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }





    }







}
