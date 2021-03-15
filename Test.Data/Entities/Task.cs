using System;
using System.ComponentModel.DataAnnotations;
using Test.Common;

namespace Test.Data.Entities
{
    public class Task
    {
        [Key]
        public Guid Id { get; set; }
        public ETaskState State { get; set; }
        public DateTime TimeStamp { get; set; }

        public Task()
        {
            Id = Guid.NewGuid();
            State = ETaskState.Created;
            TimeStamp = DateTime.Now;
        }
    }
}