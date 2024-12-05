namespace ApiForAngular.DTO
{
    public class CreateTaskDto
    {
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int ProjectID { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid AssignedTo { get; set; }
        public int TaskPriorityID { get; set; }
    }
}
