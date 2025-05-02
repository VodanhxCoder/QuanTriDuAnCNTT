using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BELibrary.Entity
{
    [Table("Message")]
    public class Message
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Guid GroupId { get; set; }
        public string Content { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedBy { get; set; }

        public bool? IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool? IsPinned { get; set; }
        public Guid? PinnedBy { get; set; }
        public DateTime? PinnedDate { get; set; }

        public virtual Group Group { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual Account UserCreatedBy { get; set; }
    }
}