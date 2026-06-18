//using Microsoft.EntityFrameworkCore;
//using ServiceLayer.Data;
//using ServiceLayer.Models;
//using ServiceLayer.Services;
//using System.Threading.Tasks;
//using Xunit;

namespace Tests
{
    public class UserServiceTests
    {
        private InformaticTextBookContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<InformaticTextBookContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            return new InformaticTextBookContext(options);
        }

        [Fact]
        public async Task GetUserByLoginAndPasswordAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Создаем тестовые данные
            var role = new Role { RoleId = 1, RoleName = "Преподаватель" };
            var user = new User
            {
                UserId = 1,
                UserLogin = "teacher1",
                UserPassword = "password123",
                Role = role,
                RoleId = 1
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var result = await service.GetUserByLoginAndPasswordAsync("teacher1", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("teacher1", result.UserLogin);
            Assert.Equal("Преподаватель", result.Role.RoleName);
        }

        [Fact]
        public async Task GetUserByLoginAndPasswordAsync_InvalidLogin_ReturnsNull()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new UserService(context);

            // Act
            var result = await service.GetUserByLoginAndPasswordAsync("nonexistent", "password");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var role = new Role { RoleId = 2, RoleName = "Студент" };
            var user = new User
            {
                UserId = 100,
                UserLogin = "student100",
                UserPassword = "pass",
                Role = role,
                RoleId = 2
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var result = await service.GetUserByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("student100", result.UserLogin);
            Assert.Equal(2, result.RoleId);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ReturnsOnlyStudents()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Создаем роли
            var teacherRole = new Role { RoleId = 1, RoleName = "Преподаватель" };
            var studentRole = new Role { RoleId = 2, RoleName = "Студент" };

            // Создаем пользователей
            var users = new[]
            {
                new User { UserId = 1, UserLogin = "teacher1", UserPassword = "pass", RoleId = 1, Role = teacherRole },
                new User { UserId = 2, UserLogin = "student1", UserPassword = "pass", RoleId = 2, Role = studentRole },
                new User { UserId = 3, UserLogin = "student2", UserPassword = "pass", RoleId = 2, Role = studentRole },
                new User { UserId = 4, UserLogin = "teacher2", UserPassword = "pass", RoleId = 1, Role = teacherRole }
            };

            context.Roles.AddRange(teacherRole, studentRole);
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            var service = new UserService(context);

            // Act
            var result = await service.GetAllStudentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, u => Assert.Equal(2, u.RoleId));
            Assert.All(result, u => Assert.Equal("Студент", u.Role.RoleName));
        }
    }
}
