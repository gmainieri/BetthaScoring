//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bettha_Scoring.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class test_section_question_options
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public test_section_question_options()
        {
            this.execution_answers = new HashSet<execution_answers>();
        }
    
        public int id { get; set; }
        public int test_section_question_id { get; set; }
        public Nullable<int> test_competence_id { get; set; }
        public string content { get; set; }
        public decimal score { get; set; }
        public bool deprecated { get; set; }
        public System.DateTime created_at { get; set; }
        public System.DateTime updated_at { get; set; }
        public string external_id { get; set; }
        public string extra_content { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<execution_answers> execution_answers { get; set; }
        public virtual test_competences test_competences { get; set; }
        public virtual test_section_questions test_section_questions { get; set; }
    }
}