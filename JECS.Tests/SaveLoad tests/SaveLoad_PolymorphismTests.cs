using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JECS.Tests
{
    [TestClass]
    public class SaveLoad_PolymorphismTests
    {
        [TestMethod]
        public void SaveLoad_Polymorphism()
        {
            AbstractBaseClass value = new ChildClass1()
                {
                    StringValue = "test1",
                    IntValue = 532,
                };

            TestUtilities.PerformSaveLoadTest<AbstractBaseClass>(value);
        }
        
        [TestMethod]
        public void SaveLoad_Interface()
        {
            IBaseClassInterface value = new ChildClass1()
            {
                StringValue = "test1",
                IntValue = 532,
            };

            TestUtilities.PerformSaveLoadTest<IBaseClassInterface>(value);
        }
        
        [TestMethod]
        public void SaveLoad_Polymorphism_Null()
        {
            TestUtilities.PerformSaveLoadTest<AbstractBaseClass>(null);
        }
        
        [TestMethod]
        public void SaveLoad_Interface_Null()
        {
            TestUtilities.PerformSaveLoadTest<IBaseClassInterface>(null);
        }
        
        
        [TestMethod]
        public void SaveLoad_PolymorphismArray()
        {
            var array = new AbstractBaseClass[]
            {
                new ChildClass1()
                {
                    StringValue = "test1",
                    IntValue = 532,
                },
                new ChildClass2()
                {
                    StringValue = "test222",
                    FloatValue = 101.0101f,
                },
                null,
            };

            TestUtilities.PerformSaveLoadTest(array, CollectionAssert.AreEqual);
        }
        
        [TestMethod]
        public void SaveLoad_InterfaceArray()
        {
            var array = new IBaseClassInterface[]
            {
                new ChildClass1()
                {
                    StringValue = "test1",
                    IntValue = 532,
                },
                new ChildClass2()
                {
                    StringValue = "test222",
                    FloatValue = 101.0101f,
                },
                null,
            };

            TestUtilities.PerformSaveLoadTest(array, CollectionAssert.AreEqual);
        }



        interface IBaseClassInterface
        {
        }
        abstract class AbstractBaseClass : IBaseClassInterface
        {
            public string StringValue { get; set; }
        }
        class ChildClass1 : AbstractBaseClass
        {
            public int IntValue { get; set; }

            public override bool Equals(object obj)
                => obj is ChildClass1 other && StringValue == other.StringValue && IntValue == other.IntValue;
        }
        class ChildClass2 : AbstractBaseClass
        {
            public float FloatValue { get; set; }

            public override bool Equals(object obj)
                => obj is ChildClass2 other && StringValue == other.StringValue && FloatValue == other.FloatValue;
        }
    }
}
