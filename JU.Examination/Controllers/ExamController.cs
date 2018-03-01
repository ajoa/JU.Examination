using JU.ADAccess;
using JU.ADAccess.Model;
using JU.Examination.CustomExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JU.Examination.Controllers
{
    public class ExamController : Controller
    {

        [Authorize(Roles = "Student,Admin,Staff")]
        public ActionResult Student(string lang = "Swe")
        {
            bool inEnglish = (null == lang ? false : (lang.ToUpper().StartsWith("E") ? true : false));

            ViewModel.ExamViewModel examViewModel = new ViewModel.ExamViewModel();

            // Retrieves civic number for logged in user
            string civicNumber = "";
            try
            {
                civicNumber = getCivicNumberForLoggedInUser(inEnglish);
                civicNumber = fixCivicNumberForAlcom(civicNumber, inEnglish);
            }
            catch (Exception e)
            {
                examViewModel.Message.Status = ViewModel.MessageStatusViewModel.ERROR;
                examViewModel.Message.Title = inEnglish ? "Error retrieving civic number" : "Fel vid hämtning av personnummer";
                examViewModel.Message.Text = e.Message;
                // We get an error, let's return
                return View(examViewModel);
            }

            examViewModel = getSStudent(civicNumber, inEnglish);
            return View(examViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public ActionResult Staff(string lang = "Swe")
        {
            ViewBag.civicorsignature = "";
            bool inEnglish = (null == lang ? false : (lang.ToUpper().StartsWith("E") ? true : false));

            ViewModel.ExamViewModel examViewModel = new ViewModel.ExamViewModel();
            return View(examViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public ActionResult Staff(string civicorsignature = "", string lang = "Swe")
        {
            if (String.IsNullOrEmpty(civicorsignature) || String.IsNullOrWhiteSpace(civicorsignature))
            {
                return RedirectToAction("Staff", "Exam", new { lang = lang });
            }

            civicorsignature = civicorsignature.Trim();
            ViewBag.civicorsignature = civicorsignature;

            ViewModel.ExamViewModel examViewModel = new ViewModel.ExamViewModel();
            bool inEnglish = (null == lang ? false : (lang.ToUpper().StartsWith("E") ? true : false));
            string civicNumber = ""; // We can only search civic number
                        
            try
            {
                // Determines if it is a civic number or signature
                if (civicorsignature.CivicNumberToLadokFormat().CanBeLadokCivicNumber())
                {
                    // Well, that seems to be one civic number
                    civicNumber = fixCivicNumberForAlcom(civicorsignature, inEnglish);
                }
                else
                {
                    // Well maybe that's a signature
                    civicNumber = getCivicNumberFornUser(civicorsignature, inEnglish);
                    civicNumber = fixCivicNumberForAlcom(civicNumber, inEnglish);
                }
            }
            catch (Exception e)
            {
                examViewModel.Message.Status = ViewModel.MessageStatusViewModel.ERROR;
                examViewModel.Message.Title = inEnglish ? "Error in evaluate search value" : "Fel vid tolkande av sökvärde";
                examViewModel.Message.Text = e.Message;
                // We get an error, let's return
                return View(examViewModel);
            }

            examViewModel = getSStudent(civicNumber, inEnglish);
            return View(examViewModel);
        }

        [Authorize]
        public ActionResult GetExamPdf(int exam, bool download = true, string lang = "Swe")
        {
            bool inEnglish = (null == lang ? false : (lang.ToUpper().StartsWith("E") ? true : false));

            // Retrieves civic number for logged in user
            string civicNumber = "";
            try
            {
                civicNumber = getCivicNumberForLoggedInUser(inEnglish);
                civicNumber = fixCivicNumberForAlcom(civicNumber, inEnglish);
            }
            catch (Exception e)
            {
                ViewModel.ExamViewModel examViewModel = new ViewModel.ExamViewModel();
                examViewModel.Message.Status = ViewModel.MessageStatusViewModel.ERROR;
                examViewModel.Message.Title = inEnglish ? "Error retrieving civic number" : "Fel vid hämtning av personnummer";
                examViewModel.Message.Text = e.Message;
                // We get an error, let's return
                return View((User.IsInRole("Student") ? "Student" : "Staff"), examViewModel);
            }

            try
            {
                AlcomScanningOfExam.REST.ScanningOfExam searchScanningOfExam = new AlcomScanningOfExam.REST.ScanningOfExam();
                AlcomScanningOfExam.ScannedExam scannedExam = searchScanningOfExam.GetScannedExam(exam, true);

                // kontrollerar att ingen fuskar.
                if (User.IsInRole("Student"))
                {
                    civicNumber = civicNumber.Replace("-", "");
                    string examCivic = scannedExam.Exam.Civic.Replace("-", "");
                    if (!civicNumber.Equals(examCivic, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception((inEnglish ? "Not allowed to view exam for " : "Ej behörig att se tenta för ") + civicNumber.CivicNumberToDisplayFormat() +  "!");
                    }
                }

                if (scannedExam.Error.Length == 0)
                {
                    // loggar
                    string logDescription = String.Format("Pnr={0}, ExamCode={1}, ExamDate={2}, AlcomFileId={3}", civicNumber, scannedExam.Exam.ExamCode, scannedExam.Exam.ExamDate.ToString("yyyy-MM-dd"), scannedExam.Exam.AlcomFileId);
                    JU.ApplicationSupport.Log dbLog = new ApplicationSupport.Log();
                    dbLog.AddLog("JU.Examination", ApplicationSupport.Log.LogType.INFO, "VIEW_SCANNED_EXAM", "AlcomExamId_" + exam, ApplicationSupport.Log.ExternalIdType.NO_REF, logDescription, User.Identity.Name);

                    if (download)
                    {
                        // Laddar ner
                        string sFileName = "Tenta_" + exam.ToString() + scannedExam.Exam.ExamDate.ToString("yyyy-MM-dd") + ".pdf";
                        byte[] byteArray = scannedExam.PdfFile;
                        Stream stream = new MemoryStream(byteArray);
                        stream.Flush();
                        stream.Position = 0;
                        return File(stream, "application/pdf", Server.UrlEncode(sFileName));
                    }
                    else
                    {
                        // Visar direkt i browsern
                        byte[] byteArray = scannedExam.PdfFile;
                        MemoryStream pdfStream = new MemoryStream();
                        pdfStream.Write(byteArray, 0, byteArray.Length);
                        pdfStream.Position = 0;
                        return new FileStreamResult(pdfStream, "application/pdf");
                    }
                }
                else
                {
                    throw new Exception(scannedExam.Error);
                }
            }
            catch (Exception e)
            {
                //Ta hand om eventuellt exception
                ViewModel.ExamViewModel examViewModel = new ViewModel.ExamViewModel();
                examViewModel.Message.Status = ViewModel.MessageStatusViewModel.ERROR;
                examViewModel.Message.Title = inEnglish ? "Error loading scanned exam" : "Fel vid hämtande av skannad tenta";
                examViewModel.Message.Text = e.Message;
                // We get an error, let's return
                return View((User.IsInRole("Student") ? "Student" : "Staff"), examViewModel);
            }
        }

        private string fixCivicNumberForAlcom(string civicnumber, bool inEnglish)
        {
            string alcomCivicnumberFormat = civicnumber.CivicNumberToDisplayFormat();

            if (11 != alcomCivicnumberFormat.Length)
            {
                throw new Exception(!inEnglish ? "Felaktigt personnummer (" + civicnumber + ")." : "Incorrect civic number (" + civicnumber + ")");
            }

            return alcomCivicnumberFormat;
        }

        private string getCivicNumberForLoggedInUser(bool inEnglish)
        {
            string userName = User.Identity.Name;
            if (String.IsNullOrEmpty(userName))
            {
                throw new Exception((inEnglish ? "Ej inloggad?" : "Not logged in?"));
            }

            string civicNumber = getCivicNumberFornUser(userName, inEnglish);
            return civicNumber;
        }

        private string getCivicNumberFornUser(string userName,  bool inEnglish)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new Exception((inEnglish ? "Användarnamn saknas" : "Username is missing")); 
            }

            string civicNumber = "";

            IUserManager userManager = new ADUserManager();
            IUser user = userManager.GetUserByUsername(userName);
            if (null == user)
            {
                throw new Exception((!inEnglish ? "Fel användarnamn (" : "Wrong user name (") + userName + ")." +
                     (!inEnglish ? " Kan inte återfinnas i användardatabasen." : " Can not be found in the user database.")); // Detta är väl osanolikt
            }

            IReadOnlyDictionary<string, string> attributes = user.ADAttributes;
            civicNumber = attributes.FirstOrDefault(a => a.Key.Equals("employeeNumber")).Value;
            if (null == civicNumber || civicNumber.Length == 0)
            {
                throw new Exception(!inEnglish ? "Användarkontot (" + userName + ") har inga personnummer, kontakta helpdesk." : "The User account (" + userName + ") has no civic number, contact the helpdesk"); // 404 användarnamnet ej funnet
            }

            return civicNumber;

        }

        private ViewModel.ExamViewModel getSStudent(string civicnumber, bool inEnglish)
        {
            ViewModel.ExamViewModel examViewModel = new ViewModel.ExamViewModel();

            try
            {
                AlcomScanningOfExam.REST.ScanningOfExam searchScanningOfExam = new AlcomScanningOfExam.REST.ScanningOfExam();
                examViewModel.ScanningOfExams = searchScanningOfExam.StudentExam(civicnumber);

                if (examViewModel.ScanningOfExams.Error.Length > 0)
                {
                    examViewModel.Message.Status = ViewModel.MessageStatusViewModel.ERROR;
                    examViewModel.Message.Title = inEnglish ? "Error reading Scanned exams" : "Fel vid läsning av skannade tentor";
                    examViewModel.Message.Text = examViewModel.ScanningOfExams.Error;
                }

                if (examViewModel.ScanningOfExams.Exams.Count > 0)
                {
                    examViewModel.ScanningOfExams.Exams = examViewModel.ScanningOfExams.Exams.OrderByDescending(e => e.ExamDate).ToList();
                }
            }
            catch(Exception e)
            {
                examViewModel.Message.Status = ViewModel.MessageStatusViewModel.ERROR;
                examViewModel.Message.Title = inEnglish ? "Exception reading Scanned exams" : "Fel vid läsning av skannade tentor";
                examViewModel.Message.Text = e.Message;
            }

            return examViewModel;
        }

    }
}