using fuszerkomat_api.Data.Models;

namespace fuszerkomat_api.VMO
{
    public class ChatVMO
    {
        public string ConversationId { get; set; }
        public string CorespondentId { get; set; }
        public string CorespondentImg {  get; set; }
        public string CorespondentName { get; set; }
        public bool IsArchived { get; set; } = false;
        public LastChatMsgVMO? LastMsg {  get; set; }
        public TaskChatVMO? TaskData { get; set; }        
    }

    public class LastChatMsgVMO
    {
        public string Msg { get; set; }
        public bool Own {  get; set; }
    }

    public class TaskChatVMO
    {
        public int Id { get; set; }
        public string CreatorId { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public Status Status { get; set; }
        public ApplicationStatusVMO? ApplicationStatus { get; set; }
    }

    public class ApplicationStatusVMO
    {
        public ApplicationStatus Status { get; set; }
    }
}
