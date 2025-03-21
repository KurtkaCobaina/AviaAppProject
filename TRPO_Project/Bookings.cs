//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TRPO_Project
{
    using System;
    using System.Collections.Generic;
    
    public partial class Bookings
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bookings()
        {
            this.Payments = new HashSet<Payments>();
        }
    
        public int BookingID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> FlightID { get; set; }
        public Nullable<System.DateTime> BookingDate { get; set; }
        public string Status { get; set; }
        public Nullable<int> Amount { get; set; }
    
        public virtual Flights Flights { get; set; }
        public virtual Users Users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payments> Payments { get; set; }
    }
}
