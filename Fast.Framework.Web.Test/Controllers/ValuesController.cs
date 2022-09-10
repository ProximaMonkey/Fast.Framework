using Fast.Framework.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fast.Framework.Web.Test.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly ILogger<ValuesController> logger;

        private readonly IDbContext db;

        public ValuesController(ILogger<ValuesController> logger, IDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }
    }
}
