using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Test.Bl.Messages;
using Test.Common;
using Test.Data;
using Task = Test.Data.Entities.Task;

namespace Test.Bl.RequestHandlers
{
    public interface ITasksRequestsHandler
    {
        public Task<Guid> Create();
        public Task<ETaskState?> Get(Guid id);
    }

    /// <summary>
    /// Логика по обработке запросов к TasksController
    /// </summary>
    public class TasksRequestsHandler : ITasksRequestsHandler
    {
        private readonly TestDbContext _dbContext;
        private readonly IBusControl _busControl;


        public TasksRequestsHandler(TestDbContext dbContext, IBusControl busControl)
        {
            _dbContext = dbContext;
            _busControl = busControl;
        }

        /// <summary>
        /// Создаем задачу
        /// </summary>
        /// <returns>Id задачи</returns>
        public async Task<Guid> Create()
        {
            var newTask = new Task();

            _dbContext.Tasks.Add(newTask);
            await _dbContext.SaveChangesAsync();

            await _busControl.Publish(new TaskCreatedMessage {TaskId = newTask.Id});

            return newTask.Id;
        }

        /// <summary>
        /// Получаем статус задачи по Id задачи
        /// </summary>
        /// <param name="id">Id задачи</param>
        /// <returns>Статус задачи</returns>
        public async Task<ETaskState?> Get(Guid id)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            return task?.State;
        }
    }
}