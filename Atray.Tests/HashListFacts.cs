namespace Atray.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Linq;

    using NUnit.Framework;
    using ATray.Activity;

    class GenericComparer<T> : IComparer<T>
    {

        private bool called = false;

        public bool Called
        {
            get
            {
                var result = called;
                called = false;
                return called;
            }
        }

        public int Compare(T x, T y)
        {
            called = true;
            return 0;
        }
    }

    [TestFixture]
    public class ListTest
    {
        static byte[] _serializedList = new byte[] {
            0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00,
            0x7e, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f, 0x6c,
            0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x47, 0x65,
            0x6e, 0x65, 0x72, 0x69, 0x63, 0x2e, 0x4c, 0x69, 0x73, 0x74, 0x60,
            0x31, 0x5b, 0x5b, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x49,
            0x6e, 0x74, 0x33, 0x32, 0x2c, 0x20, 0x6d, 0x73, 0x63, 0x6f, 0x72,
            0x6c, 0x69, 0x62, 0x2c, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6f,
            0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x30, 0x2c, 0x20,
            0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65, 0x3d, 0x6e, 0x65, 0x75,
            0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50, 0x75, 0x62, 0x6c, 0x69,
            0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b, 0x65, 0x6e, 0x3d, 0x62,
            0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34,
            0x65, 0x30, 0x38, 0x39, 0x5d, 0x5d, 0x03, 0x00, 0x00, 0x00, 0x06,
            0x5f, 0x69, 0x74, 0x65, 0x6d, 0x73, 0x05, 0x5f, 0x73, 0x69, 0x7a,
            0x65, 0x08, 0x5f, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x07,
            0x00, 0x00, 0x08, 0x08, 0x08, 0x09, 0x02, 0x00, 0x00, 0x00, 0x03,
            0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x0f, 0x02, 0x00, 0x00,
            0x00, 0x04, 0x00, 0x00, 0x00, 0x08, 0x05, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0b };
        int[] _list1_contents;
        HashList<int> _list1;

        [SetUp]
        public void SetUp()
        {
            // FIXME arrays currently do not support generic collection
            // interfaces
            _list1_contents = new int[] { 55, 50, 22, 80, 56, 52, 40, 63 };
            // _list1 = new List <int> (_list1_contents);

            _list1 = new HashList<int>();
            foreach (var i in _list1_contents)
                _list1.Add(i);
        }

        [Test]  // This was for bug #74980
        public void InsertTest()
        {
            var test = new HashList<string>();
            test.Insert(0, "a");
            test.Insert(0, "b");
            test.Insert(1, "c");

            Assert.AreEqual(3, test.Count);
            Assert.AreEqual("b", test[0]);
            Assert.AreEqual("c", test[1]);
            Assert.AreEqual("a", test[2]);
        }

        [Test]
        public void IndexOfTest()
        {
            var l = new HashList<int>
            {
                100,
                200
            };

            Assert.AreEqual(1, l.IndexOf(200), "Could not find value");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructWithInvalidCollectionTest()
        {
            HashList<int> n = null;
            var l1 = new HashList<int>(n);
        }

        [Test]
        public void AddTest()
        {
            var count = _list1.Count;
            _list1.Add(-1);
            Assert.AreEqual(count + 1, _list1.Count);
            Assert.AreEqual(-1, _list1[_list1.Count - 1]);
        }

        [Test]
        public void AddRangeTest()
        {
            var count = _list1.Count;
            // FIXME arrays currently do not support generic collection
            // interfaces
            int[] range = { -1, -2, -3 };
            var tmp = new HashList<int>();
            foreach (var i in range)
                tmp.Add(i);
            // _list1.AddRange (range);
            _list1.AddRange(tmp);

            Assert.AreEqual(count + 3, _list1.Count);
            Assert.AreEqual(-1, _list1[_list1.Count - 3]);
            Assert.AreEqual(-2, _list1[_list1.Count - 2]);
            Assert.AreEqual(-3, _list1[_list1.Count - 1]);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AddNullRangeTest()
        {
            int[] n = null;
            _list1.AddRange(n);
        }
        
        [Test]
        public void SortTestTrickyPivot()
        {
            var array = new int[] { 1, 3, 5, 2, 6, 6, 6, 6, 6, 6, 6, 7, 4 };

            var list = array.ToList<int>();

            list.Sort(delegate (int x, int y)
            {
                return x < y ? -1 : 1;
            });

            var res = string.Join(",", list);
            Assert.AreEqual("1,2,3,4,5,6,6,6,6,6,6,6,7", res);
        }

        [Test]
        public void ContainsTest()
        {
            Assert.IsTrue(_list1.Contains(22));
            Assert.IsFalse(_list1.Contains(23));
        }

        private string StringConvert(int i)
        {
            return i.ToString();
        }

        static bool FindMultipleOfThree(int i)
        {
            return (i % 3) == 0;
        }

        static bool FindMultipleOfFour(int i)
        {
            return (i % 4) == 0;
        }

        static bool FindMultipleOfTwelve(int i)
        {
            return (i % 12) == 0;
        }

        [Test]
        public void RemoveTest()
        {
            var count = _list1.Count;
            var result = _list1.Remove(22);
            Assert.IsTrue(result);
            Assert.AreEqual(count - 1, _list1.Count);

            Assert.AreEqual(-1, _list1.IndexOf(22));

            result = _list1.Remove(0);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveAtTest()
        {
            var count = _list1.Count;
            _list1.RemoveAt(0);
            Assert.AreEqual(count - 1, _list1.Count);
            Assert.AreEqual(50, _list1[0]);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RemoveOutOfRangeTest()
        {
            _list1.RemoveAt(_list1.Count);
        }
        [Test]
        public void ToArrayTest()
        {
            var copiedContents = _list1.ToArray();
            Assert.IsFalse(ReferenceEquals(copiedContents, _list1_contents));

            Assert.AreEqual(_list1.Count, copiedContents.Length);
            Assert.AreEqual(_list1[0], copiedContents[0]);
        }
        
        bool IsPositive(int i)
        {
            return i >= 0;
        }
        
        [Test]
        public void AddRange_Bug77019()
        {
            var l = new HashList<int>();
            var d = new Dictionary<string, int>();
            l.AddRange(d.Values);
            Assert.AreEqual(0, l.Count, "Count");
        }

        [Test]
        public void VersionCheck_Add()
        {
            var list = new HashList<int>();
            IEnumerator enumerator = list.GetEnumerator();
            list.Add(5);

            try
            {
                enumerator.MoveNext();
                Assert.Fail("#1");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                enumerator.Reset();
                Assert.Fail("#2");
            }
            catch (InvalidOperationException)
            {
            }

            enumerator = list.GetEnumerator();
            enumerator.MoveNext();
        }

        [Test]
        public void VersionCheck_AddRange()
        {
            var list = new HashList<int>();
            IEnumerator enumerator = list.GetEnumerator();
            list.AddRange(new int[] { 5, 7 });

            try
            {
                enumerator.MoveNext();
                Assert.Fail("#1");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                enumerator.Reset();
                Assert.Fail("#2");
            }
            catch (InvalidOperationException)
            {
            }

            enumerator = list.GetEnumerator();
            enumerator.MoveNext();
        }

        [Test]
        public void VersionCheck_Clear()
        {
            var list = new HashList<int>();
            IEnumerator enumerator = list.GetEnumerator();
            list.Clear();

            try
            {
                enumerator.MoveNext();
                Assert.Fail("#1");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                enumerator.Reset();
                Assert.Fail("#2");
            }
            catch (InvalidOperationException)
            {
            }

            enumerator = list.GetEnumerator();
            enumerator.MoveNext();
        }

        [Test]
        public void VersionCheck_Insert()
        {
            var list = new HashList<int>();
            IEnumerator enumerator = list.GetEnumerator();
            list.Insert(0, 7);

            try
            {
                enumerator.MoveNext();
                Assert.Fail("#1");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                enumerator.Reset();
                Assert.Fail("#2");
            }
            catch (InvalidOperationException)
            {
            }

            enumerator = list.GetEnumerator();
            enumerator.MoveNext();
        }

        [Test]
        public void VersionCheck_Remove()
        {
            var list = new HashList<int>
            {
                5
            };
            IEnumerator enumerator = list.GetEnumerator();
            // version number is not incremented if item does not exist in list
            list.Remove(7);
            enumerator.MoveNext();
            list.Remove(5);

            try
            {
                enumerator.MoveNext();
                Assert.Fail("#1");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                enumerator.Reset();
                Assert.Fail("#2");
            }
            catch (InvalidOperationException)
            {
            }

            enumerator = list.GetEnumerator();
            enumerator.MoveNext();
        }
        

        [Test]
        public void VersionCheck_RemoveAt()
        {
            var list = new HashList<int>
            {
                5
            };
            IEnumerator enumerator = list.GetEnumerator();
            list.RemoveAt(0);

            try
            {
                enumerator.MoveNext();
                Assert.Fail("#1");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                enumerator.Reset();
                Assert.Fail("#2");
            }
            catch (InvalidOperationException)
            {
            }

            enumerator = list.GetEnumerator();
            enumerator.MoveNext();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))] // #699182
        public void VersionCheck_Indexer()
        {
            var list = new HashList<int>() { 0, 2, 3 };
            var enumerator = list.GetEnumerator();

            list[0] = 1;

            enumerator.MoveNext();
        }
        

        class SortTestComparer : IComparer<string>
        {

            public int Compare(string s1, string s2)
            {
                return String.Compare(s1, s2);
            }
        }
        

        // for bug #77039 test case
        class GenericIComparable : IComparable<GenericIComparable>
        {
            private int _NumberToSortOn;

            public int NumberToSortOn
            {
                get { return _NumberToSortOn; }
                set { _NumberToSortOn = value; }
            }

            public GenericIComparable(int val)
            {
                _NumberToSortOn = val;
            }

            public int CompareTo(GenericIComparable other)
            {
                return NumberToSortOn.CompareTo(other.NumberToSortOn);
            }
        }
        

        class NonGenericIComparable : IComparable
        {
            private int _NumberToSortOn;

            public int NumberToSortOn
            {
                get { return _NumberToSortOn; }
                set { _NumberToSortOn = value; }
            }

            public NonGenericIComparable(int val)
            {
                _NumberToSortOn = val;
            }

            public int CompareTo(object obj)
            {
                return NumberToSortOn.CompareTo((obj as NonGenericIComparable).NumberToSortOn);
            }
        }
        
        // for bug #81387 test case
        [Test]
        public void Test_Contains_After_Remove()
        {
            var list = new HashList<int>
            {
                2
            };

            list.Remove(2);

            Assert.AreEqual(false, list.Contains(2), "#0");
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetItem_OutOfRange()
        {
            var list = new HashList<string>
            {
                [0] = "foo"
            };
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetItem_IList_OutOfRange()
        {
            IList<string> list = new HashList<string>
            {
                [0] = "foo"
            };
        }

        public class EquatableClass : IEquatable<EquatableClass>
        {
            int _x;
            public EquatableClass(int x)
            {
                _x = x;
            }

            public bool Equals(EquatableClass other)
            {
                return this._x == other._x;
            }
        }

        delegate void D();
        bool Throws(D d)
        {
            try
            {
                d();
                return false;
            }
            catch
            {
                return true;
            }
        }

        [Test]
        public void Enumerator_Reset()
        {
            var l = new HashList<int>() {
                4
            };

            var e = l.GetEnumerator();
            Assert.IsTrue(e.MoveNext(), "#1");
            Assert.AreEqual(4, e.Current, "#2");
            e.Reset();
            Assert.AreEqual(0, e.Current, "#3");
        }

        [Test]
        public void LastIndexOfEmpty_2558()
        {
            var l = new HashList<int>();
            Assert.AreEqual(-1, l.IndexOf(-1));
        }


        #region Enumerator mutability

        class Bar
        {
        }

        class Foo : IEnumerable<Bar>
        {
            Baz enumerator;

            public Foo()
            {
                enumerator = new Baz();
            }

            public IEnumerator<Bar> GetEnumerator()
            {
                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return enumerator;
            }
        }

        class Baz : IEnumerator<Bar>
        {
            public bool DisposeWasCalled = false;

            public void Dispose()
            {
                DisposeWasCalled = true;
            }

            public bool MoveNext()
            {
                return false; //assume empty collection
            }

            public void Reset()
            {
            }

            public Bar Current
            {
                get { return null; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        [Test]
        public void PremiseAboutDisposeBeingCalledWhenLooping()
        {
            var enumerable = new Foo();
            var enumerator = enumerable.GetEnumerator() as Baz;
            Assert.IsNotNull(enumerator);
            Assert.AreEqual(false, enumerator.DisposeWasCalled);
            foreach (var element in enumerable) ; //sic
            Assert.AreEqual(true, enumerator.DisposeWasCalled);
        }

        [Test]
        public void TwoEnumeratorsOfTwoDifferentListsAreDifferent()
        {
            var twoThree = new HashList<int> { 2, 3 };
            var oneTwo = new HashList<int> { 2, 4 };
            Assert.IsFalse(oneTwo.GetEnumerator().Equals(twoThree.GetEnumerator()));
        }

        [Test]
        public void TwoEnumeratorsOfTwoDifferentListsWithSameElementsAreDifferent()
        {
            var twoThree = new HashList<int> { 2, 3 };
            var anotherTwoThree = new HashList<int> { 2, 3 };
            Assert.IsFalse(twoThree.GetEnumerator().Equals(anotherTwoThree.GetEnumerator()));
        }

        [Test]
        public void EnumeratorIsSameInSameListAfterSubsequentCalls()
        {
            var enumerable = new HashList<Bar>();
            var enumerator = enumerable.GetEnumerator();
            var enumerator2 = enumerable.GetEnumerator();

            Assert.IsFalse(ReferenceEquals(enumerator2, enumerator)); //because they are value-types

            Assert.IsTrue(enumerator2.Equals(enumerator));
        }


        [Test] // was bug in Mono 2.10.9
        public void EnumeratorIsStillSameInSubsequentCallsEvenHavingADisposalInBetween()
        {
            var enumerable = new HashList<Bar>();
            var enumerator = enumerable.GetEnumerator();
            enumerator.Dispose();
            var enumerator2 = enumerable.GetEnumerator();

            Assert.IsFalse(ReferenceEquals(enumerator2, enumerator)); //because they are value-types

            Assert.IsTrue(enumerator2.Equals(enumerator));
        }

        [Test]
        public void EnumeratorIsObviouslyDifferentAfterListChanges()
        {
            var enumerable = new HashList<Bar>();
            var enumerator = enumerable.GetEnumerator();
            enumerable.Add(new Bar());
            var enumerator2 = enumerable.GetEnumerator();

            Assert.IsFalse(ReferenceEquals(enumerator2, enumerator)); //because they are value-types

            Assert.IsFalse(enumerator2.Equals(enumerator));
        }

        [Test] // was bug in Mono 2.10.9
        public void DotNetDoesntThrowObjectDisposedExceptionAfterSubsequentDisposes()
        {
            var enumerable = new HashList<Bar>();
            var enumerator = enumerable.GetEnumerator();
            Assert.AreEqual(false, enumerator.MoveNext());
            enumerator.Dispose();
            Assert.AreEqual(false, enumerator.MoveNext());
        }
        #endregion
    }
}
