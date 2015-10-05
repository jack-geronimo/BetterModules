﻿using BetterModules.Core.Models;
using BetterModules.Core.Modules.Registration;

namespace BetterModules.Sample.Module.Models.Maps
{
    public class InheritedTestItemModelMap : EntitySubClassMapBase<InheritedTestItemModel>
    {
        public InheritedTestItemModelMap()
            : base(SampleModuleDescriptor.ModuleName)
        {
            Table("InheritedTestItems");
            
            Map(x => x.Description).Not.Nullable().Length(100);
        }
    }
}
