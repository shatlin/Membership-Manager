﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MM.ClientModels
{
    public partial class MemberCategory
    {
        public MemberCategory()
        {
            CpdmemberCategorySetUp = new HashSet<CpdmemberCategorySetUp>();
            MemberType = new HashSet<MemberType>();
            PlanMaster = new HashSet<PlanMaster>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual ICollection<CpdmemberCategorySetUp> CpdmemberCategorySetUp { get; set; }
        public virtual ICollection<MemberType> MemberType { get; set; }
        public virtual ICollection<PlanMaster> PlanMaster { get; set; }
    }
    public partial class MemberCategoryConfiguration : IEntityTypeConfiguration<MemberCategory>
    {
        public void Configure(EntityTypeBuilder<MemberCategory> builder)
        {
  builder.Property(e => e.CreatedOn).HasColumnType("datetime");

                builder.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                builder.Property(e => e.ModifiedOn).HasColumnType("datetime");

                builder.Property(e => e.Name).HasMaxLength(100);
        }

    }
    public static partial class Seeder
    {
        public static void SeedMemberCategory(this ModelBuilder modelBuilder)
        {

        }
    }
}
