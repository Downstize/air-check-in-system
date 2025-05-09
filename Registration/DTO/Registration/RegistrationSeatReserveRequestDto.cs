namespace Registration.DTO.Registration
{
    public class RegistrationSeatReserveRequestDto
    {
        public string DynamicId   { get; set; }
        public string DepartureId { get; set; }
        public string PassengerId { get; set; }
        public string SeatNumber  { get; set; }
    }
}