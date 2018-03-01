using System;
using System.Collections.Generic;

namespace AlcomScanningOfExam.WS
{
    public class CoverSheet
    {
        public CoverSheet()
        {
        }


        public bool SetCoverSheet(CoverSheetData csd)
        {

            Alcom.wPrintSheetJU courseInfo = new Alcom.wPrintSheetJU();
            courseInfo.courseid = csd.Courseid;
            courseInfo.coursename_full = csd.CoursenameFull;
            courseInfo.coursename_presentation = csd.CoursenamePresentation;
            courseInfo.examcode = csd.ExamCode;
            courseInfo.examdate = csd.ExamDate;
            courseInfo.examname = csd.ExamName;
            courseInfo.examtime = csd.ExamTime;
            courseInfo.numstudent = csd.Numstudent;
            courseInfo.room = csd.Room;
            courseInfo.location = csd.Location;
            courseInfo.informationtext = csd.FreeText.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");

            //courseInfo.studentInfos = new Alcom.wStudentInfo[]();
            List<Alcom.wStudentInfo> theStudents = new List<Alcom.wStudentInfo>();
            foreach (CoverSheetStudent student in csd.Students)
            {
                Alcom.wStudentInfo studentToAdd = new Alcom.wStudentInfo();
                studentToAdd.anonymkod = student.Anonymkod;
                studentToAdd.email = student.Email;
                studentToAdd.enamn = student.Enamn;
                studentToAdd.fnamn = student.Fnamn;
                studentToAdd.id = student.UserId;
                studentToAdd.pnr = student.Civic;

                theStudents.Add(studentToAdd);
            }
            courseInfo.studentInfos = theStudents.ToArray();

            Alcom.WWS ws = new Alcom.WWS();
            string returnMessage = "";
            byte[] returnPdf = new byte[0];

            // printAnonymous, onlyPrintList, onlyPrintListHead
            if (ws.generatePrintSheetJU(courseInfo, ref returnMessage, ref returnPdf, false, true, false))
            {
                if (null != returnPdf && 0 < returnPdf.Length)
                {
                    // spara/printa/skicka pdfBytes
                    csd.PdfFile = returnPdf;
                    csd.ErrorMsg = "";
                    return true;
                }
                else
                {
                    // felmeddela generering
                    csd.ErrorMsg = "Något gick fel vid generering av Pdf-fil";
                    return false;
                }
            }
            else
            {
                // läs returnMessage;
                csd.ErrorMsg = returnMessage;
                return false;
            }
        }
    }

    public class ScanningOfExam
    {
        public SearchResultScanningOfExam StudentExam(string Civic)
        {
            Alcom.wObjectSearchInfo objInf = new Alcom.wObjectSearchInfo();
            Alcom.wObjectSearchInfo[] objInfs = new Alcom.wObjectSearchInfo[1];

            //'Indice kan vara t.ex. s_pnr eller s_uid (personnummer eller användarid)
            objInf.wIndex = "s_pnr";
            objInf.wValue = Civic;
            objInf.wWild = false;

            objInfs[0] = objInf;

            return StudentExamSearch(objInfs);
        }

        public SearchResultScanningOfExam StudentExam(string Civic, string Courseid)
        {
            Alcom.wObjectSearchInfo[] objInfs = new Alcom.wObjectSearchInfo[2];

            //'Indice kan vara t.ex. s_pnr eller s_uid (personnummer eller användarid)
            Alcom.wObjectSearchInfo objInf = new Alcom.wObjectSearchInfo();
            objInf.wIndex = "s_pnr";
            objInf.wValue = Civic;
            objInf.wWild = false;

            objInfs[0] = objInf;

            objInf = new Alcom.wObjectSearchInfo();
            objInf.wIndex = "c_code";
            objInf.wValue = Courseid;
            objInf.wWild = false;

            objInfs[1] = objInf;

            return StudentExamSearch(objInfs);
        }

        public SearchResultScanningOfExam StudentExam(DateTime from, DateTime to)
        {
            Alcom.wObjectSearchInfo objInf = new Alcom.wObjectSearchInfo();
            Alcom.wObjectSearchInfo[] objInfs = new Alcom.wObjectSearchInfo[1];

            //'Indice kan vara t.ex. s_pnr eller s_uid (personnummer eller användarid)
            // 2016-06-08 00:00:00
            objInf.wIndex = "s_pnr";
            objInf.wValue = "";
            objInf.wWild = true;

            objInfs[0] = objInf;


            return StudentExamSearch(objInfs, from, to);

        }

