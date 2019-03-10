using System;
using System.Collections.Generic;
using System.Linq;
using SUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SUCC.Tests
{
    [TestClass]
    public class ComplexTypeTests
    {
        static DataFile file = new DataFile("tests/" + nameof(ComplexTypeTests), autoSave: false);

        [TestMethod]
        public void ComplexTypeTest()
        {
            file.Set(nameof(TestComplexTypes), TestComplexTypes);

            if (TestUtilities.SaveFiles)
                file.SaveAllData();

            var loaded = file.Get<ComplexType[]>(nameof(TestComplexTypes));

            CollectionAssert.AreEqual(TestComplexTypes, loaded);
        }


        class ComplexType
        {
            public ComplexType() { }
            public ComplexType(int integer, string text, bool boolean)
            {
                Integer = integer;
                String = text;
                Boolean = boolean;
            }

            public int Integer;
            [DoSave] private string String;
            [DoSave] public bool Boolean { get; private set; }

            public override bool Equals(object obj)
            {
                var other = (ComplexType)obj;
                if (other == null) return false;

                return this.Integer == other.Integer 
                    && this.String == other.String 
                    && this.Boolean == other.Boolean;
            }
        }

        static readonly ComplexType[] TestComplexTypes = new ComplexType[]
        {
            new ComplexType(69, "sugandese nuts lmao", true),
            new ComplexType(123123, "testing test test test test test teeeeeeeeeeest", false),
            new ComplexType(897897897, "hfkdsla;hfjdksafhdsjkfhdsjkfdsjhka;fa;fd'l6324783274^&%D^&$&#*%^&*%&^(", true),
        };
    }
}