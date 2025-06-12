using Microsoft.AspNetCore.Mvc;
using SysmonDotNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SysmonDotNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly MetricsDbContext _db;
    public MetricsController(MetricsDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetLatest([FromQuery]int count=10) {
        var list = await _db.Metrics
            .OrderByDescending(m=>m.Timestamp)
            .Take(count)
            .ToListAsync();
        return Ok(list);
    }
}
