using System;
using System.Web;
using NUnit.Framework;
using Moq;
using Microsoft.Practices.Unity;

namespace UnityAutoMoq.Tests
{
    [TestFixture]
    public class UnityAutoMoqContainerFixture
    {
        private UnityAutoMoqContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new UnityAutoMoqContainer();
        }

        [Test]
        public void Can_get_instance_without_registering_it_first()
        {
            var mocked = container.Resolve<IService>();

            mocked.ShouldNotBeNull();
        }

        [Test]
        public void Can_get_mock()
        {
            Mock<IService> mock = container.GetMock<IService>();

            mock.ShouldNotBeNull();
        }

        [Test]
        public void Mocked_object_and_resolved_instance_should_be_the_same()
        {
            Mock<IService> mock = container.GetMock<IService>();
            var mocked = container.Resolve<IService>();

            mock.Object.ShouldBeSameAs(mocked);
        }

        [Test]
        public void Mocked_object_and_resolved_instance_should_be_the_same_order_independent()
        {
            var mocked = container.Resolve<IService>();
            Mock<IService> mock = container.GetMock<IService>();

            mock.Object.ShouldBeSameAs(mocked);
        }

        [Test]
        public void Should_apply_default_default_value_when_none_specified()
        {
            container = new UnityAutoMoqContainer();
            var mocked = container.GetMock<IService>();

            mocked.DefaultValue.ShouldEqual(DefaultValue.Mock);
        }

        [Test]
        public void Should_apply_specified_default_value_when_specified()
        {
            container = new UnityAutoMoqContainer(DefaultValue.Empty);
            var mocked = container.GetMock<IService>();

            mocked.DefaultValue.ShouldEqual(DefaultValue.Empty);
        }

        [Test]
        public void Should_apply_specified_default_value_when_specified_2()
        {
            container = new UnityAutoMoqContainer{DefaultValue = DefaultValue.Empty};
            var mocked = container.GetMock<IService>();

            mocked.DefaultValue.ShouldEqual(DefaultValue.Empty);
        }

        [Test]
        public void Can_resolve_concrete_type_with_dependency()
        {
            var concrete = container.Resolve<Service>();

            concrete.ShouldNotBeNull();
            concrete.AnotherService.ShouldNotBeNull();
        }

        [Test]
        public void Getting_mock_after_resolving_concrete_type_should_return_the_same_mock_as_passed_as_argument_to_the_concrete()
        {
            var concrete = container.Resolve<Service>();
            Mock<IAnotherService> mock = container.GetMock<IAnotherService>();

            concrete.AnotherService.ShouldBeSameAs(mock.Object);
        }

        [Test]
        public void Can_configure_mock_as_several_interfaces()
        {
            container.ConfigureMock<IService>().As<IDisposable>();

            container.GetMock<IService>().As<IDisposable>();
        }

        [Test]
        public void Can_configure_mock_as_several_interfaces_2()
        {
            container.ConfigureMock<IService>().As<IDisposable>().As<IAnotherService>();

            container.GetMock<IService>().As<IDisposable>();
            container.GetMock<IService>().As<IAnotherService>();
        }

        [Test]
        public void Can_lazy_load_dependencies()
        {
            var service = container.Resolve<LazyService>();

            Assert.That(service.ServiceFunc(), Is.InstanceOf<IService>());
        }

        [Test]
        public void Can_mock_abstract_classes()
        {
            var mock = container.GetMock<HttpContextBase>();

            mock.ShouldBeOfType<Mock<HttpContextBase>>();
        }

        [Test]
        public void Can_inject_mocked_abstract_class()
        {
            var concrete = container.Resolve<ServiceWithAbstractDependency>();
            var mock = container.GetMock<HttpContextBase>();

            concrete.HttpContextBase.ShouldBeSameAs(mock.Object);
        }

        [Test]
        public void Can_get_registered_implementation()
        {
            container.RegisterType<IAnotherService, AnotherService>();
            var real = container.Resolve<IAnotherService>();

            real.ShouldBeOfType<AnotherService>();
        }

        [Test]
        public void Can_mock_abstract_classes_without_parameterless_constructor()
        {
            var real = container.Resolve<AbstractService>();
            var mock = container.GetMock<AbstractService>();

            real.ShouldBeSameAs(mock.Object);
        }
    }
}