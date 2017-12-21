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
using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class application_users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public application_users()
        {
            this.campaign_user_infos = new HashSet<campaign_user_infos>();
            this.executions = new HashSet<executions>();
        }
    
        public int id { get; set; }
        public int application_id { get; set; }
        public string external_id { get; set; }
        public System.DateTime created_at { get; set; }
        public System.DateTime updated_at { get; set; }
    
        public virtual applications applications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<campaign_user_infos> campaign_user_infos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<executions> executions { get; set; }

        [NotMapped]
        public int[] arrayCaract;

        [NotMapped]
        public double[] scoresTeste;

        [NotMapped]
        public double[] zScores;

        /// <summary>
        /// Ser� uma lista com 15 listas de inteiros (ou doubles, ainda n�o sei), 
        /// 15 => uma lista pra cada quest�o do quiz, por exemplo, 
        /// a primeira quest�o possui nove respostas, 
        /// portanto a primeira lista desta lista possuir� 9 elementos
        /// </summary>
        [NotMapped]
        public List<List<int>> respostasDoStyle;
    }
}
