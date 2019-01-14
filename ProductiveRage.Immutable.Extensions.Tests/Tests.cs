﻿using System.Collections.Generic;
using Bridge;
using Bridge.QUnit;

namespace ProductiveRage.Immutable.Extensions.Tests
{
	public static class Tests
	{
		[Ready]
		public static void Go()
		{
			// The NonBlankTrimmedString class was changed in December 2018 to be an [ObjectLiteral] because it improves deserialisation time for API responses that include
			// many string instance because instantiating [ObjectLiteral] types seems to be much faster than "full" types. However, the support in Bridge for [ObjectLiteral]
			// seems to be a little patchy (some of which is discussed in https://forums.bridge.net/forum/community/help/6001) and so I've added some hacks to this library
			// to improve the support. These tests exercise those hacks (and should help highlight any regressions in case the Bridge internals change in the future in a
			// way that invalidates any of the approachs in the hacks).
			ToStringTests();
			EqualsTests();
			DictionaryTests();
		}

		private static void ToStringTests()
		{
			QUnit.Module("ToString()");

			QUnit.Test("NonBlankTrimmedString ToString() call works as expected", assert => assert.Equal(new NonBlankTrimmedString("xyz").ToString(), "xyz"));
			QUnit.Test("NonBlankTrimmedString in string interpolation works as expected", assert => assert.Equal($"{new NonBlankTrimmedString("xyz")}", "xyz"));
			QUnit.Test("NonBlankTrimmedString in string concatenation works as expected", assert => assert.Equal("" + new NonBlankTrimmedString("xyz"), "xyz"));
			QUnit.Test("[ObjectLiteral]-as-generic-type-param's ToString() call works as expected", assert =>
			{
				// This is part of the second reproduce case in https://forums.bridge.net/forum/community/help/6001
				var x = new MultiLanguageTextBoxModel(new LangKey(1));
				assert.Equal(x.TestValue, "1");
			});
		}

		private static void EqualsTests()
		{
			QUnit.Module("Equals(..)");

			QUnit.Test("An instance of NonBlankTrimmedString is found to be equal to itself", assert =>
			{
				var x = new NonBlankTrimmedString("xyz");
				AssertEqualsViaNonBlankTrimmedStringEqualsCall(assert, x, x);
			});
			QUnit.Test("Two instances of NonBlankTrimmedString with the same value are found to be equal when compared as NonBlankTrimmedString", assert =>
			{
				var x = new NonBlankTrimmedString("xyz");
				var y = new NonBlankTrimmedString("xyz");
				AssertEqualsViaNonBlankTrimmedStringEqualsCall(assert, x, y);
			});
			QUnit.Test("Two instances of NonBlankTrimmedString with the same value are found to be equal when compared as generic type param of NonBlankTrimmedString", assert =>
			{
				var x = new NonBlankTrimmedString("xyz");
				var y = new NonBlankTrimmedString("xyz");
				AssertEqualsViaSharedGenericTypeEqualsCall(assert, x, y);
			});
			QUnit.Test("Two instances of NonBlankTrimmedString with the same value are found to be equal when compared as Object", assert =>
			{
				var x = new NonBlankTrimmedString("xyz");
				var y = new NonBlankTrimmedString("xyz");
				AssertEqualsViaSharedGenericTypeEqualsCall(assert, x, y);
			});
		}

		private static void DictionaryTests()
		{
			QUnit.Module("Dictionary lookups");

			QUnit.Test("Two instances of NonBlankTrimmedString with the same value are found to be equal when used as a Dictionary key", assert =>
			{
				// This will test both the bridge.getHashCode AND the bridge.equals hacks
				var d = new Dictionary<NonBlankTrimmedString, int>
				{
					{ new NonBlankTrimmedString("xyz"), 123 }
				};
				assert.Ok(d.ContainsKey(new NonBlankTrimmedString("xyz")));
			});
		}

		private static void AssertEqualsViaObjectEqualsCall(Assert assert, object x, object y)
		{
			if ((x == null) && (y == null))
				return;

			assert.Ok((x != null) && (y != null), "Unless both x and y are null, they must both NOT be null in order to be equal");
			assert.Ok(x.Equals(y));
		}

		private static void AssertEqualsViaNonBlankTrimmedStringEqualsCall(Assert assert, NonBlankTrimmedString x, NonBlankTrimmedString y)
		{
			if ((x == null) && (y == null))
				return;

			assert.Ok((x != null) && (y != null), "Unless both x and y are null, they must both NOT be null in order to be equal");
			assert.Ok(x.Equals(y));
		}

		private static void AssertEqualsViaSharedGenericTypeEqualsCall<T>(Assert assert, T x, T y) where T : NonBlankTrimmedString
		{
			if ((x == null) && (y == null))
				return;

			assert.Ok((x != null) && (y != null), "Unless both x and y are null, they must both NOT be null in order to be equal");
			assert.Ok(x.Equals(y));
		}

		// This class is part of the second reproduce case in https://forums.bridge.net/forum/community/help/6001
		[ObjectLiteral(ObjectCreateMode.Constructor)]
		public struct LangKey
		{
			public LangKey(int value) { Value = value; }
			public int Value { get; }
			public override string ToString() => Value.ToString();
		}
		
		// This class is part of the second reproduce case in https://forums.bridge.net/forum/community/help/6001
		public sealed class MultiLanguageTextBoxModel : AbstractMultiLanguageTextBoxModel<LangKey>
		{
			public MultiLanguageTextBoxModel(LangKey selected) : base(selected) { }
		}

		// This class is part of the second reproduce case in https://forums.bridge.net/forum/community/help/6001
		public abstract class AbstractMultiLanguageTextBoxModel<TKey>
		{
			public AbstractMultiLanguageTextBoxModel(TKey selected)
			{
				Selected = selected;
				TestValue = Selected.ToString();
			}
			public TKey Selected { get; }
			public string TestValue { get; }
		}
	}
}