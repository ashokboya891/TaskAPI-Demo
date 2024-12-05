namespace ApiForAngular.DTO
{
    public class TaskDto
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string CreatedOnString { get; set; }
        public string LastUpdatedOnString { get; set; }
        public List<TaskStatusDetailDto> TaskStatusDetails { get; set; }
    }
}
