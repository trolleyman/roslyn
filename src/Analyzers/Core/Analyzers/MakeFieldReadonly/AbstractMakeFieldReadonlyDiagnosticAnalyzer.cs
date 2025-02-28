﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.MakeFieldReadonly
{
    internal abstract class AbstractMakeFieldReadonlyDiagnosticAnalyzer<
        TSyntaxKind, TThisExpression>
        : AbstractBuiltInCodeStyleDiagnosticAnalyzer
        where TSyntaxKind : struct, Enum
        where TThisExpression : SyntaxNode
    {
        protected AbstractMakeFieldReadonlyDiagnosticAnalyzer()
            : base(
                IDEDiagnosticIds.MakeFieldReadonlyDiagnosticId,
                EnforceOnBuildValues.MakeFieldReadonly,
                CodeStyleOptions2.PreferReadonly,
                new LocalizableResourceString(nameof(AnalyzersResources.Add_readonly_modifier), AnalyzersResources.ResourceManager, typeof(AnalyzersResources)),
                new LocalizableResourceString(nameof(AnalyzersResources.Make_field_readonly), AnalyzersResources.ResourceManager, typeof(AnalyzersResources)))
        {
        }

        protected abstract ISyntaxKinds SyntaxKinds { get; }
        protected abstract bool IsWrittenTo(SemanticModel semanticModel, TThisExpression expression, CancellationToken cancellationToken);

        public override DiagnosticAnalyzerCategory GetAnalyzerCategory() => DiagnosticAnalyzerCategory.SemanticDocumentAnalysis;

        // We need to analyze generated code to get callbacks for read/writes to non-generated members in generated code.
        protected override GeneratedCodeAnalysisFlags GeneratedCodeAnalysisFlags => GeneratedCodeAnalysisFlags.Analyze;

        protected override void InitializeWorker(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(context =>
            {
                // State map for fields:
                //  'isCandidate' : Indicates whether the field is a candidate to be made readonly based on it's options.
                //  'written'     : Indicates if there are any writes to the field outside the constructor and field initializer.
                var fieldStateMap = new ConcurrentDictionary<IFieldSymbol, (bool isCandidate, bool written)>();

                var threadStaticAttribute = context.Compilation.ThreadStaticAttributeType();
                var dataContractAttribute = context.Compilation.DataContractAttribute();
                var dataMemberAttribute = context.Compilation.DataMemberAttribute();

                // We register following actions in the compilation:
                // 1. A symbol action for field symbols to ensure the field state is initialized for every field in
                //    the compilation.
                // 2. An operation action for field references to detect if a candidate field is written outside
                //    constructor and field initializer, and update field state accordingly.
                // 3. A symbol start/end action for named types to report diagnostics for candidate fields that were
                //    not written outside constructor and field initializer.

                context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);

                context.RegisterSymbolStartAction(context =>
                {
                    context.RegisterOperationAction(AnalyzeOperation, OperationKind.FieldReference);

                    // Can't allow changing the fields to readonly if the struct overwrites itself.  e.g. `this = default;`
                    var writesToThis = false;
                    context.RegisterSyntaxNodeAction(context =>
                    {
                        writesToThis = writesToThis || IsWrittenTo(context.SemanticModel, (TThisExpression)context.Node, context.CancellationToken);
                    }, SyntaxKinds.Convert<TSyntaxKind>(SyntaxKinds.ThisExpression));

                    context.RegisterSymbolEndAction(context =>
                    {
                        if (writesToThis)
                            return;

                        OnSymbolEnd(context);
                    });
                }, SymbolKind.NamedType);

                return;

                // Local functions.
                void AnalyzeFieldSymbol(SymbolAnalysisContext symbolContext)
                {
                    _ = TryGetOrInitializeFieldState((IFieldSymbol)symbolContext.Symbol, symbolContext.Options, symbolContext.CancellationToken);
                }

                void AnalyzeOperation(OperationAnalysisContext operationContext)
                {
                    var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                    var (isCandidate, written) = TryGetOrInitializeFieldState(fieldReference.Field, operationContext.Options, operationContext.CancellationToken);

                    // Ignore fields that are not candidates or have already been written outside the constructor/field initializer.
                    if (!isCandidate || written)
                    {
                        return;
                    }

                    // Check if this is a field write outside constructor and field initializer, and update field state accordingly.
                    if (IsFieldWrite(fieldReference, operationContext.ContainingSymbol))
                    {
                        UpdateFieldStateOnWrite(fieldReference.Field);
                    }
                }

                void OnSymbolEnd(SymbolAnalysisContext symbolEndContext)
                {
                    // Report diagnostics for candidate fields that are not written outside constructor and field initializer.
                    var members = ((INamedTypeSymbol)symbolEndContext.Symbol).GetMembers();
                    foreach (var member in members)
                    {
                        if (member is IFieldSymbol field && fieldStateMap.TryRemove(field, out var value))
                        {
                            var (isCandidate, written) = value;
                            if (isCandidate && !written)
                            {
                                var option = GetCodeStyleOption(field, symbolEndContext.Options);
                                var diagnostic = DiagnosticHelper.Create(
                                    Descriptor,
                                    field.Locations[0],
                                    option.Notification.Severity,
                                    additionalLocations: null,
                                    properties: null);
                                symbolEndContext.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }

                static bool IsCandidateField(IFieldSymbol symbol, INamedTypeSymbol? threadStaticAttribute, INamedTypeSymbol? dataContractAttribute, INamedTypeSymbol? dataMemberAttribute) =>
                        symbol is
                        {
                            DeclaredAccessibility: Accessibility.Private,
                            IsReadOnly: false,
                            IsConst: false,
                            IsImplicitlyDeclared: false,
                            Locations.Length: 1,
                            IsFixedSizeBuffer: false,
                        } &&
                        symbol.Type.IsMutableValueType() == false &&
                        !symbol.GetAttributes().Any(
                           static (a, threadStaticAttribute) => SymbolEqualityComparer.Default.Equals(a.AttributeClass, threadStaticAttribute),
                           threadStaticAttribute) &&
                        !IsDataContractSerializable(symbol, dataContractAttribute, dataMemberAttribute);

                static bool IsDataContractSerializable(IFieldSymbol symbol, INamedTypeSymbol? dataContractAttribute, INamedTypeSymbol? dataMemberAttribute)
                {
                    if (dataContractAttribute is null || dataMemberAttribute is null)
                        return false;

                    return symbol.GetAttributes().Any(static (x, dataMemberAttribute) => SymbolEqualityComparer.Default.Equals(x.AttributeClass, dataMemberAttribute), dataMemberAttribute)
                        && symbol.ContainingType.GetAttributes().Any(static (x, dataContractAttribute) => SymbolEqualityComparer.Default.Equals(x.AttributeClass, dataContractAttribute), dataContractAttribute);
                }

                // Method to update the field state for a candidate field written outside constructor and field initializer.
                void UpdateFieldStateOnWrite(IFieldSymbol field)
                {
                    Debug.Assert(IsCandidateField(field, threadStaticAttribute, dataContractAttribute, dataMemberAttribute));
                    Debug.Assert(fieldStateMap.ContainsKey(field));

                    fieldStateMap[field] = (isCandidate: true, written: true);
                }

                // Method to get or initialize the field state.
                (bool isCandidate, bool written) TryGetOrInitializeFieldState(IFieldSymbol fieldSymbol, AnalyzerOptions options, CancellationToken cancellationToken)
                {
                    if (!IsCandidateField(fieldSymbol, threadStaticAttribute, dataContractAttribute, dataMemberAttribute))
                    {
                        return default;
                    }

                    if (fieldStateMap.TryGetValue(fieldSymbol, out var result))
                    {
                        return result;
                    }

                    result = ComputeInitialFieldState(fieldSymbol, options, threadStaticAttribute, dataContractAttribute, dataMemberAttribute, cancellationToken);
                    return fieldStateMap.GetOrAdd(fieldSymbol, result);
                }

                // Method to compute the initial field state.
                static (bool isCandidate, bool written) ComputeInitialFieldState(
                    IFieldSymbol field,
                    AnalyzerOptions options,
                    INamedTypeSymbol? threadStaticAttribute,
                    INamedTypeSymbol? dataContractAttribute,
                    INamedTypeSymbol? dataMemberAttribute,
                    CancellationToken cancellationToken)
                {
                    Debug.Assert(IsCandidateField(field, threadStaticAttribute, dataContractAttribute, dataMemberAttribute));

                    var option = GetCodeStyleOption(field, options);
                    if (option == null || !option.Value)
                    {
                        return default;
                    }

                    return (isCandidate: true, written: false);
                }
            });
        }

        private static bool IsFieldWrite(IFieldReferenceOperation fieldReference, ISymbol owningSymbol)
        {
            // Check if the underlying member is being written or a writable reference to the member is taken.
            var valueUsageInfo = fieldReference.GetValueUsageInfo(owningSymbol);
            if (!valueUsageInfo.IsWrittenTo())
            {
                return false;
            }

            // Writes to fields inside constructor are ignored, except for the below cases:
            //  1. Instance reference of an instance field being written is not the instance being initialized by the constructor.
            //  2. Field is being written inside a lambda or local function.

            // Check if we are in the constructor of the containing type of the written field.
            var isInConstructor = owningSymbol.IsConstructor();
            var isInStaticConstructor = owningSymbol.IsStaticConstructor();
            var field = fieldReference.Field;
            if ((isInConstructor || isInStaticConstructor) &&
                field.ContainingType == owningSymbol.ContainingType)
            {
                // For instance fields, ensure that the instance reference is being initialized by the constructor.
                var instanceFieldWrittenInCtor = isInConstructor &&
                    fieldReference.Instance?.Kind == OperationKind.InstanceReference &&
                    !fieldReference.IsTargetOfObjectMemberInitializer();

                // For static fields, ensure that we are in the static constructor.
                var staticFieldWrittenInStaticCtor = isInStaticConstructor && field.IsStatic;

                if (instanceFieldWrittenInCtor || staticFieldWrittenInStaticCtor)
                {
                    // Finally, ensure that the write is not inside a lambda or local function.
                    if (fieldReference.TryGetContainingAnonymousFunctionOrLocalFunction() is null)
                    {
                        // It is safe to ignore this write.
                        return false;
                    }
                }
            }

            return true;
        }

        private static CodeStyleOption2<bool> GetCodeStyleOption(IFieldSymbol field, AnalyzerOptions options)
            => options.GetAnalyzerOptions(field.Locations[0].SourceTree!).PreferReadonly;
    }
}
