// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Linq;
    using Mono.Cecil;

    internal readonly struct CustomAttributeFactory
    {
        private readonly WellKnownTypes _wellKnownTypes;

        public CustomAttributeFactory(WellKnownTypes wellKnownTypes)
        {
            _wellKnownTypes = wellKnownTypes;
        }

        public CustomAttribute CompilerGenerated()
        {
            MethodDefinition constructor = _wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            return new CustomAttribute(_wellKnownTypes.Module.ImportReference(constructor));
        }

        public CustomAttribute Embedded()
        {
            return new CustomAttribute(_wellKnownTypes.MicrosoftCodeAnalysisEmbeddedAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0));
        }

        public CustomAttribute Nullable(byte value)
        {
            var customAttribute = new CustomAttribute(_wellKnownTypes.SystemRuntimeCompilerServicesNullableAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 1 && !method.Parameters[0].ParameterType.IsArray));
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_wellKnownTypes.TypeSystem.Byte, value));
            return customAttribute;
        }

        public CustomAttribute Nullable(byte[] value)
        {
            var customAttribute = new CustomAttribute(_wellKnownTypes.SystemRuntimeCompilerServicesNullableAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.IsArray));
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(new ArrayType(_wellKnownTypes.TypeSystem.Byte), value));
            return customAttribute;
        }

        public CustomAttribute NullableContext(byte value)
        {
            var customAttribute = new CustomAttribute(_wellKnownTypes.SystemRuntimeCompilerServicesNullableContextAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 1));
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_wellKnownTypes.TypeSystem.Byte, value));
            return customAttribute;
        }

        public CustomAttribute NullablePublicOnly(bool value)
        {
            var customAttribute = new CustomAttribute(_wellKnownTypes.SystemRuntimeCompilerServicesNullablePublicOnlyAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 1));
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_wellKnownTypes.TypeSystem.Boolean, value));
            return customAttribute;
        }

        public CustomAttribute AttributeUsage(AttributeTargets validOn, bool? allowMultiple = null, bool? inherited = null)
        {
            MethodDefinition constructor = _wellKnownTypes.SystemAttributeUsageAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 1);
            var customAttribute = new CustomAttribute(_wellKnownTypes.Module.ImportReference(constructor));
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_wellKnownTypes.SystemAttributeTargets, (int)validOn));
            if (allowMultiple is object)
                customAttribute.Properties.Add(new CustomAttributeNamedArgument(nameof(AttributeUsageAttribute.AllowMultiple), new CustomAttributeArgument(_wellKnownTypes.TypeSystem.Boolean, allowMultiple!.Value)));

            if (inherited is object)
                customAttribute.Properties.Add(new CustomAttributeNamedArgument(nameof(AttributeUsageAttribute.Inherited), new CustomAttributeArgument(_wellKnownTypes.TypeSystem.Boolean, inherited!.Value)));

            return customAttribute;
        }

        public CustomAttribute ParamArray()
        {
            MethodDefinition constructor = _wellKnownTypes.SystemParamArrayAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            var customAttribute = new CustomAttribute(_wellKnownTypes.Module.ImportReference(constructor));
            return customAttribute;
        }

        public CustomAttribute ReferenceAssembly()
        {
            MethodDefinition constructor = _wellKnownTypes.SystemRuntimeCompilerServicesReferenceAssemblyAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            var customAttribute = new CustomAttribute(_wellKnownTypes.Module.ImportReference(constructor));
            return customAttribute;
        }
    }
}
