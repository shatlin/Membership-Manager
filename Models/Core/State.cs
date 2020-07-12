﻿using System;
using System.Collections.Generic;

namespace MM.CoreModels
{
    public partial class CoreState
    {
        public CoreState()
        {
            CoreAddress = new HashSet<CoreAddress>();
        }

        public int Id { get; set; }
        public int? CountryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual ICollection<CoreAddress> CoreAddress { get; set; }
    }
}
