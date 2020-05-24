using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmokeTest.Scheduler.Models
{
    public sealed class ScheduleTypeModel
    {
        #region Constructors

        public ScheduleTypeModel(in string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        #endregion Constructors

        #region Properties

        public string Name { get; set; }

        #endregion Properties
    }
}
