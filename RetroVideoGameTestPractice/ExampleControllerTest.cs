using Microsoft.AspNetCore.Mvc;
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
        [TestMethod]
        public void IndexLoadsIndexView()
        {
            // Arrange - create instance of the ExampleController
            var controller = new ExampleController();

            // Act - we must cast the return type from IActionResult (which is generic) to a ViewResult (which is specific)
            var result = (ViewResult)controller.Index();

            // Assert - check that the ViewResult that is returned is named "Index"
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}
