using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Stargate.Server.Data.Models
{
    [Keyless]
    public class PersonAstronaut
    {
        public int PersonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CurrentRank { get; set; } = string.Empty;
        public string? CurrentDutyTitle { get; set; } = string.Empty;
        public DateTime? CareerStartDate { get; set; }
        public DateTime? CareerEndDate { get; set; }
    }

    public class PersonAstronautConfiguration : IEntityTypeConfiguration<PersonAstronaut>
    {
        public void Configure(EntityTypeBuilder<PersonAstronaut> builder)
        {
            builder.HasNoKey();
        }
    }
}
