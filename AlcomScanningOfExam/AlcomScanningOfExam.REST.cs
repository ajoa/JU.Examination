using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace AlcomScanningOfExam.REST
{
    public class CoverSheet
    {

        public CoverSheet()
        {
        }

        public bool SetCoverSheet(CoverSheetData csd)
        {
            /*
            {
              "anonymous": false,
              "printStudents": true,
              "hidePnr": false,
              "useSeating": true,
              "dividedRooms": {
                "rooms": [
                  {
                    "roomName": "Salsnamnet",
                    "numberOfStudents": 3,
                    "seatingStart": 1
                  }
                ]
              },
              "seatingNumberStart": 1,
              "useDividedRooms": false,
              "examSheetData": {
                "c_name": "Elteknik",
                "courseNamePresentation": "Elteknik full name",
                "c_code": "ETA101",
                "e_code": "1000",
                "e_date": "2016-11-08",
                "e_time": "13:05 - 16:00",
                "e_name": "Elteknik",
                "students": [
                  {
                    "s_anoncode": "",
                    "anonymousCodeOriginal": "EA0001",
                    "s_fname": "Jocke",
                    "s_ename": "Pihlström",
                    "s_pnr": "121212-1234",
                    "s_id": "jopi0001",
                    "s_email": "jocke.pihlstrom@alcom.se"
                  },
                ]
              },
              "miscellaneous": {
                "optionalTextObjects": [
                  {
                    "xmlTagName": "fritext",
                    "text": "En fritext rad som dyker upp på närvarolistan",
                    "printOnPage": 0
                  },
                  {
                    "xmlTagName": "e_Room",
                    "text": "Salsnamnet",
                    "printOnPage": 0
                  },
                  {
                    "xmlTagName": "e_Registered",
                    "text": "3",
                    "printOnPage": 0
                  }
                ]
              }
            }
            */

            string requestBody = "";
            requestBody += "{";
            requestBody += String.Format("\"anonymous\": {0},", "false");
            requestBody += String.Format("\"printStudents\": {0},", "true");
            requestBody += String.Format("\"hidePnr\": {0},", "false");
            requestBody += String.Format("\"useSeating\": {0},", "true");
            //requestBody += String.Format("\"dividedRooms\": {{ \"rooms\": [ {{ \"roomName\": \"{0}\", \"numberOfStudents\": {1}, \"seatingStart\": {2} }} ] }},", csd.Room, csd.Students.Count, "1");
            requestBody += String.Format("\"seatingNumberStart\": {0},", "1");
            requestBody += String.Format("\"useDividedRooms\": {0},", "false");
            requestBody += "\"examSheetData\": {";
            requestBody += String.Format("\"c_name\": \"{0}\",", replaceJsonChar(csd.CoursenameFull));
            requestBody += String.Format("\"courseNamePresentation\": \"{0}\",", replaceJsonChar(csd.CoursenamePresentation));
            requestBody += String.Format("\"c_code\": \"{0}\",", csd.Courseid);
            requestBody += String.Format("\"e_code\": \"{0}\",", csd.ExamCode);
            requestBody += String.Format("\"e_date\": \"{0}\",", csd.ExamDate);
            requestBody += String.Format("\"e_time\": \"{0}\",", csd.ExamTime);
            requestBody += String.Format("\"e_name\": \"{0}\",", replaceJsonChar(csd.ExamName));
            //requestBody += String.Format("\"e_location\": \"{0}\",", csd.Location);
            //requestBody += String.Format("\"e_guards\": \"{0}\",", csd.Guards);
            requestBody += String.Format("\"e_room\": \"{0}\",", csd.Room);
            //requestBody += String.Format("\"ladok_idnr\":null"); //\"{0}\",", csd.ExamTime);
            requestBody += "\"students\":  [";
            foreach (CoverSheetStudent student in csd.Students)
            {
                requestBody += "{";
                requestBody += String.Format("\"s_anoncode\": \"{0}\",", student.Anonymkod);
                requestBody += String.Format("\"anonymousCodeOriginal\": \"{0}\",", "");
                requestBody += String.Format("\"s_fname\": \"{0}\",", replaceJsonChar(student.Fnamn));
                requestBody += String.Format("\"s_ename\": \"{0}\",", replaceJsonChar(student.Enamn));
                requestBody += String.Format("\"s_pnr\": \"{0}\",", replaceJsonChar(student.Civic));
                requestBody += String.Format("\"s_id\": \"{0}\",", student.UserId);
                requestBody += String.Format("\"s_email\": \"{0}\",", replaceJsonChar(student.Email));
                requestBody += "},";
            }
            requestBody += "]"; // Students
            requestBody += "},"; // examSheetData
            requestBody += "\"miscellaneous\": {";
            requestBody += "\"optionalTextObjects\": [";
            requestBody += String.Format("{{ \"xmlTagName\": \"{0}\", \"text\": \"{1}\", \"printOnPage\": {2} }},", "fritext", replaceJsonChar(csd.FreeText), "0");
            //requestBody += String.Format("{{ \"xmlTagName\": \"{0}\", \"text\": \"{1}\", \"printOnPage\": {2} }},", "e_Room", csd.Room, "0");
            requestBody += String.Format("{{ \"xmlTagName\": \"{0}\", \"text\": \"{1}\", \"printOnPage\": {2} }},", "e_Registered", csd.Students.Count, "0");
            requestBody += "]"; // optionalTextObjects
            requestBody += "},"; // miscellaneous
            requestBody += "}";


            requestBody = requestBody.Replace(",}", "}");
            requestBody = requestBody.Replace("},]", "}]");
            requestBody = requestBody.Replace("},}", "}}");

            try
            {
                alcomRestAccess restAccess = new alcomRestAccess();
                alcomRestResponse restResponse = restAccess.httpPost("https://windreamsrv.win.hj.se/rest/api/print/examsheet", requestBody);

                if (restResponse.Ok)
                {
                    // Att läsa Json 
                    var jss = new JavaScriptSerializer();
                    jss.MaxJsonLength = Int32.MaxValue;
                    dynamic data = jss.Deserialize<dynamic>(restResponse.Respons);


                    if (null != data["fileAsBase64"] && 0 < data["fileAsBase64"].Length)
                    {
                        //result.PdfFile = Encoding.ASCII.GetBytes(data["fileAsBase64"]);
                        csd.PdfFile = Convert.FromBase64String(data["fileAsBase64"]);
                        csd.ErrorMsg = "";
                    }
                    else
                    {
                        csd.ErrorMsg = "Något gick fel vid hämtning av Pdf-fil";
                    }
                }
                else
                {
                    // något fel;
                    csd.ErrorMsg = " (HTTP status code: " + restResponse.httpResponsCode + ")";
                    try {
                        var jss = new JavaScriptSerializer();
                        jss.MaxJsonLength = Int32.MaxValue;
                        dynamic data = jss.Deserialize<dynamic>(restResponse.Respons);
                        if (null != data["statusMessage"] && 0 < data["statusMessage"].Length)
                            csd.ErrorMsg += " Message:" + data["statusMessage"];
                    }
                    catch { }

                    return false;
                }
            }
            catch (Exception e)
            {
                csd.ErrorMsg = e.Message;
                return false;
            }
            return true;
        }

        private string replaceJsonChar(string jsonValue)
        {
            /*
                \b  Backspace (ascii code 08)
                \f  Form feed (ascii code 0C)
                \n  New line
                \r  Carriage return
                \t  Tab
                \v  Vertical tab
                \'  Apostrophe or single quote
                \"  Double quote
                \\  Backslash caracter
            */
            return jsonValue.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "'");
        }
    }

    public class ScanningOfExam
    {
        public SearchResultScanningOfExam StudentExam(string Civic)
        {
            /*
                {
                  "objectTypes": [
                    "ScannedExam"
                  ],
                  "searchIndiceses": [
                    {
                      "index": "s_pnr",
                      "value": "121212-1212",
                      "useWildcard": false
                    }
                  ],
                  "includeDocumentIndicesesInResponse": true,
                  "includeSystemIndicesesInResponse": false,
                  "useDatesInSearch": false
                }            
            */
            string requestBody = "";
            requestBody += "{";
            requestBody += "\"objectTypes\": [ \"ScannedExam\" ],";
            requestBody += "\"searchIndiceses\": [";
            requestBody += "{";
            requestBody += "\"index\": \"s_pnr\",";
            requestBody += String.Format("\"value\": \"{0}\",", Civic);
            requestBody += "\"useWildcard\": false";
            requestBody += "}";
            requestBody += "],";
            requestBody += "\"includeDocumentIndicesesInResponse\": true,";
            requestBody += "\"includeSystemIndicesesInResponse\": false,";
            requestBody += "\"useDatesInSearch\": false";
            requestBody += "}";

            return StudentExamSearch("http://windreamsrv.win.hj.se/rest/api/windream/search/documents", requestBody);

        }

        public SearchResultScanningOfExam StudentExam(string Civic, string Courseid)
        {
            /*
                {
                  "objectTypes": [
                    "ScannedExam"
                  ],
                  "searchIndiceses": [
                    {
                      "index": "s_pnr",
                      "value": "121212-1212",
                      "useWildcard": false
                    },
                    {
                      "index": "c_code",
                      "value": "ABC123",
                      "useWildcard": false
                    }
                  ],
                  "includeDocumentIndicesesInResponse": true,
                  "includeSystemIndicesesInResponse": false,
                  "useDatesInSearch": false
                }            
            */
            string requestBody = "";
            requestBody += "{";
            requestBody += "\"objectTypes\": [ \"ScannedExam\" ],";
            requestBody += "\"searchIndiceses\": [";
            requestBody += "{";
            requestBody += "\"index\": \"s_pnr\",";
            requestBody += String.Format("\"value\": \"{0}\",", Civic);
            requestBody += "\"useWildcard\": false";
            requestBody += "},";
            requestBody += "{";
            requestBody += "\"index\": \"c_code\",";
            requestBody += String.Format("\"value\": \"{0}\",", Courseid);
            requestBody += "\"useWildcard\": false";
            requestBody += "}";
            requestBody += "],";
            requestBody += "\"includeDocumentIndicesesInResponse\": true,";
            requestBody += "\"includeSystemIndicesesInResponse\": false,";
            requestBody += "\"useDatesInSearch\": false";
            requestBody += "}";

            return StudentExamSearch("http://windreamsrv.win.hj.se/rest/api/windream/search/documents", requestBody);
        }

        public SearchResultScanningOfExam StudentExam(DateTime from, DateTime to, bool includeTimeInSearch = true)
        {
            /*
                {
                  "objectTypes": [ "ScannedExam" ],
                  "includeDocumentIndicesesInResponse": true,
                  "includeSystemIndicesesInResponse": false,
                  "useDatesInSearch": true,
                  "searchStartDate": "2016-08-01",
                  "searchEndDate": "2016-10-31"
                }
            */
            string requestBody = "";
            requestBody += "{";
            requestBody += "\"objectTypes\": [ \"ScannedExam\" ],";
            requestBody += "\"includeDocumentIndicesesInResponse\": true,";
            requestBody += "\"includeSystemIndicesesInResponse\": false,";
            requestBody += "\"useDatesInSearch\": true,";
            if(includeTimeInSearch)
            {
                requestBody += String.Format("\"searchStartDate\": \"{0}\",", dateString(from, true));
                requestBody += String.Format("\"searchEndDate\": \"{0}\"", dateString(to, true));
            }
            else
            {
                requestBody += String.Format("\"searchStartDate\": \"{0}T00:00:00\",", dateString(from, false));
                requestBody += String.Format("\"searchEndDate\": \"{0}T00:00:00\"", dateString(to, false));
            }
            requestBody += "}";

            return StudentExamSearch("http://windreamsrv.win.hj.se/rest/api/windream/search/documents", requestBody);

            //return StudentExamSearch("http://windreamsrv.win.hj.se/rest/api/search/windream/date/" + dateString(from), requestBody);

        }

        private string dateString(DateTime date, bool includeHoursMinutesSecondsInAlcomFormat)
        {
            string dateAndTime = date.Year + "-" + date.Month.ToString().PadLeft(2, '0') + "-" + date.Day.ToString().PadLeft(2, '0'); 
            if(includeHoursMinutesSecondsInAlcomFormat)
            {
                dateAndTime += "T";
                dateAndTime += date.ToString("HH:mm:ss");
            }
            return dateAndTime;
        }

        private SearchResultScanningOfExam StudentExamSearch(string url, string requestBody)
        {
            SearchResultScanningOfExam result = new SearchResultScanningOfExam();

            try
            {
                alcomRestAccess restAccess = new alcomRestAccess();
                alcomRestResponse restResponse = null;

                if(requestBody.Length == 0)
                {
                    restResponse = restAccess.httpGet(url);
                }
                else
                {
                    restResponse = restAccess.httpPost(url, requestBody);
                }

                if (restResponse.Ok)
                {
                    // Att läsa Json 
                    var jss = new JavaScriptSerializer();
                    jss.MaxJsonLength = int.MaxValue;
                    dynamic data = jss.Deserialize<dynamic>(restResponse.Respons);

                    try {
                        result.alcomInternalRunningTime = Convert.ToInt32(Math.Round(data["searchResponseTimeInSeconds"] * 1000, 0));
                    } catch
                    {
                        result.alcomInternalRunningTime = -1;
                    }


                    if (url.Contains("rest/api/windream"))
                    {
                        data = data["documentSearchResults"];
                    }

                    if (data != null)
                    {

                        for (int i = 0; i < data.Length; i++)
                        {
                            ScanningOfExamData scanningOfExam = new ScanningOfExamData();
                            scanningOfExam.AlcomFileId = data[i]["fileId"];
                            scanningOfExam.AlcomScannedDate = Convert.ToDateTime(data[i]["createDate"]);

                            string indIndicesesName = "objectIndiceses";
                            if (url.Contains("rest/api/windream"))
                            {
                                indIndicesesName = "documentIndiceses";
                            }


                            foreach (dynamic ind in data[i][indIndicesesName])
                            {

                                string indval = "";
                                if (null != ind["value"])
                                {
                                    indval = ind["value"].ToString();
                                }

                                string key = (Convert.ToString(ind["index"])).ToLower();

                                switch (key)
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

                            result.Exams.Add(scanningOfExam);
                        }
                    }
                }
                else
                {
                    // något fel;
                    result.Error = " (HTTP status code: " + restResponse.httpResponsCode + ")";
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
                string url = String.Format("http://windreamsrv.win.hj.se/rest/api/windream/file/{0}/false", examId);
                if(getAlsoStudentExamInfoAlthoughAbitSlower)
                {
                    url = String.Format("http://windreamsrv.win.hj.se/rest/api/windream/file/{0}/true", examId);
                }

                alcomRestAccess restAccess = new alcomRestAccess();
                alcomRestResponse restResponse = null;
                restResponse = restAccess.httpGet(url);

                if (restResponse.Ok)
                {
                    // Att läsa Json 
                    var jss = new JavaScriptSerializer();
                    jss.MaxJsonLength = Int32.MaxValue;
                    dynamic data = jss.Deserialize<dynamic>(restResponse.Respons);


                    if (null != data["fileAsBase64"] && 0 < data["fileAsBase64"].Length)
                    {
                        //result.PdfFile = Encoding.ASCII.GetBytes(data["fileAsBase64"]);
                        result.PdfFile = Convert.FromBase64String(data["fileAsBase64"]);
                        result.Error = "";
                    }
                    else
                    {
                        result.Error = "Något gick fel vid hämtning av Pdf-fil";
                    }

                    ScanningOfExamData scanningOfExam = new ScanningOfExamData();
                    scanningOfExam.AlcomFileId = data["fileId"];
                    if (getAlsoStudentExamInfoAlthoughAbitSlower)
                    {
                        if (null != data["objectIndiceses"])
                        {
                            foreach (dynamic ind in data["objectIndiceses"])
                            {

                                string indval = "";
                                if (null != ind["value"])
                                {
                                    indval = ind["value"].ToString();
                                }

                                string key = (Convert.ToString(ind["index"])).ToLower();

                                switch (key)
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
                        }
                    }
                    result.Exam = scanningOfExam;
                }
                else
                {
                    // något fel;
                    result.Error = " (HTTP status code: " + restResponse.httpResponsCode + ")";
                }
            }
            catch (Exception e)
            {
                result.Error = e.Message;
            }

            return result;
        }
    }


    internal class alcomRestAccess
    {
        public alcomRestResponse httpPost(string _url, string _requestBody)
        {
            return httpPostOrGet("POST", _url, _requestBody);
        }

        public alcomRestResponse httpGet(string _url)
        {
            return httpPostOrGet("GET", _url, "");
        }

        private alcomRestResponse httpPostOrGet(string method, string _url, string requestBody)
        {
            try
            {
                // Använder HttpWebRequest för att enklara komma åt statuskoden.. annars är det smidgare att använda webClient (DownloadString)
                // Och för att kunna hanterar timeout
                System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(_url);
                httpWebRequest.Method = (method == "POST" ? System.Net.WebRequestMethods.Http.Post : System.Net.WebRequestMethods.Http.Get);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Timeout = 1000 * 120; // två minuter behövs för försättsblad

                // Att request body
                if (method == "POST")
                {
                    byte[] buf = Encoding.UTF8.GetBytes(requestBody);
                    httpWebRequest.ContentLength = buf.Length;
                    httpWebRequest.GetRequestStream().Write(buf, 0, buf.Length);
                }

                try
                {
                    using (System.Net.HttpWebResponse httpWebResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        System.IO.Stream resStream = httpWebResponse.GetResponseStream();
                        System.IO.StreamReader reader = new System.IO.StreamReader(resStream);
                        string responsContent = reader.ReadToEnd();
                        int responseCode = (int)httpWebResponse.StatusCode;

                        if (responseCode == 200)
                        {
                            // Ett ok svar. Kolla datat

                            // Att läsa Json 
                            /* var jss = new JavaScriptSerializer();
                            dynamic data = jss.Deserialize<dynamic>(responsContent);
                            string latestterm = data["checkstudent"][currPnr].ContainsKey("latestterm") && data["checkstudent"][currPnr]["latestterm"] != null ? data["checkstudent"][currPnr]["latestterm"] : ""; */
                            return new alcomRestResponse() { Ok = true, httpResponsCode = 200, Respons = responsContent };
                        }
                        else
                        {
                            // Vi fick något annat än statuskod 200 men inget fel kastat
                            return new alcomRestResponse() { Ok = false, httpResponsCode = responseCode, Respons = responsContent };
                        }
                    }
                }
                catch (System.Net.WebException webException)
                {
                    int errResponseCode = 0;
                    string errResponseText = "";

                    if (webException.Response != null)
                    {
                        System.Net.HttpWebResponse httpWebExceptionResponse = (System.Net.HttpWebResponse)webException.Response;

                        System.IO.Stream resStream = httpWebExceptionResponse.GetResponseStream();
                        System.IO.StreamReader reader = new System.IO.StreamReader(resStream);
                        errResponseText = reader.ReadToEnd();
                        errResponseCode = (int)httpWebExceptionResponse.StatusCode;
                    }
                    else
                    {
                        // Kunde inte kontakta servern, felaktig url? servern nere? 
                        errResponseCode = 0;
                        errResponseText = "Kunde inte kontakta servern. Message:" + webException.Message;
                    }
                    return new alcomRestResponse() { Ok = false, httpResponsCode = errResponseCode, Respons = errResponseText };
                }
            }
            catch (Exception Err)
            {
                return new alcomRestResponse() { Ok = false, httpResponsCode = 0, Respons = Err.Message };
            }
        }
    }

    internal class alcomRestResponse
    {
        public bool Ok { get; set; }
        public string Respons { get; set; }
        public int httpResponsCode { get; set; }

        public alcomRestResponse()
        {
            this.Ok = true;
            this.Respons = "";
            this.httpResponsCode = 0;
        }

    }

}
