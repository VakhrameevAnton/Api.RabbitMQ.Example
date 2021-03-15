using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Test.Bl.Messages;
using Test.Common;
using Test.Data;

namespace Test.Bl.MessageHandlers
{
    /// <summary>
    /// Обработчик сообщений TaskCreatedMessage
    /// </summary>
    public class TaskCreatedMessageHandler : IConsumer<TaskCreatedMessage>
    {
        private readonly IMessageScheduler _publishEndpoint;
        private readonly TestDbContext _dbContext;
        private readonly ILogger<TaskCreatedMessageHandler> _logger;

        public TaskCreatedMessageHandler(TestDbContext dbContext, ILogger<TaskCreatedMessageHandler> logger, IMessageScheduler publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskCreatedMessage> context)
        {
            var taskId = context.Message.TaskId;
            
            // Если нулл - предупреждаем, что нулл и ничего не делаем
            if (taskId == null)
            {
                _logger.Log(LogLevel.Warning, "TaskCreatedMessage: message.taskId == null");
                return;
            }

            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            
            // Если нулл - пишем эррор(какая-то тут очевидная нестыковка), что нулл и ничего не делаем.
            // Если по условию задачи не прописано иное
            if (task == null)
            {
                _logger.Log(LogLevel.Error, $"TaskCreatedMessage: Задача с Id == {taskId} не найдена");
                return;
            }

            // Меняем статус
            task.State = ETaskState.Running;
            _dbContext.Update(task);
            await _dbContext.SaveChangesAsync();

            // Оповещаем, что статус поменяли и говорим, в какоем время надо ее закрыть (через 2 мин) 
            await _publishEndpoint.SchedulePublish(DateTime.UtcNow + TimeSpan.FromMinutes(2),
                new TaskStartedMessage {TaskId = taskId});
        }
    }
}