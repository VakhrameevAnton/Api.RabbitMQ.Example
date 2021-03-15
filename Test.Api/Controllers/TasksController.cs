using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Test.Bl.RequestHandlers;

namespace Test.Api.Controllers
{
    [Route("task")]
    public class TasksController : ControllerBase
    {
        private readonly ITasksRequestsHandler _tasksRequestsHandler;

        public TasksController(ITasksRequestsHandler tasksRequestsHandler)
        {
            _tasksRequestsHandler = tasksRequestsHandler;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var taskState = await _tasksRequestsHandler.Get(id);
            if (taskState == null)
            {
                return NotFound();
            }

            return Ok(taskState);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var taskId = await _tasksRequestsHandler.Create();
            return Accepted(taskId);
        }
    }
}