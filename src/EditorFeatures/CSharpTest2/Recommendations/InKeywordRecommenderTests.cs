﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Recommendations
{
    public class InKeywordRecommenderTests : KeywordRecommenderTests
    {
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotAtRoot_Interactive()
        {
            await VerifyAbsenceAsync(SourceCodeKind.Script,
@"$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotAfterClass_Interactive()
        {
            await VerifyAbsenceAsync(SourceCodeKind.Script,
@"class C { }
$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotAfterGlobalStatement_Interactive()
        {
            await VerifyAbsenceAsync(SourceCodeKind.Script,
@"System.Console.WriteLine();
$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotAfterGlobalVariableDeclaration_Interactive()
        {
            await VerifyAbsenceAsync(SourceCodeKind.Script,
@"int i = 0;
$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInUsingAlias()
        {
            await VerifyAbsenceAsync(
@"using Goo = $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInGlobalUsingAlias()
        {
            await VerifyAbsenceAsync(
@"global using Goo = $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInEmptyStatement()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"$$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotAfterFrom()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"var q = from $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestAfterFromIdentifier()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"var q = from x $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestAfterFromAndTypeAndIdentifier()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"var q = from int x $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotAfterJoin()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"var q = from x in y
          join $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestAfterJoinIdentifier()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"var q = from x in y
          join z $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestAfterJoinAndTypeAndIdentifier()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"var q = from x in y
          join int z $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestAfterJoinNotAfterIn()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"var q = from x in y
          join z in $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        [WorkItem(544158, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544158")]
        public async Task TestNotAfterJoinPredefinedType()
        {
            await VerifyAbsenceAsync(
@"using System;
using System.Linq;
class C {
    void M()
    {
        var q = from x in y
                join int $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        [WorkItem(544158, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544158")]
        public async Task TestNotAfterJoinType()
        {
            await VerifyAbsenceAsync(
@"using System;
using System.Linq;
class C {
    void M()
    {
        var q = from x in y
                join Int32 $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInForEach()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"foreach (var v $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInForEach1()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"foreach (var v $$ c"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInForEach2()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"foreach (var v $$ c"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInForEach()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"foreach ($$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInForEach1()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"foreach (var $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInForEach2()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"foreach (var v in $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInForEach3()
        {
            await VerifyAbsenceAsync(AddInsideMethod(
@"foreach (var v in c $$"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInterfaceTypeVarianceAfterAngle()
        {
            await VerifyKeywordAsync(
@"interface IGoo<$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInterfaceTypeVarianceNotAfterIn()
        {
            await VerifyAbsenceAsync(
@"interface IGoo<in $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInterfaceTypeVarianceAfterComma()
        {
            await VerifyKeywordAsync(
@"interface IGoo<Goo, $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInterfaceTypeVarianceAfterAttribute()
        {
            await VerifyKeywordAsync(
@"interface IGoo<[Goo]$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestDelegateTypeVarianceAfterAngle()
        {
            await VerifyKeywordAsync(
@"delegate void D<$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestDelegateTypeVarianceAfterComma()
        {
            await VerifyKeywordAsync(
@"delegate void D<Goo, $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestDelegateTypeVarianceAfterAttribute()
        {
            await VerifyKeywordAsync(
@"delegate void D<[Goo]$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInClassTypeVarianceAfterAngle()
        {
            await VerifyAbsenceAsync(
@"class IGoo<$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInStructTypeVarianceAfterAngle()
        {
            await VerifyAbsenceAsync(
@"struct IGoo<$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInBaseListAfterAngle()
        {
            await VerifyAbsenceAsync(
@"interface IGoo : Bar<$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestNotInGenericMethod()
        {
            await VerifyAbsenceAsync(
@"interface IGoo {
    void Goo<$$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestFrom2()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"var q2 = from int x $$ ((IEnumerable)src))"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestFrom3()
        {
            await VerifyKeywordAsync(AddInsideMethod(
@"var q2 = from x $$ ((IEnumerable)src))"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        [WorkItem(544158, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544158")]
        public async Task TestNotAfterFromPredefinedType()
        {
            await VerifyAbsenceAsync(
@"using System;
using System.Linq;
class C {
    void M()
    {
        var q = from int $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        [WorkItem(544158, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544158")]
        public async Task TestNotAfterFromType()
        {
            await VerifyAbsenceAsync(
@"using System;
using System.Linq;
class C {
    void M()
    {
        var q = from Int32 $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsParameterModifierInMethods()
        {
            await VerifyKeywordAsync(@"
class Program
{
    public static void Test($$ p) { }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsParameterModifierInSecondParameter()
        {
            await VerifyKeywordAsync(@"
class Program
{
    public static void Test(int p1, $$ p2) { }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsParameterModifierInDelegates()
        {
            await VerifyKeywordAsync(@"
public delegate int Delegate($$ int p);");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsParameterModifierInLocalFunctions()
        {
            await VerifyKeywordAsync(@"
class Program
{
    public static void Test()
    {
        void localFunc($$ int p) { }
    }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsParameterModifierInLambdaExpressions()
        {
            await VerifyKeywordAsync(@"
public delegate int Delegate(in int p);

class Program
{
    public static void Test()
    {
        Delegate lambda = ($$ int p) => p;
    }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsParameterModifierInAnonymousMethods()
        {
            await VerifyKeywordAsync(@"
public delegate int Delegate(in int p);

class Program
{
    public static void Test()
    {
        Delegate anonymousDelegate = delegate ($$ int p) { return p; };
    }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsModifierInMethodReturnTypes()
        {
            await VerifyAbsenceAsync(@"
class Program
{
    public $$ int Test()
    {
        return ref x;
    }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsModifierInGlobalMemberDeclaration()
        {
            await VerifyAbsenceAsync(SourceCodeKind.Script, @"
public $$ ");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsModifierInDelegateReturnType()
        {
            await VerifyAbsenceAsync(@"
public delegate $$ int Delegate();

class Program
{
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task TestInAsModifierInMemberDeclaration()
        {
            await VerifyAbsenceAsync(@"
class Program
{
    public $$ int Test { get; set; }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInMethodFirstArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M() {
        Call($$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInMethodSecondArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M(object arg1) {
        Call(arg1, $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInBaseCallFirstArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C() : base($$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInBaseCallSecondArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C(object arg1) : base(arg1, $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInThisCallFirstArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C() : this($$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInThisCallSecondArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C(object arg1) : this(arg1, $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        [WorkItem(24079, "https://github.com/dotnet/roslyn/issues/24079")]
        public async Task TestInAsParameterModifierInConversionOperators()
        {
            await VerifyKeywordAsync(@"
class Program
{
    public static explicit operator double($$) { }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        [WorkItem(24079, "https://github.com/dotnet/roslyn/issues/24079")]
        public async Task TestInAsParameterModifierInBinaryOperators()
        {
            await VerifyKeywordAsync(@"
class Program
{
    public static Program operator +($$) { }
}");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInConstructorCallFirstArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M() {
        new MyType($$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInConstructorSecondArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M(object arg1) {
        new MyType(arg1, $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInMethodFirstNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M() {
        Call(a: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInMethodSecondNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M(object arg1) {
        Call(a: arg1, b: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInBaseCallFirstNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C() : base(a: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInBaseCallSecondNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C(object arg1) : base(a: arg1, b: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInThisCallFirstNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C() : this(a: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInThisCallSecondNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    public C(object arg1) : this(a: arg1, b: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInConstructorCallFirstNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M() {
        new MyType(a: $$");
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInConstructorSecondNamedArgumentModifier()
        {
            await VerifyKeywordAsync(@"
class C {
    void M(object arg1) {
        new MyType(a: arg1, b: $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_FirstParameter()
        {
            await VerifyKeywordAsync(
@"static class Extensions {
    static void Extension($$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        [WorkItem(30339, "https://github.com/dotnet/roslyn/issues/30339")]
        public async Task TestExtensionMethods_FirstParameter_AfterThisKeyword()
        {
            await VerifyKeywordAsync(
@"static class Extensions {
    static void Extension(this $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_SecondParameter()
        {
            await VerifyKeywordAsync(
@"static class Extensions {
    static void Extension(this int i, $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_SecondParameter_AfterThisKeyword()
        {
            await VerifyAbsenceAsync(
@"static class Extensions {
    static void Extension(this int i, this $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_FirstParameter_NonStaticClass()
        {
            await VerifyKeywordAsync(
@"class Extensions {
    static void Extension($$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_FirstParameter_AfterThisKeyword_NonStaticClass()
        {
            await VerifyAbsenceAsync(
@"class Extensions {
    static void Extension(this $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_SecondParameter_NonStaticClass()
        {
            await VerifyKeywordAsync(
@"class Extensions {
    static void Extension(this int i, $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_SecondParameter_AfterThisKeyword_NonStaticClass()
        {
            await VerifyAbsenceAsync(
@"class Extensions {
    static void Extension(this int i, this $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_FirstParameter_NonStaticMethod()
        {
            await VerifyKeywordAsync(
@"static class Extensions {
    void Extension($$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_FirstParameter_AfterThisKeyword_NonStaticMethod()
        {
            await VerifyAbsenceAsync(
@"static class Extensions {
    void Extension(this $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_SecondParameter_NonStaticMethod()
        {
            await VerifyKeywordAsync(
@"static class Extensions {
    void Extension(this int i, $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestExtensionMethods_SecondParameter_AfterThisKeyword_NonStaticMethod()
        {
            await VerifyAbsenceAsync(
@"static class Extensions {
    void Extension(this int i, this $$");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        public async Task TestInFunctionPointerTypeNoExistingModifiers()
        {
            await VerifyKeywordAsync(@"
class C
{
    delegate*<$$");
        }

        [Theory, Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
        [InlineData("in")]
        [InlineData("out")]
        [InlineData("ref")]
        [InlineData("ref readonly")]
        public async Task TestNotInFunctionPointerTypeExistingModifiers(string modifier)
        {
            await VerifyAbsenceAsync($@"
class C
{{
    delegate*<{modifier} $$");
        }
    }
}
