namespace Registration.DTO.Registration
{
    public class RegistrationPassengerResponseDto
    {
        public string DynamicId       { get; set; }
        public string DepartureId     { get; set; }
        public string PassengerId     { get; set; }
        public string SeatNumber      { get; set; }
        public bool   IsPaid          { get; set; }
        public DateTime RegisteredAt  { get; set; }
    }
}