        private SearchResultScanningOfExam StudentExamSearch(Alcom.wObjectSearchInfo[] objInfs, DateTime? from = null, DateTime? to = null)
        {
            SearchResultScanningOfExam result = new SearchResultScanningOfExam();
            string mess = "";
            try
            {
                Alcom.WWS ws = new Alcom.WWS();

                Alcom.wFileInfo[] wfinfs = new Alcom.wFileInfo[1];
                if (from == null || to == null)
                {
                    ws.executeObjectSearch(objInfs, ref wfinfs, false, ref mess);
                }
                else
                {
                    ws.executeTimeSpanSearch(objInfs, "Created", (DateTime)from, (DateTime)to, false, ref wfinfs, ref mess);
                }

                if (null != wfinfs)
                {
                    foreach (Alcom.wFileInfo wfinf in wfinfs)
                    {
                        if (wfinf == null)
                            continue;

                        ScanningOfExamData scanningOfExam = new ScanningOfExamData();
                        //string restext = "";
                        scanningOfExam.AlcomFileId = wfinf.fileId;
                        scanningOfExam.AlcomScannedDate = wfinf.fileCreated;
                        //restext += "***** NEW HIT *****" + "< br />";
                        //restext += "File ID: " + wfinf.fileId + "< br />";
                        //restext += "File name: " + wfinf.fileName + "< br />";
                        //restext += "File created: " + wfinf.fileCreated + "< br />";
                        //restext += "File path: " + wfinf.fileWindreamPath + "< br />";

                        //restext += "## INDICES ##" + "<br />";
                        foreach (Alcom.wdInfo ind in wfinf.wdInfos)
                        {

                            string indval = "";
                            if (null != ind.indexValue)
                            {
                                indval = ind.indexValue.ToString();
                            }
                            //restext += " Indice: " + ind.indexName + " = " + indval + "<br />";

                            switch (ind.indexName.ToLower())
                            {
                                case "s_firstname":
                                    scanningOfExam.Fnamn = indval;
                                    break;
                                case "s_lastname":
                                    scanningOfExam.Enamn = indval;
                                    break;
                                case "s_pnr":
                                    scanningOfExam.Civic = indval;
                                    break;
                                case "s_anoncode":
                                    scanningOfExam.Anonymkod = indval;
                                    break;
                                case "s_email":
                                    scanningOfExam.Email = indval;
                                    break;
                                case "c_code":
                                    scanningOfExam.Courseid = indval;
                                    break;
                                case "c_name":
                                    scanningOfExam.CourseName = indval;
                                    break;
                                case "e_code":
                                    scanningOfExam.ExamCode = indval;
                                    break;
                                case "e_name":
                                    scanningOfExam.ExamName = indval;
                                    break;
                                case "e_date":
                                    try
                                    {
                                        scanningOfExam.ExamDate = Convert.ToDateTime(indval);
                                    }
                                    catch
                                    {
                                        scanningOfExam.ExamDate = DateTime.MinValue;
                                    }
                                    break;
                            }
                        }

                        //restext += "*** END OF HIT ***" + "<br />" + "<br />";
                        result.Exams.Add(scanningOfExam);
                    }
                }
                else
                {
                    // Inga träffar;
                }
            }
            catch (Exception e)
            {
                result.Error = e.Message;
            }

            return result;
        }

        public ScannedExam GetScannedExam(int examId, bool getAlsoStudentExamInfoAlthoughAbitSlower)
        {
            ScannedExam result = new ScannedExam();
            try
            {
                Alcom.WWS ws = new Alcom.WWS();
                Alcom.wFileInfo wfinf = new Alcom.wFileInfo();
                string returnMessage = "";
                byte[] returnPdf = new byte[0];

                bool statusGetFile = false;
                if (getAlsoStudentExamInfoAlthoughAbitSlower)
                {
                    statusGetFile = ws.getFileByFileIdInclMeta(examId, ref wfinf, ref returnMessage, ref returnPdf, false);
                }
                else
                {
                    statusGetFile = ws.getFileByFileId(examId, ref returnMessage, ref returnPdf, false);
                }


                if (statusGetFile)
                {
                    if (null != returnPdf && 0 < returnPdf.Length)
                    {
                        // spara/printa/skicka pdfBytes
                        result.PdfFile = returnPdf;
                        result.Error = "";
                    }
                    else
                    {
                        // felmeddela generering
                        result.Error = "Något gick fel vid hämtning av Pdf-fil";
                    }
                }
                else
                {
                    // läs returnMessage;
                    result.Error = returnMessage;
                }


                if (getAlsoStudentExamInfoAlthoughAbitSlower && null != wfinf)
                {
                    ScanningOfExamData scanningOfExam = new ScanningOfExamData();
                    //string restext = "";

                    scanningOfExam.AlcomFileId = wfinf.fileId;
                    scanningOfExam.AlcomScannedDate = wfinf.fileCreated;
                    //restext += "***** NEW HIT *****" + "< br />";
                    //restext += "File ID: " + wfinf.fileId + "< br />";
                    //restext += "File name: " + wfinf.fileName + "< br />";
                    //restext += "File created: " + wfinf.fileCreated + "< br />";
                    //restext += "File path: " + wfinf.fileWindreamPath + "< br />";

                    //restext += "## INDICES ##" + "<br />";

                    foreach (Alcom.wdInfo ind in wfinf.wdInfos)
                    {

                        string indval = "";
                        if (null != ind.indexValue)
                        {
                            indval = ind.indexValue.ToString();
                        }

                        switch (ind.indexName.ToLower())
                        {
                            case "s_firstname":
                                scanningOfExam.Fnamn = indval;
                                break;
                            case "s_lastname":
                                scanningOfExam.Enamn = indval;
                                break;
                            case "s_pnr":
                                scanningOfExam.Civic = indval;
                                break;
                            case "s_anoncode":
                                scanningOfExam.Anonymkod = indval;
                                break;
                            case "s_email":
                                scanningOfExam.Email = indval;
                                break;
                            case "c_code":
                                scanningOfExam.Courseid = indval;
                                break;
                            case "c_name":
                                scanningOfExam.CourseName = indval;
                                break;
                            case "e_code":
                                scanningOfExam.ExamCode = indval;
                                break;
                            case "e_name":
                                scanningOfExam.ExamName = indval;
                                break;
                            case "e_date":
                                try
                                {
                                    scanningOfExam.ExamDate = Convert.ToDateTime(indval);
                                }
                                catch
                                {
                                    scanningOfExam.ExamDate = DateTime.MinValue;
                                }
                                break;
                        }
                    }                        
                    
                    result.Exam = scanningOfExam;
                }
                else
                {
                    // Ingen data;
                }
            }
            catch (Exception e)
            {
                result.Error = e.Message;
            }

            return result;
        }

    }

}
