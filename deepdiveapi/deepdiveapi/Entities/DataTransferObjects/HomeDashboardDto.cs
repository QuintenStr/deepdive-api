using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO representing the data for a home dashboard, including excursion statistics and random excursion information.
    /// </summary>
    public class HomeDashboardDto
    {
        [Required]
        public List<ExcursionInfo> RandomExcursionInfos { get; set; }
        [Required]
        public int TotalExcursions { get; set; }
        [Required]
        public int UpcomingExcursions { get; set; }
        [Required]
        public int ParticipatedInExcursions { get; set; }
        [Required]
        public int GoingToParticipateInExcursions { get; set; }
    }

    /// <summary>
    /// Sub-DTO for providing excursion information.
    /// </summary>
    public class ExcursionInfo
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        [Required]
        public string ImageName { get; set; }
    }
}
