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
    /// Обработчик сообщений TaskStartedMessage
    /// </summary>
    public class TaskStartedMessageHandler : IConsumer<TaskStartedMessage>
    {
        private readonly TestDbContext _dbContext;
        private readonly ILogger<TaskStartedMessageHandler> _logger;

        public TaskStartedMessageHandler(TestDbContext dbContext, ILogger<TaskStartedMessageHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskStartedMessage> context)
        {
            var taskId = context.Message.TaskId;
            
            // Если нулл - предупреждаем, что нулл и ничего не делаем
            if (taskId == null)
            {
                _logger.Log(LogLevel.Warning, "TaskCreatedMessage: message.taskId == null");
                return;
            }

            // Если нулл - пишем эррор(какая-то тут очевидная нестыковка), что нулл и ничего не делаем.
            // Если по условию задачи не прописано иное
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                _logger.Log(LogLevel.Error, $"TaskCreatedMessage: Задача с Id == {taskId} не найдена");
                return;
            }

            // Меняем статус
            task.State = ETaskState.Finished;
            _dbContext.Update(task);
            await _dbContext.SaveChangesAsync();
        }
    }
}