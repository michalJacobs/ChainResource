using MizeProject.Models;
using MizeProject.Storages;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MizeProject.Tests
{
    public class MockStorage
    {
        public static Mock<IStorage<T>> CreateMockStorage<T>(
            StorageLevel level,
            bool isRead,
            bool isWrite,
            bool isExpired,
            bool hasValue,
            T? value)
        {
            var mockStorage = new Mock<IStorage<T>>();

            mockStorage.Setup(s => s.Level).Returns(level);
            mockStorage.Setup(s => s.IsRead).Returns(isRead);
            mockStorage.Setup(s => s.IsWrite).Returns(isWrite);
            mockStorage.Setup(s => s.IsExpired).Returns(isExpired);
            mockStorage.Setup(s => s.StorageValue).Returns(value);

            mockStorage
                .Setup(s => s.ReadAsync())
                .ReturnsAsync(new Result<T>(hasValue, value));

            mockStorage
                .Setup(s => s.WriteAsync(It.IsAny<T>()))
                .Returns(Task.CompletedTask);

            return mockStorage;
        }
    }
}
