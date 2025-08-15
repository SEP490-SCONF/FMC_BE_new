using Microsoft.AspNetCore.Mvc;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Health
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                Message = "Certificate Management System is running successfully"
            });
        }

        // GET: api/Health/check-database
        [HttpGet("check-database")]
        public IActionResult CheckDatabase()
        {
            try
            {
                var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
                var hangfireConnection = _configuration.GetConnectionString("HangfireConnection");

                return Ok(new
                {
                    Status = "Database connections configured",
                    DefaultConnection = !string.IsNullOrEmpty(defaultConnection) ? "Configured" : "Not configured",
                    HangfireConnection = !string.IsNullOrEmpty(hangfireConnection) ? "Configured" : "Using DefaultConnection",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Status = "Database check failed",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // GET: api/Health/system-info
        [HttpGet("system-info")]
        public IActionResult GetSystemInfo()
        {
            return Ok(new
            {
                ServerTime = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                OSVersion = Environment.OSVersion.ToString(),
                RuntimeVersion = Environment.Version.ToString(),
                Services = new
                {
                    CertificateManagement = "Available",
                    HangfireBackgroundJobs = "Conditional",
                    EmailService = "Available",
                    PaymentService = "Available"
                }
            });
        }
    }
}
