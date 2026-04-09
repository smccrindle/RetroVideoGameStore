using RetroVideoGameStore.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroVideoGameTestPractice
{
    [TestClass]
    public class ExampleControllerTest
    {
        [TestMethod]
        public void IndexReturnsSomething()
        {
            // Arrange
            var controller = new ExampleController();
            // Act: call the Index() method and store what comes back
            var result = controller.Index();
            // Assert
            Assert.IsNotNull(result);
        }
    }
}
