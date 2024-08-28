// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RequestHandledAttribute.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Routing
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Specifies that the method, when used on this attribute, will instantiate the type and call the <see cref="IRequestHandler"/> with given parameters.
    /// </summary>
    public class RequestHandlerAttribute<[DynamicallyAccessedMembers(
              DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )] T> : RequestHandlerAttribute where T : IRequestHandler
    {
        /// <summary>
        /// Creates an new instance of this <see cref="RequestHandlerAttribute{T}"/> class.
        /// </summary>
        public RequestHandlerAttribute() : base(typeof(T)) { }

        /// <summary>
        /// Creates an new instance of this <see cref="RequestHandlerAttribute{T}"/> class with the specified
        /// constructor arguments for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="constructorArguments">An optional array of objects which is passed to the request handler constructor.</param>
        public RequestHandlerAttribute(params object?[] constructorArguments) : base(typeof(T))
        {
            this.ConstructorArguments = constructorArguments;
        }
    }
#endif

    /// <summary>
    /// Specifies that the method, when used on this attribute, will instantiate the type and call the <see cref="IRequestHandler"/> with given parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequestHandlerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type that implements <see cref="IRequestHandler"/> which will be instantiated.
        /// </summary>

        [DynamicallyAccessedMembers(
              DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )]
        public Type RequestHandlerType { get; set; }

        /// <summary>
        /// Specifies parameters for the given type's constructor.
        /// </summary>
        public object?[] ConstructorArguments { get; set; }

        /// <summary>
        /// Creates a new instance of this attribute with the informed parameters.
        /// </summary>
        /// <param name="handledBy">The type that implements <see cref="IRequestHandler"/> which will be instantiated.</param>
        public RequestHandlerAttribute([DynamicallyAccessedMembers(
              DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )] Type handledBy)
        {
            this.RequestHandlerType = handledBy;
            this.ConstructorArguments = Array.Empty<object?>();
        }

        internal IRequestHandler Activate()
        {
            IRequestHandler? rhandler = Activator.CreateInstance(this.RequestHandlerType, this.ConstructorArguments) as IRequestHandler;
            if (rhandler is null)
            {
                throw new ArgumentException(SR.Format(SR.RequestHandler_ActivationException, this.RequestHandlerType.FullName, this.ConstructorArguments.Length));
            }
            return rhandler;
        }
    }
}
