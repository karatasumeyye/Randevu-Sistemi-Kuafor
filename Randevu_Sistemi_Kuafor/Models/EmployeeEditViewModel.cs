namespace Randevu_Sistemi_Kuafor.Models
{
    public class EmployeeEditViewModel
    {
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserEmail { get; set; }
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
    }
}
