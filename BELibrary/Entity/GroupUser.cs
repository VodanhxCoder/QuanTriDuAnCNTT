using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BELibrary.Entity
{
    [Table("GroupUser")]
    public class GroupUser
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }

        public virtual Group Group { get; set; }
        public virtual Account User { get; set; }
    }
}