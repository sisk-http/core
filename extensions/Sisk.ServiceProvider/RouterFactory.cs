// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouterFactory.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Provides a class that instantiates a router, capable of porting applications for use with Agirax.
    /// </summary>
    /// <definition>
    /// public abstract class RouterFactory
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public abstract class RouterFactory
    {
        /// <summary>
        /// Build and gets a router that will be used later by an <see cref="ListeningHost"/>.
        /// </summary>
        /// <definition>
        /// public abstract Router BuildRouter();
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public abstract Router BuildRouter();

        /// <summary>
        /// Method that is called by the service instantiator with the parameters defined in configuration before calling <see cref="BuildRouter()"/>.
        /// </summary>
        /// <param name="setupParameters">Parameters that are defined in a configuration file.</param>
        /// <definition>
        /// public abstract void Setup(NameValueCollection setupParameters);
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public abstract void Setup(NameValueCollection setupParameters);

        /// <summary>
        /// Method that is called immediately before initializing the service, after all configurations was parsed and set up.
        /// </summary>
        /// <definition>
        /// public abstract void Bootstrap();
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public abstract void Bootstrap();

        /// <summary>
        /// Associates the parameters received in the service configuration to a managed object.
        /// </summary>
        /// <typeparam name="T">The type of the managed object that will have the service parameters mapped.</typeparam>
        /// <param name="setupParameters">The application input parameters.</param>
        /// <definition>
        /// public T MapSetupParameters{{T}}(NameValueCollection setupParameters)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public T MapSetupParameters<T>(NameValueCollection setupParameters)
        {
            T parametersObject = Activator.CreateInstance<T>()!;

            Type parameterType = typeof(T);
            PropertyInfo[] properties = parameterType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object mappingValue;
                string? value = setupParameters[property.Name];
                Type propertyValue = property.PropertyType;

                if (value == null) continue;
                if (propertyValue.IsEnum)
                {
                    mappingValue = Enum.Parse(propertyValue, value, true);
                }
                else if (propertyValue == typeof(string))
                {
                    mappingValue = value;
                }
                else
                {
                    try
                    {
                        mappingValue = Convert.ChangeType(value, property.PropertyType);
                    }
                    catch
                    {
                        throw new InvalidCastException($"Cannot cast the property {property.Name} value into {propertyValue.FullName}.");
                    }
                }

                property.SetValue(parametersObject, mappingValue);
            }

            ValidationContext vc = new ValidationContext(parametersObject);
            ICollection<ValidationResult> results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(parametersObject, vc, results, true);

            if (!isValid)
            {
                throw new ValidationException(results.First(), null, null);
            }

            return parametersObject;
        }
    }
}
