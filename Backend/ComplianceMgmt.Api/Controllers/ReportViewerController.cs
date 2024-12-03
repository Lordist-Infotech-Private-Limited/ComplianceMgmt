using BoldReports.Web.ReportViewer;
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]/[action]")]
    [Microsoft.AspNetCore.Cors.EnableCors("AllowSpecificOrigin")]
    public class ReportViewerController(IMemoryCache memoryCache,
        IWebHostEnvironment hostingEnvironment, ComplianceMgmtDbContext context) : ControllerBase, IReportController
    {

        // Post action to process the report from server based json parameters and send the result back to the client.
        [HttpPost]
        public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
        {
            try
            {
                Console.WriteLine("Processing report action...");
                if (jsonArray == null)
                {
                    Console.WriteLine("Received null JSON payload.");
                    throw new ArgumentNullException(nameof(jsonArray), "The payload cannot be null.");
                }

                HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
                return ReportHelper.ProcessReport(jsonArray, this, memoryCache);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostReportAction: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw; // Allow the exception to propagate for logging or debugging.
            }
        }



        // Method will be called to initialize the report information to load the report with ReportHelper for processing.
        [NonAction]
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            var connection = context.CreateConnection();
            var query = @"SELECT * FROM statemaster";
            var states = connection.Query<StateMaster>(query);
            reportOption.ReportModel.ProcessingMode = ProcessingMode.Local;
            string basePath = Path.Combine(hostingEnvironment.WebRootPath, "Resources");
            string reportPath = Path.Combine(basePath, reportOption.ReportModel.ReportPath);
            FileStream fileStream = new(reportPath, FileMode.Open, FileAccess.Read);
            MemoryStream stream = new();
            fileStream.CopyTo(stream);
            stream.Position = 0;
            stream.Close();
            reportOption.ReportModel.Stream = stream;
            reportOption.ReportModel.DataSources.Add(new BoldReports.Web.ReportDataSource { Name = "DataSource", Value = states });
            //string reportPath = Path.Combine(hostingEnvironment.WebRootPath, "StateReport.rdl");

            //if (!System.IO.File.Exists(reportPath))
            //{
            //    throw new FileNotFoundException("Report file not found at path: " + reportPath);
            //}

            //FileStream reportStream = new(reportPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            //reportOption.ReportModel.Stream = reportStream;

            //Console.WriteLine($"Report initialized with path: {reportPath}");
        }

        // Method will be called when reported is loaded with internally to start to layout process with ReportHelper.
        [NonAction]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
        }

        //Get action for getting resources from the report
        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        // Method will be called from Report Viewer client to get the image src for Image report item.
        public object GetResource(ReportResource resource)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000"); // Set the allowed origin
            return ReportHelper.GetResource(resource, this, memoryCache);
        }

        [HttpPost]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, memoryCache);
        }
    }
}
