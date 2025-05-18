using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MizeProject.Models;
using MizeProject.Storages;
using MizeProject.Chain;
namespace MizeProject.Tests
{
    [TestClass]
    public class ChainResourceTests
    {
        private Mock<IStorage<ExchangeRateList>> _mockMemoryStorage;
        private Mock<IStorage<ExchangeRateList>> _mockFileSystemStorage;
        private Mock<IStorage<ExchangeRateList>> _mockWebServiceStorage;
        private ChainResource<ExchangeRateList> _chainResource;
        private ExchangeRateList _sampleExchangeRateList;

        [TestInitialize]
        public void Setup()
        {
            // יצירת רשימת שערי חליפין לדוגמה
            _sampleExchangeRateList = new ExchangeRateList
            {
                Rates = new Dictionary<string, decimal>
                {
                    { "USD", 3.5m },
                    { "EUR", 4.2m }
                }
            };

            // הגדרת ה-Mocks לאמצעי האחסון
            _mockMemoryStorage = new Mock<IStorage<ExchangeRateList>>();
            _mockFileSystemStorage = new Mock<IStorage<ExchangeRateList>>();
            _mockWebServiceStorage = new Mock<IStorage<ExchangeRateList>>();

            // הגדרת ההגדרות הבסיסיות של האחסונים
            _mockMemoryStorage.Setup(s => s.IsWrite).Returns(true);
            _mockFileSystemStorage.Setup(s => s.IsWrite).Returns(true);
            _mockWebServiceStorage.Setup(s => s.IsWrite).Returns(false);

            // הגדרת שרשרת המשאבים
            _chainResource = new ChainResource<ExchangeRateList>(new IStorage<ExchangeRateList>[]
            {
                _mockMemoryStorage.Object,
                _mockFileSystemStorage.Object,
                _mockWebServiceStorage.Object
            });
        }
        [TestMethod]
        public void SimpleTest_ShouldPass()
        {
            // בדיקה פשוטה שתמיד עוברת
            Assert.IsTrue(true);
        }


        [TestMethod]
        public async Task GetValue_WhenMemoryHasValidData_ReturnsMemoryData()
        {
            // Arrange
            _mockMemoryStorage.Setup(s => s.IsExpired).Returns(false);

            // תיקון: שימוש ב-Returns במקום ReturnsAsync
            var result = Task.FromResult(new Result<ExchangeRateList> { Value = _sampleExchangeRateList });
            _mockMemoryStorage.Setup(s => s.ReadAsync()).Returns(result);

            // Act
            var actualResult = await _chainResource.GetValue();
            Assert.IsNotNull(actualResult);
            ExchangeRateList actualValue = actualResult.Value!;

            // Assert
            Assert.AreEqual(_sampleExchangeRateList, actualValue);
            _mockMemoryStorage.Verify(s => s.ReadAsync(), Times.Once);
            _mockFileSystemStorage.Verify(s => s.ReadAsync(), Times.Never);
            _mockWebServiceStorage.Verify(s => s.ReadAsync(), Times.Never);
        }
    }


}