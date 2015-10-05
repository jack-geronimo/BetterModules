﻿using System;
using System.Linq;
using BetterModules.Core.DataAccess;
using BetterModules.Core.DataAccess.DataContext;
using BetterModules.Core.Exceptions.DataTier;
using BetterModules.Sample.Module;
using BetterModules.Sample.Module.Models;
using Microsoft.Framework.DependencyInjection;
using NHibernate.Proxy.DynamicProxy;
using Xunit;

namespace BetterModules.Core.Database.Tests.DataAccess
{
    public class DefaultRepositoryIntegrationTests : DatabaseTestBase
    {
        private TestItemCategory category1;
        private TestItemModel model1;
        private TestItemModel model2;
        private TestItemModel model3;
        private TestItemModel deletedModel;
        private bool isSet;
        public DefaultRepositoryIntegrationTests()
        {
            if (!isSet)
            {
                isSet = true;

                category1 = DatabaseTestDataProvider.ProvideRandomTestItemCategory();

                model1 = DatabaseTestDataProvider.ProvideRandomTestItemModel(category1);
                model1.Name = "DRT_01";
                model2 = DatabaseTestDataProvider.ProvideRandomTestItemModel(category1);
                model2.Name = "DRT_02";
                model3 = DatabaseTestDataProvider.ProvideRandomTestItemModel(category1);
                model3.Name = "DRT_03";
                deletedModel = DatabaseTestDataProvider.ProvideRandomTestItemModel(category1);
                deletedModel.Name = "DRT_04";

                Repository.Save(model3);
                Repository.Save(deletedModel);
                Repository.Save(model2);
                Repository.Save(model1);
                UnitOfWork.Commit();

                Repository.Delete(deletedModel);
                UnitOfWork.Commit();
            }
        }

        [Fact]
        public void Should_UnProxy_Entity()
        {
            var proxy = Repository.AsProxy<TestItemModel>(SampleModuleDescriptor.TestItemModelId);
            Assert.NotNull(proxy);
            Assert.True(proxy is IProxy);

            var unproxy = Repository.UnProxy(proxy);
            Assert.NotNull(unproxy);
            Assert.Equal(unproxy.Id, SampleModuleDescriptor.TestItemModelId);
            Assert.False(unproxy is IProxy);
        }

        [Fact]
        public void Should_Return_Same_Entity_When_UnProxying_Entity()
        {
            var proxy = Repository.AsProxy<TestItemModel>(model1.Id);
            Assert.NotNull(proxy);

            var unproxy = Repository.UnProxy(proxy);
            Assert.NotNull(unproxy);

            Assert.Equal(unproxy, proxy);
        }

        [Fact]
        public void Should_Load_Entity_AsProxy()
        {
            var proxy = Repository.AsProxy<TestItemModel>(Guid.NewGuid());

            Assert.NotNull(proxy);
            Assert.True(proxy is IProxy);
        }

        [Fact]
        public void Should_Retrieve_First_Entity_By_Id()
        {
            var entity = Repository.First<TestItemModel>(model1.Id);

            Assert.NotNull(entity);
            Assert.Equal(entity.Id, model1.Id);
        }

        [Fact]
        public void Should_Throw_Exception_Retrieving_First_Entity_By_Id()
        {
            Assert.Throws<EntityNotFoundException>(() =>
            {
                Repository.First<TestItemModel>(Guid.NewGuid());
            });

        }

        [Fact]
        public void Should_Retrieve_First_Entity_By_Filter()
        {
            var entity = Repository.First<TestItemModel>(m => m.Id == model1.Id);

            Assert.NotNull(entity);
            Assert.Equal(entity.Id, model1.Id);
        }

        [Fact]
        public void Should_Throw_Exception_Retrieving_First_Entity_By_Filter()
        {
            Assert.Throws<EntityNotFoundException>(() =>
            {
                var guid = Guid.NewGuid();
                Repository.First<TestItemModel>(m => m.Id == guid);
            });
        }

