

using Base.Helper;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.UnitTests
{

    class Person
    {
        public string Name { get; set; } = "";
    }

    [TestClass]
    public class MiniMapperTests
    {

        [TestMethod]
        public void AnyPropertyValuesDifferent_PropertiesAreEqual_ShouldReturnFalse()
        {
            var obj1 = new { Name = "Walter" };
            var obj2 = new { Name = "Walter" };
            var isDifferent = MiniMapper.AnyPropertyValuesDifferent(obj1, obj2);
            Assert.IsFalse(isDifferent);
        }

        [TestMethod]
        public void AnyPropertyValuesDifferent_PropertiesAreNotEqual_ShouldReturnTrue()
        {
            var obj1 = new { Name = "Walter" };
            var obj2 = new { Name = "Walter1" };
            var isDifferent = MiniMapper.AnyPropertyValuesDifferent(obj1, obj2);
            Assert.IsTrue(isDifferent);
        }

        [TestMethod]
        public void AnyPropertyValuesDifferent_PropertiesHaveDifferentTypes_ShouldReturnFalse()
        {
            var obj1 = new { Name = "Walter" };
            var obj2 = new { Name = 4711 };
            var isDifferent = MiniMapper.AnyPropertyValuesDifferent(obj1, obj2);
            Assert.IsFalse(isDifferent);
        }

        [TestMethod]
        public void CopyProperties_SetTargetToEmpty_ShouldCopyEmpty()
        {
            var obj1 = new Person();
            var obj2 = new Person { Name = "Walter" };
            MiniMapper.CopyProperties(obj2, obj1);
            Assert.AreEqual("", obj2.Name);
        }



    }
}
