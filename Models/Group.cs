using System;
using System.Collections.Generic;

#nullable disable

namespace DShabuninAIS.Models
{
    public partial class Group
    {
        public Group()
        {
            Students = new HashSet<Student>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
