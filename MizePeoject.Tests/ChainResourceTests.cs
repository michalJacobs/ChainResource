using MizeProject.Chain;
using MizeProject.Models;
using MizeProject.Storages;
using MizeProject.Tests;
using Moq;

namespace MizeProject.Tests
{

    [TestClass]
    public class ChainResourceTests
    {
        private Mock<IStorage<ExchangeRateList>>? mockMemoryStorage;
        private Mock<IStorage<ExchangeRateList>>? mockFileStorage;
        private Mock<IStorage<ExchangeRateList>>? mockWebStorage;
        private List<IStorage<ExchangeRateList>>? storages;
        private ChainResource<ExchangeRateList>? chainResource;

        [TestInitialize]
        public void TestInitialize()
        {
            mockMemoryStorage = MockStorage.CreateMockStorage<ExchangeRateList>(
                StorageLevel.Memory,
                isRead: true,
                isWrite: true,
                isExpired: false,
                hasValue: false,
                value: null);

            mockFileStorage = MockStorage.CreateMockStorage<ExchangeRateList>(
                StorageLevel.File,
                isRead: true,
                isWrite: true,
                isExpired: false,
                hasValue: false,
                value: null);

            mockWebStorage = MockStorage.CreateMockStorage<ExchangeRateList>(
                StorageLevel.Web,
                isRead: true,
                isWrite: false,
                isExpired: false,
                hasValue: false,
                value: null);

            storages = new List<IStorage<ExchangeRateList>>
            {
                mockMemoryStorage.Object,
                mockFileStorage.Object,
                mockWebStorage.Object
            };

            chainResource = new ChainResource<ExchangeRateList>(storages);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mockMemoryStorage = null;
            mockFileStorage = null;
            mockWebStorage = null;
            storages = null;
            chainResource = null;
        }

        private ExchangeRateList CreateExchangeRateList(string baseCurrency, Dictionary<string, decimal> rates, long timestamp)
        {
            return new ExchangeRateList
            {
                Base = baseCurrency,
                Rates = rates,
                Timestamp = timestamp
            };
        }

        private void SetupMockStorage(
            Mock<IStorage<ExchangeRateList>> mockStorage,
            bool hasValue,
            ExchangeRateList value,
            bool isExpired)
        {
            mockStorage.Setup(s => s.IsExpired).Returns(isExpired);
            mockStorage
                .Setup(s => s.ReadAsync())
                .ReturnsAsync(new Result<ExchangeRateList>(hasValue, value));
        }

        [TestMethod]
        public async Task GetValue_WhenNoStorageHasValue_ReturnsEmptyResult()
        {

            var result = await chainResource!.GetValue();

            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task GetValue_WhenWebStorageHasValue_ReturnsValueAndUpdatesLowerStorages()
        {
            var rates = new Dictionary<string, decimal> { { "ILS", 3.5m }, { "EUR", 0.85m } };
            var exchangeRateList = CreateExchangeRateList("USD", rates, 1621324800);

            SetupMockStorage(mockWebStorage!, true, exchangeRateList, false);

            var result = await chainResource!.GetValue();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("USD", result.Value!.Base);
            Assert.AreEqual(2, result.Value.Rates.Count);
            Assert.AreEqual(3.5m, result.Value.Rates["ILS"]);


            mockMemoryStorage!.Verify(s => s.WriteAsync(exchangeRateList), Times.Once);
            mockFileStorage!.Verify(s => s.WriteAsync(exchangeRateList), Times.Once);
        }

        [TestMethod]
        public async Task GetValue_WhenMemoryStorageIsExpired_MovesToFileStorage()
        {
            var expiredRates = new Dictionary<string, decimal> { { "ILS", 3.5m } };
            var expiredExchangeRateList = CreateExchangeRateList("USD", expiredRates, 1621324800);

            var validRates = new Dictionary<string, decimal> { { "ILS", 3.6m } };
            var validExchangeRateList = CreateExchangeRateList("USD", validRates, 1621324800);

            SetupMockStorage(mockMemoryStorage, true, expiredExchangeRateList, true);
            SetupMockStorage(mockFileStorage, true, validExchangeRateList, false);

            var result = await chainResource!.GetValue();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(3.6m, result.Value.Rates["ILS"]);

            mockMemoryStorage.Verify(s => s.WriteAsync(It.Is<ExchangeRateList>(r => r.Rates["ILS"] == 3.6m)), Times.Once);
        }

        [TestMethod]
        public async Task GetValue_WhenFileStorageHasValue_ReturnsValueAndUpdatesMemoryStorage()
        {
            var fileRates = new Dictionary<string, decimal> { { "ILS", 3.6m } };
            var fileExchangeRateList = CreateExchangeRateList("USD", fileRates, 1621324800);

            SetupMockStorage(mockFileStorage!, true, fileExchangeRateList, false);

            var result = await chainResource!.GetValue();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(3.6m, result.Value!.Rates["ILS"]);

            mockMemoryStorage!.Verify(s => s.WriteAsync(fileExchangeRateList), Times.Once);

            mockWebStorage!.Verify(s => s.ReadAsync(), Times.Never);
        }

        [TestMethod]
        public async Task GetValue_WithDifferentStorageOrder_RespectsPriority()
        {
            mockMemoryStorage = MockStorage.CreateMockStorage<ExchangeRateList>(
                StorageLevel.Memory,
                isRead: true,
                isWrite: true,
                isExpired: false,
                hasValue: true,
                value: CreateExchangeRateList("USD", new Dictionary<string, decimal> { { "ILS", 3.5m } }, 1621324800));

            mockFileStorage = MockStorage.CreateMockStorage<ExchangeRateList>(
                StorageLevel.File,
                isRead: true,
                isWrite: true,
                isExpired: false,
                hasValue: true,
                value: CreateExchangeRateList("USD", new Dictionary<string, decimal> { { "ILS", 3.6m } }, 1621324800));

            mockWebStorage = MockStorage.CreateMockStorage<ExchangeRateList>(
                StorageLevel.Web,
                isRead: true,
                isWrite: false,
                isExpired: false,
                hasValue: true,
                value: CreateExchangeRateList("USD", new Dictionary<string, decimal> { { "ILS", 3.7m } }, 1621324800));

            var unsortedStorages = new List<IStorage<ExchangeRateList>>
    {
        mockWebStorage.Object,
        mockMemoryStorage.Object,
        mockFileStorage.Object
    };

            var chainResource = new ChainResource<ExchangeRateList>(unsortedStorages);

            var result = await chainResource.GetValue();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(3.5m, result.Value!.Rates["ILS"]);
        }
    }


}
