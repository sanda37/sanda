namespace sanda.Models
{
    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }  // جعل الحقل اختياريًا
        public string? LastName { get; set; }  // جعل الحقل اختياريًا
        public string? PhoneNumber { get; set; }  // جعل الحقل اختياريًا
        public int? Age { get; set; }  // جعل الحقل اختياريًا
        public string? Email { get; set; }
        public string? Gender { get; set; }  // جعل الحقل اختياريًا
        public string? Address { get; set; }  // جعل الحقل اختياريًا
        public string? ProfilePicturePath { get; set; }  // جعل الحقل اختياريًا
       // public bool? HasMobilityDisability { get; private set; }  // جعل الحقل اختياريًا
        //public string? DisabilityProofPath { get; private set; }
    }
}