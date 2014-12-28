using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Moq;

namespace UnityAutoMoq
{
    public class UnityAutoMoqBuilderStrategy : BuilderStrategy
    {
        private readonly UnityAutoMoqContainer autoMoqContainer;
        private readonly Dictionary<Type, object> mocks;

        public UnityAutoMoqBuilderStrategy(UnityAutoMoqContainer autoMoqContainer)
        {
            this.autoMoqContainer = autoMoqContainer;
            mocks = new Dictionary<Type, object>();
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            var type = context.OriginalBuildKey.Type;

            if (autoMoqContainer.Registrations.Any(r => r.RegisteredType == type))
                return;

            if (type.IsInterface || type.IsAbstract)
            {
                context.Existing = GetOrCreateMock(type);
                context.BuildComplete = true;
            }
        }

        private object GetOrCreateMock(Type t)
        {
            if (mocks.ContainsKey(t))
                return mocks[t];

            Type genericType = typeof(Mock<>).MakeGenericType(new[] { t });

            object mock = Activator.CreateInstance(genericType, GetConstructorArguments(t));

            AsExpression interfaceImplementations = autoMoqContainer.GetInterfaceImplementations(t);
            if (interfaceImplementations != null)
                interfaceImplementations.GetImplementations().Each(type => genericType.GetMethod("As").MakeGenericMethod(type).Invoke(mock, null));

            genericType.InvokeMember("DefaultValue", BindingFlags.SetProperty, null, mock, new object[] { autoMoqContainer.DefaultValue });

            object mockedInstance = genericType.InvokeMember("Object", BindingFlags.GetProperty, null, mock, null);
            mocks.Add(t, mockedInstance);

            return mockedInstance;
        }

        /// <summary>
        /// Get the arguments for the constructor with the most parameters following Unity convention.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// There are more than one constructors with the same (max) amount of parameters. Unity throws the same exception where resolving a concrete type.
        /// </exception>
        private object[] GetConstructorArguments(Type t)
        {
            var constructors = (from c in t.GetConstructors()
                                let numberOfParameters = c.GetParameters().Length
                                group c by numberOfParameters into x
                                orderby x.Key descending
                                select x).ToList();

            if (constructors.Count == 0)
                return new object[0];
            if (constructors[0].Count() > 1)
                throw new InvalidOperationException(string.Format("The type {0} has multiple constructor of length {1}. Unable to disambiguate.", t.Name, constructors[0].Key));

            // Constructor with most arguments to follow Unity convention
            return constructors[0].First().GetParameters().Select(x => autoMoqContainer.Resolve(x.ParameterType, null)).ToArray();
        }
    }
}