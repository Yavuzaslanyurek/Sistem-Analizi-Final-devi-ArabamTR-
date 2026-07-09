namespace ArabamTR.Models
{
    public class VehicleAd
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Year { get; set; }
        public string Km { get; set; }
        public string Plate { get; set; }

        // Yüklenen resmin sunucudaki adını/yolunu tutacak alan
        public string ImageUrl { get; set; }

        // İlanı hangi kullanıcının yüklediğini belirten alanlar (İlişki)
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
