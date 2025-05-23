using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Adventure19.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        [HttpGet]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionFeature?.Error;

            if (ex != null)
            {

                // Percorso fisso per i log
                var logDirectory = @"C:\Users\Betacom\errori-ridewild";
                Directory.CreateDirectory(logDirectory);

                var filePath = Path.Combine(logDirectory, "errorlog.csv");

                // Scrivi intestazione solo se il file non esiste
                if (!System.IO.File.Exists(filePath))
                {
                    var header = "Time,User,Message,Source,TargetSite,HResult";
                    System.IO.File.AppendAllText(filePath, header + System.Environment.NewLine);
                }

                // Funzione per gestire escape CSV
                string EscapeCsv(string s)
                {
                    if (string.IsNullOrEmpty(s))
                        return "";
                    if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                        s = $"\"{s.Replace("\"", "\"\"")}\"";
                    return s;
                }

                var csvLine = string.Join(",",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    EscapeCsv(User?.Identity?.Name ?? "Anonymous"),
                    EscapeCsv(ex.Message),
                    EscapeCsv(ex.Source),
                    EscapeCsv(ex.TargetSite?.Name),
                    ex.HResult.ToString()
                );

                System.IO.File.AppendAllText(filePath, csvLine + System.Environment.NewLine);
            }

            return Problem(detail: ex?.Message);
        }
    }
}
