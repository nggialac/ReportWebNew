//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReportWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BRANCH
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BRANCH()
        {
            this.ACC_TRANSACTION = new HashSet<ACC_TRANSACTION>();
            this.ACCOUNTs = new HashSet<ACCOUNT>();
            this.EMPLOYEEs = new HashSet<EMPLOYEE>();
        }
    
        public int BRANCH_ID { get; set; }
        public string ADDRESS { get; set; }
        public string CITY { get; set; }
        public string NAME { get; set; }
        public string STATE { get; set; }
        public string ZIP_CODE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ACC_TRANSACTION> ACC_TRANSACTION { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ACCOUNT> ACCOUNTs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMPLOYEE> EMPLOYEEs { get; set; }
    }
}
