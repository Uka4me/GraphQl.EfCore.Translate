using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Models
{
    public class Student
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstMidName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        [NotMapped]
        public virtual string CalculatedField { get; set; }
        [NotMapped]
        public virtual int CalculatedField2 { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
