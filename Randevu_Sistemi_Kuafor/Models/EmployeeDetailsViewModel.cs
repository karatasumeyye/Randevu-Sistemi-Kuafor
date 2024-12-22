namespace Randevu_Sistemi_Kuafor.Models
{
    public class EmployeeDetailsViewModel
    {
        public Employee Employee { get; set; }
        public List<Service> Services { get; set; }
        public List<int> SelectedServices { get; set; } // Seçilen servislerin ID'lerini tutacak
    }
}
