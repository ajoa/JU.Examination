using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JU.Examination.ViewModel
{
    public class ExamViewModel
    {
        public AlcomScanningOfExam.SearchResultScanningOfExam ScanningOfExams { get; set; }

        public MessageViewModel Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        public ExamViewModel()
        {
            ScanningOfExams = new AlcomScanningOfExam.SearchResultScanningOfExam();
            Message = new MessageViewModel();
        }
    }
}