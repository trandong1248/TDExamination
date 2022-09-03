using Examination.Application.Queries.V1.GetHomeExamList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Examination.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ExamsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(IMediator mediator, ILogger<ExamsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetExamList()
        {
            _logger.LogInformation("Exams Controller V_01");
            var query = new GetHomeExamListQuery();
            var queryResult = await _mediator.Send(query);
            return Ok(queryResult);
        }
    }
}