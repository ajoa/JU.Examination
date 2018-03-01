using System;
using System.Collections.Generic;


/// <summary>
/// Data container for REST and WS 
/// </summary>
namespace AlcomScanningOfExam
{
    public class CoverSheetData
    {
        public string Courseid { get; set; }
        public string CoursenameFull { get; set; }
        public string CoursenamePresentation { get; set; }
        public string ExamCode { get; set; }
        public string ExamDate { get; set; }
        public string ExamName { get; set; }
        public string ExamTime { get; set; }
        public string Numstudent { get; set; }
        public string Room { get; set; }
        public string Location { get; set; }
        public string FreeText { get; set; }

        public List<CoverSheetStudent> Students { get; set; }

        public string ErrorMsg { get; set; }
        public byte[] PdfFile { get; set; }
        //private string errorMsg = "";
        //private byte[] pdfFile = null;
        //public string ErrorMsg { get { return errorMsg; } }
        //public byte[] PdfFile { get { return pdfFile; } }
        //private string errorMsg = "";
        //private byte[] pdfFile = null;

        public CoverSheetData()
        {
            Courseid = "";
            CoursenameFull = "";
            CoursenamePresentation = "";
            ExamCode = "";
            ExamDate = "";
            ExamName = "";
            ExamTime = "";
            Numstudent = "";
            Room = "";
            Location = "";
            FreeText = "";
            Students = new List<CoverSheetStudent>();
        }
    }

    public class CoverSheetStudent
    {
        public string Anonymkod { get; set; }
        public string Email { get; set; }
        public string Enamn { get; set; }
        public string Fnamn { get; set; }
        public string UserId { get; set; }
        public string Civic { get; set; }

        public CoverSheetStudent()
        {
            Anonymkod = "";
            Email = "";
            Enamn = "";
            Fnamn = "";
            UserId = "";
            Civic = "";
        }
    }

    public class ScannedExam
    {
        public byte[] PdfFile { get; set; }

        public ScanningOfExamData Exam { get; set; }

        public string Error { get; set; }

        public ScannedExam()
        {
            Exam = new ScanningOfExamData();
            Error = "";
            PdfFile = null;
        }
    }

    public class SearchResultScanningOfExam
    {
        public List<ScanningOfExamData> Exams { get; set; }

        public string Error { get; set; }

        /// <summary>
        /// Intern körtid i millisekunder på alcomservern
        /// </summary>
        public int alcomInternalRunningTime { get; set; }


        public SearchResultScanningOfExam()
        {
            Exams = new List<ScanningOfExamData>();
            Error = "";
            alcomInternalRunningTime = 0;

        }
    }
    public class ScanningOfExamData
    {
        public string Anonymkod { get; set; }
        public string Email { get; set; }
        public string Enamn { get; set; }
        public string Fnamn { get; set; }
        public string Civic { get; set; }
        public string Courseid { get; set; }
        public string CourseName { get; set; }
        public string ExamCode { get; set; }
        public DateTime ExamDate { get; set; }
        public string ExamName { get; set; }

        public long AlcomFileId { get; set; }
        public DateTime AlcomScannedDate { get; set; }



        public ScanningOfExamData()
        {
            Anonymkod = "";
            Email = "";
            Enamn = "";
            Fnamn = "";
            Civic = "";

            Courseid = "";
            CourseName = "";
            ExamCode = "";
            ExamDate = DateTime.MinValue;
            ExamName = "";

            AlcomFileId = 0;
            AlcomScannedDate = DateTime.MinValue;
        }
    }


}
