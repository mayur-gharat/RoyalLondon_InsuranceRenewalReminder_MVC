using EventLogHandler;
using InsuranceRenewalReminder;
using InsuranceRenewalTypes;
using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.IO;  


namespace InsuranceRenewalReminder_MVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase flupload)
        {
            EventLogger Logger = new EventLogger();
            try
            {
                if (flupload != null && flupload.ContentLength > 0)
                {
                    //Evaluate Input file path
                    string FileName = Path.GetFileName(flupload.FileName);
                    string FilePath = Server.MapPath(WebConfigurationManager.AppSettings["InputFilePath"] + "\\" + FileName);

                    //Upload input file
                    flupload.SaveAs(FilePath);

                    //Generate Files
                    System.IO.FileInfo InputFile = new System.IO.FileInfo(FilePath);
                    if (InputFile.Exists)
                    {
                        UIHelper Helper = new UIHelper();
                        ResponseBase result = Helper.CreateOutputFiles(Helper.GetInputFields(FilePath));

                        //Show notification
                        if (result == null)
                        {
                            ViewBag.Message = "Output file creation failed!!";
                            ViewBag.ResponseCode = -1;
                        }
                        else if (result.ReturnCode < 0)
                        {
                            ViewBag.Message = result.ReturnMessage;
                            ViewBag.ResponseCode = -1;
                        }
                        else if (result.ReturnCode > 0)
                        {
                            ViewBag.Message = result.ReturnMessage;
                            ViewBag.ResponseCode = 1;
                        }
                        else
                        {
                            ViewBag.Message = "Success!! Files generated at " + Server.MapPath(WebConfigurationManager.AppSettings["OutputFilePath"]);
                            if (!string.IsNullOrWhiteSpace(result.ReturnMessage))
                            {
                                ViewBag.Message = ViewBag.Message + "<br/>" + result.ReturnMessage;
                            }
                            ViewBag.ResponseCode = 0;
                        }

                        //Delete uploaded file after finish
                        InputFile.Delete();
                    }
                    else
                    {
                        ViewBag.Message = "Error while uploading file, Please select appropreate Input file.";
                        ViewBag.ResponseCode = -1;
                    }
                }
                else
                {
                    ViewBag.Message = "Please select appropreate Input file.";
                    ViewBag.ResponseCode = -1;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Index::[HttpPost]", ex);
                ViewBag.Message = "Error while creating output file!";
                ViewBag.ResponseCode = -1;
            }
            
            return View();
        }

    }
}