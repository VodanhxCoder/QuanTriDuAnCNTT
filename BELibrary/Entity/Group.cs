using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BELibrary.Entity
{
    [Table("Group")]
    public class Group
    {
        [Key]
        public Guid Id { get; set; }

        public string Type { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime LastActive { get; set; }

        public virtual ICollection<GroupUser> GroupUsers { get; set; }
    }
}