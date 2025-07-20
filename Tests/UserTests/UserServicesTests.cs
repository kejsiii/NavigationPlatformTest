using Application.Interfaces;
using Application.Resources;
using Application.Services;
using AutoMapper;
using Common.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using DTO.DTO.User;
using Moq;
using Presentation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.UserTests
{
    public class UserServicesTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IJWTUtilities> _jwtMock;
        private readonly Mock<IJwtBlacklistServices> _jwtBlacklistServicesMock;
        private readonly UserServices _userServices;


        public UserServicesTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _mapperMock = new Mock<IMapper>();
            _jwtMock = new Mock<IJWTUtilities>();
            _jwtBlacklistServicesMock = new Mock<IJwtBlacklistServices>();

            _userServices = new UserServices(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _mapperMock.Object,
                _jwtMock.Object,
                _jwtBlacklistServicesMock.Object
            );
        }

        #region LogIn

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@example.com", Password = "password123" };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, Password = request.Password, RoleId = Guid.NewGuid() };
            var role = new Role { Id = user.RoleId, Name = "Admin" };
            var expectedToken = "fake-jwt-token";

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, request.Password))
                               .ReturnsAsync(user);
            _roleRepositoryMock.Setup(r => r.GetAsyncById(user.RoleId))
                               .ReturnsAsync(role);
            _jwtMock.Setup(j => j.GenerateToken(user.Id, request.Email, role.Name))
                    .Returns(expectedToken);

            // Act
            var result = await _userServices.LoginAsync(request);

            // Assert
            Assert.Equal(expectedToken, result);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenUserIsNull()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "notfound@example.com", Password = "password" };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, request.Password))
                               .ReturnsAsync((User)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _userServices.LoginAsync(request));
            Assert.Equal(StringResourceMessage.InvalidCredentials, ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenPasswordDoesNotMatch()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@example.com", Password = "wrongpassword" };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, Password = "correctpassword", RoleId = Guid.NewGuid() };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, request.Password))
                               .ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _userServices.LoginAsync(request));
            Assert.Equal(StringResourceMessage.InvalidCredentials, ex.Message);
        }

        #endregion

        #region Register

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnResponse_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "newuser@example.com",
                Username = "newuser",
                Password = "123",
                Role = "User"
            };

            var role = new Role { Id = Guid.NewGuid(), Name = "User" };

            var mappedUser = new User
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                RoleId = role.Id,
                Status = Status.Active.ToString(),
                Id = Guid.NewGuid(),
            };

            _userRepositoryMock.Setup(r => r.FindByEmailOrUsernameAsync(request.Email, request.Username))
                .ReturnsAsync((User)null);

            _roleRepositoryMock.Setup(r => r.GetRoleByName(request.Role))
                .ReturnsAsync(role);

            _mapperMock.Setup(m => m.Map<User>(It.IsAny<RegisterRequestDto>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns(mappedUser);

            
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(mappedUser.Id);

            // Act
            var result = await _userServices.RegisterUserAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(mappedUser.Id, result.Id);
            Assert.Equal(role.Id, mappedUser.RoleId);
            Assert.Equal(Status.Active.ToString(), mappedUser.Status);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowConflictException_WhenUserAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "existing@example.com",
                Username = "existinguser",
                Role = "User"
            };

            _userRepositoryMock.Setup(r => r.FindByEmailOrUsernameAsync(request.Email, request.Username))
                .ReturnsAsync(new User());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _userServices.RegisterUserAsync(request));
            Assert.Equal(StringResourceMessage.UserAlreadyExists, ex.Message);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "new@example.com",
                Username = "newuser",
                Role = "InvalidRole"
            };

            _userRepositoryMock.Setup(r => r.FindByEmailOrUsernameAsync(request.Email, request.Username))
                .ReturnsAsync((User)null);

            _roleRepositoryMock.Setup(r => r.GetRoleByName(request.Role))
                .ReturnsAsync((Role)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _userServices.RegisterUserAsync(request));
            Assert.Equal(StringResourceMessage.RoleNotFound, ex.Message);
        }
        #endregion

        #region Logout
        [Fact]
        public async Task LogoutAsync_ShouldCallAddToBlacklistAsync_WithGivenToken()
        {
            // Arrange
            var token = "test-jwt-token";

            _jwtBlacklistServicesMock
                .Setup(s => s.AddToBlacklistAsync(token))
                .Returns(Task.CompletedTask)
                .Verifiable(); // Ensure it's called

            // Act
            await _userServices.LogoutAsync(token);

            // Assert
            _jwtBlacklistServicesMock.Verify(s => s.AddToBlacklistAsync(token), Times.Once);
        }
        #endregion
    }

}