        [Fact]
        public void Should_Retrieve_FirstOrDefault_Entity_By_Id()
        {
            var entity = Repository.FirstOrDefault<TestItemModel>(model1.Id);

            Assert.NotNull(entity);
            Assert.Equal(entity.Id, model1.Id);
        }

        [Fact]
        public void Should_Retrieve_FirstOrDefault_Entity_By_Filter()
        {
            var entity = Repository.FirstOrDefault<TestItemModel>(m => m.Id == model1.Id);

            Assert.NotNull(entity);
            Assert.Equal(entity.Id, model1.Id);
        }

        [Fact]
        public void Should_Retrieve_Null_Retrieving_FirstOrDefault_Entity_By_Id()
        {
            var guid = Guid.NewGuid();
            var entity = Repository.FirstOrDefault<TestItemModel>(guid);

            Assert.Null(entity);
        }

        [Fact]
        public void Should_Retrieve_Null_Retrieving_FirstOrDefault_Entity_By_Filter()
        {
            var guid = Guid.NewGuid();
            var entity = Repository.FirstOrDefault<TestItemModel>(m => m.Id == guid);

            Assert.Null(entity);
        }

        [Fact]
        public void Should_Return_QueryOver_Without_Deleted_Generic()
        {
            var list = Repository
                .AsQueryOver<TestItemModel>().Where(q => q.Category == category1)
                .OrderBy(q => q.Name).Asc
                .List<TestItemModel>();

            Assert.NotNull(list);
            Assert.Equal(list.Count, 3);
            Assert.Equal(list[0].Id, model1.Id);
            Assert.Equal(list[1].Id, model2.Id);
            Assert.Equal(list[2].Id, model3.Id);
        }

        [Fact]
        public void Should_Return_QueryOver_Without_Deleted_By_Alias()
        {
            TestItemModel alias = null;
            var list = Repository
                .AsQueryOver(() => alias)
                .Where(() => alias.Category == category1)
                .OrderBy(q => q.Name).Asc
                .List<TestItemModel>();

            Assert.NotNull(list);
            Assert.Equal(list.Count, 3);
            Assert.Equal(list[0].Id, model1.Id);
            Assert.Equal(list[1].Id, model2.Id);
            Assert.Equal(list[2].Id, model3.Id);
        }

        [Fact]
        public void Should_Return_QueryOver_Without_Deleted_By_Null_Alias()
        {
            TestItemModel alias = null;
            var list = Repository
                .AsQueryOver<TestItemModel>(null)
                .Where(t => t.Category == category1)
                .OrderBy(q => q.Name).Asc
                .List<TestItemModel>();

            Assert.NotNull(list);
            Assert.Equal(list.Count, 3);
            Assert.Equal(list[0].Id, model1.Id);
            Assert.Equal(list[1].Id, model2.Id);
            Assert.Equal(list[2].Id, model3.Id);
        }

        [Fact]
        public void Should_Return_Queryable_By_Filter()
        {
            var list = Repository
                .AsQueryable<TestItemModel>(q => q.Category == category1)
                .OrderBy(q => q.Name)
                .ToList();

            Assert.NotNull(list);
            Assert.Equal(list.Count, 3);
            Assert.Equal(list[0].Id, model1.Id);
            Assert.Equal(list[1].Id, model2.Id);
            Assert.Equal(list[2].Id, model3.Id);
        }

        [Fact]
        public void Should_Return_Queryable_Without_Deleted()
        {
            var list = Repository
                .AsQueryable<TestItemModel>()
                .Where(q => q.Category == category1)
                .OrderBy(q => q.Name)
                .ToList();

            Assert.NotNull(list);
            Assert.Equal(list.Count, 3);
            Assert.Equal(list[0].Id, model1.Id);
            Assert.Equal(list[1].Id, model2.Id);
            Assert.Equal(list[2].Id, model3.Id);
        }

        [Fact]
        public void Should_Check_If_Record_Exists()
        {
            var exists = Repository.Any<TestItemModel>(q => q.Name == model1.Name);

            Assert.True(exists);
        }

        [Fact]
        public void Should_Check_If_Deleted_Record_Not_Exists()
        {
            var exists = Repository.Any<TestItemModel>(q => q.Name == deletedModel.Name);

            Assert.False(exists);
        }

