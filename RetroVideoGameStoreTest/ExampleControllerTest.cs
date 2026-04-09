using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RetroVideoGameStoreTest
{
    [TestClass]
    public class ExampleControllerTest
    {
        [TestMethod]
        public void IndexReturnsSomething()
        {
            // Arrange
            var controller = new ExampleController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IndexLoadsIndexView()
        {
            // Arrange - create new instance of ExampleController
            var controller = new ExampleController();

            // Act - we must cast the return type from IActionResult (generic) to a ViewResult (specific)
            var result = (ViewResult)controller.Index();

            // Assert - check that the ViewResult that is returned is actually called "Index"
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}
