using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    /// <summary>
    /// "Əlaqə" formundan gələn mesajlar. Frontend-dəki (Supabase) contact_messages
    /// cədvəlinin bu backend-dəki analoqu — eyni funksionallıq (admin oxuyur/işarələyir,
    /// sadə sürət-məhdudiyyəti eyni e-poçtdan son 1 saatda 3-dən çox mesaja icazə vermir).
    /// </summary>
    public class ContactMessage : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
    }
}