        [Fact]
        public void Should_Save_Entity()
        {
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();

            Repository.Save(model);
            UnitOfWork.Commit();

            Assert.True(model.Id != default(Guid));
        }

        [Fact]
        public void Should_Delete_Entity_By_Id_NotAsProxy()
        {
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();

            Repository.Save(model);
            UnitOfWork.Commit();

            Assert.True(model.Id != default(Guid));

            Repository.Delete<TestItemModel>(model.Id, model.Version, false);
            UnitOfWork.Commit();

            var exists = Repository.Any<TestItemModel>(q => q.Id == model.Id);
            Assert.False(exists);
        }

        [Fact]
        public void Should_Delete_Entity_By_Id_AsProxy()
        {
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();

            Repository.Save(model);
            UnitOfWork.Commit();

            Assert.True(model.Id != default(Guid));

            Repository.Delete<TestItemModel>(model.Id, model.Version, true);
            UnitOfWork.Commit();

            var exists = Repository.Any<TestItemModel>(q => q.Id == model.Id);
            Assert.False(exists);
        }

        [Fact]
        public void Should_Delete_Entity()
        {
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();

            Repository.Save(model);
            UnitOfWork.Commit();

            Assert.True(model.Id != default(Guid));

            Repository.Delete(model);
            UnitOfWork.Commit();

            var exists = Repository.Any<TestItemModel>(q => q.Id == model.Id);
            Assert.False(exists);
        }

        [Fact]
        public void Should_Attach_Entity()
        {
            // Create entity
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();
            Repository.Save(model);
            UnitOfWork.Commit();

            var version = model.Version;

            // Load detached version, touch multiple times
            var detachedModel = Repository.First<TestItemModel>(model.Id);
            Repository.Detach(detachedModel);
            detachedModel.Name = TestDataProvider.ProvideRandomString();
            UnitOfWork.Commit();
            detachedModel.Name = TestDataProvider.ProvideRandomString();
            UnitOfWork.Commit();

            Assert.Equal(detachedModel.Version, version);

            // Attach and save again
            Repository.Attach(detachedModel);
            detachedModel.Name = TestDataProvider.ProvideRandomString();
            UnitOfWork.Commit();

            Assert.NotEqual(detachedModel.Version, version);
        }

        [Fact]
        public void Should_Detach_Entity()
        {
            // Create entity
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();
            Repository.Save(model);
            UnitOfWork.Commit();

            // Touch entity - changes should be saved on flush
            var version = model.Version;

            var attachedModel = Repository.First<TestItemModel>(model.Id);
            attachedModel.Name = TestDataProvider.ProvideRandomString();
            UnitOfWork.Commit();

            Assert.True(attachedModel.Version > version);
            version = attachedModel.Version;

            // Detach and touch again - changes shouldn't saved on flush
            var detachedModel = Repository.First<TestItemModel>(model.Id);
            Repository.Detach(detachedModel);
            detachedModel.Name = TestDataProvider.ProvideRandomString();
            UnitOfWork.Commit();

            Assert.Equal(detachedModel.Version, version);
        }

        [Fact]
        public void Should_Refresh_Entity()
        {
            // Create entity
            var model = DatabaseTestDataProvider.ProvideRandomTestItemModel();
            Repository.Save(model);
            UnitOfWork.Commit();

            var version = model.Version;

            // Load attached and detached version, touch multiple times
            var detachedModel = Repository.First<TestItemModel>(model.Id);

            // Open another session
            var provider = Services.BuildServiceProvider();
            var repository2 = provider.GetService<IRepository>();
            var unitOfWork2 = provider.GetService<IUnitOfWork>();

            var attachedModel = repository2.First<TestItemModel>(model.Id);
            attachedModel.Name = TestDataProvider.ProvideRandomString();
            unitOfWork2.Commit();

            Assert.Equal(detachedModel.Version, version);

            // Refresh detached entity - version should be updated
            Repository.Refresh(detachedModel);
            Assert.NotEqual(detachedModel.Version, version);
        }
    }
}
