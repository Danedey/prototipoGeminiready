using System;

namespace prototipoGeminiready.Services
{
    public class SesionActualService
    {
        public Guid SesionId { get; set; } = Guid.NewGuid();
    }
}
