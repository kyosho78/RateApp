//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RateApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ratings
    {
        public int RatingId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public Nullable<int> RatingValue { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
    
        public virtual Suppliers Suppliers { get; set; }
        public virtual Users Users { get; set; }
    }
}
