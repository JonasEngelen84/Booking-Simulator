using Microsoft.Extensions.Logging;
using Moq;
using OBS.Booking.Client.Api;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using OBS_Booking_App.Services.Configuration;
using OBS_Booking_App.Stores;

namespace OBS_Booking_App.Tests
{
    public class BookingServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogInEmployee_WhenBookingTimeIsNow()
        {
            // Arrange
            var employee = new Employee(
                "123",
                "Test",
                DateTime.Now,
                DateTime.Now.AddHours(8),
                DateTime.Now,
                DateTime.Now.AddHours(8))
            {
                LoggedIn = false
            };

            var store = new EmployeeStore(new List<IEmployeesProvider>());
            store.Employees.Add(employee);

            var mockLogger = new Mock<ILogger<BookingService>>();
            var mockBookingApi = new Mock<IBookingApi>();

            var service = new BookingService(store, mockBookingApi.Object, mockLogger.Object);

            // Act
            await service.ExecuteAsync();

            // Assert
            Assert.True(employee.LoggedIn);
            mockBookingApi.Verify(api => api.CreateAsync(It.IsAny<CreateBookingModel>()), Times.Once);
        }
    }
}
