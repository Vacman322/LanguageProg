//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LanguageProg.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class PhotoService
    {
        public int ID { get; set; }
        public int IDService { get; set; }
        public string Photo { get; set; }
    
        public virtual Service Service { get; set; }
    }
}
