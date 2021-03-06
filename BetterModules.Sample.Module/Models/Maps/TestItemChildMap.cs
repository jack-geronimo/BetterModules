﻿using BetterModules.Core.Models;

namespace BetterModules.Sample.Module.Models.Maps
{
    public class TestItemChildMap : EntityMapBase<TestItemModelChild>
    {
        public TestItemChildMap()
            : base(SampleModuleDescriptor.ModuleName)
        {
            Table("TestItemChildren");

            Map(x => x.Name).Not.Nullable().Length(100);
            References(x => x.Category).Column("TestItemCategoryId").Cascade.SaveUpdate().LazyLoad();
            References(x => x.Item).Column("TestItemId").Cascade.SaveUpdate().LazyLoad();
        }
    }
}
