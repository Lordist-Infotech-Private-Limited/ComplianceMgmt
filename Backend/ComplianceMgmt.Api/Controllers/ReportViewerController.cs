using BoldReports.Web.ReportViewer;
using ComplianceMgmt.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

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

                //HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
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
            //reportOption.ReportModel.ReportServerCredential = new System.Net.NetworkCredential("a927ee_comlian", "P@ssw0rd");

            //reportOption.ReportModel.DataSourceCredentials.Add(new BoldReports.Web.DataSourceCredentials("DataSource", "a927ee_comlian", "P@ssw0rd"));
          
            string basePath = Path.Combine(hostingEnvironment.WebRootPath, "Resources");
            string reportPath = Path.Combine(basePath, reportOption.ReportModel.ReportPath);

            if (!System.IO.File.Exists(reportPath))
            {
                throw new FileNotFoundException("Report file not found at path: " + reportPath);
            }

            FileStream reportStream = new(reportPath, FileMode.Open, FileAccess.Read);
            reportOption.ReportModel.Stream = reportStream;

            Console.WriteLine($"Report initialized with path: {reportPath}");
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
            //Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000"); // Set the allowed origin
            return ReportHelper.GetResource(resource, this, memoryCache);
        }

        [HttpPost]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, memoryCache);
        }
    }
}
