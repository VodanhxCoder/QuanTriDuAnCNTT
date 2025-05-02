using System;

namespace HospitalManagement.Core.Dto
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Guid GroupId { get; set; }
        public string Content { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedBy { get; set; }
        public bool? IsPinned { get; set; }
        public bool IsMe { get; set; }
        public Guid? PinnedBy { get; set; }
        public DateTime? PinnedDate { get; set; }

        public Guid SendTo { get; set; }
        public UserDto UserCreatedBy { get; set; }
        // public List<IFormFile> Attachments { get; set; }
    }

    public class MessageStatisticDto
    {
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAvatar { get; set; }
        public long MessageCount { get; set; }
        public long TextMessageCount { get; set; }
        public long MediaCount { get; set; }
        public long AttachmentCount { get; set; }
    }
}