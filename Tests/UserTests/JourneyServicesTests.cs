using Application.Resources;
using Application.Services;
using AutoMapper;
using Common.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using DTO.DTO.Journey;
using MockQueryable;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.UserTests
{
    public class JourneyServicesTests
    {
        private readonly Mock<IJourneyRepository> _journeyRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IJourneyPublicLinkRepository> _journeyPublicLinkRepositoryMock = new();
        private readonly Mock<IJourneyShareRepository> _journeyShareRepositoryMock = new();
        private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
        private readonly JourneyServices _journeyServices;
        public JourneyServicesTests()
        {
            _journeyServices = new JourneyServices(
                _journeyRepositoryMock.Object,
                _mapperMock.Object,
                _userRepositoryMock.Object,
                _journeyPublicLinkRepositoryMock.Object,
                _journeyShareRepositoryMock.Object,
                _auditLogRepositoryMock.Object
            );
        }

        #region AddJourney
        [Fact]
        public async Task AddJourneyAsync_ShouldReturnOid_WhenJourneyIsSuccessfullyAdded()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;
            var expectedOid = Guid.NewGuid();

            var request = new AddJourneyRequestDto
            {
                UserId = userId,
                StartTime = startTime,
            };

            _userRepositoryMock
                .Setup(r => r.GetAsyncById(userId))
                .ReturnsAsync(new User());

            _journeyRepositoryMock
                .Setup(r => r.GetJourneyByUserIdAndStartTime(userId, startTime))
                .ReturnsAsync((Journey)null);

            _mapperMock
                .Setup(m => m.Map<Journey>(It.IsAny<AddJourneyRequestDto>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns((AddJourneyRequestDto src, Action<IMappingOperationOptions> opt) =>
                {
                    var journey = new Journey
                    {
                        UserId = src.UserId,
                        StartTime = src.StartTime,
                        IsDailyGoalAchieved = false,
                        // don't set JourneyId here, because AddAsync is expected to assign it
                    };
                    return journey;
                });

            // Relaxed matcher: any Journey will return expectedOid
            _journeyRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Journey>()))
                .ReturnsAsync(expectedOid);

            // Act
            var result = await _journeyServices.AddJourneyAsync(request);

            // Assert
            Assert.Equal(expectedOid, result);
        }

        [Fact]
        public async Task AddJourneyAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var request = new AddJourneyRequestDto
            {
                UserId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(r => r.GetAsyncById(request.UserId))
                .ReturnsAsync((User)null); // User not found

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _journeyServices.AddJourneyAsync(request));
            Assert.Equal(StringResourceMessage.UserNotFound, ex.Message);
        }

        [Fact]
        public async Task AddJourneyAsync_ShouldThrowConflictException_WhenJourneyAlreadyExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;

            var request = new AddJourneyRequestDto
            {
                UserId = userId,
                StartTime = startTime
            };

            _userRepositoryMock
                .Setup(r => r.GetAsyncById(userId))
                .ReturnsAsync(new User()); // User exists

            _journeyRepositoryMock
                .Setup(r => r.GetJourneyByUserIdAndStartTime(userId, startTime))
                .ReturnsAsync(new Journey()); // Journey already exists

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _journeyServices.AddJourneyAsync(request));
            Assert.Equal(StringResourceMessage.JourneyAlreadyExists, ex.Message);
        }
        #endregion

        #region DeleteJourney
        [Fact]
        public async Task DeleteJourneyAsync_ShouldDeleteAll_WhenJourneyExistsWithLinksAndShares()
        {
            // Arrange
            var journeyId = Guid.NewGuid();

            var journeyPublicLinks = new List<JourneyPublicLink> { new JourneyPublicLink(), new JourneyPublicLink() };
            var journeyShares = new List<JourneyShare> { new JourneyShare(), new JourneyShare() };

            var journey = new Journey
            {
                JourneyId = journeyId,
                JourneyPublicLinks = journeyPublicLinks,
                JourneyShares = journeyShares
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(journey);

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.DeleteAll(It.Is<List<JourneyPublicLink>>(l => l.Count == journeyPublicLinks.Count)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _journeyShareRepositoryMock
                .Setup(r => r.DeleteAll(It.Is<List<JourneyShare>>(l => l.Count == journeyShares.Count)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _journeyRepositoryMock
                .Setup(r => r.RemoveAsync(journey))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _journeyServices.DeleteJourneyAsync(journeyId);

            // Assert
            _journeyPublicLinkRepositoryMock.Verify();
            _journeyShareRepositoryMock.Verify();
            _journeyRepositoryMock.Verify();
        }

        [Fact]
        public async Task DeleteJourneyAsync_ShouldDeleteOnlyJourney_WhenNoLinksOrShares()
        {
            // Arrange
            var journeyId = Guid.NewGuid();

            var journey = new Journey
            {
                JourneyId = journeyId,
                JourneyPublicLinks = null,
                JourneyShares = null
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(journey);

            _journeyRepositoryMock
                .Setup(r => r.RemoveAsync(journey))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _journeyServices.DeleteJourneyAsync(journeyId);

            // Assert
            _journeyPublicLinkRepositoryMock.Verify(r => r.DeleteAll(It.IsAny<List<JourneyPublicLink>>()), Times.Never);
            _journeyShareRepositoryMock.Verify(r => r.DeleteAll(It.IsAny<List<JourneyShare>>()), Times.Never);
            _journeyRepositoryMock.Verify();
        }

        [Fact]
        public async Task DeleteJourneyAsync_ShouldThrowNotFoundException_WhenJourneyDoesNotExist()
        {
            // Arrange
            var journeyId = Guid.NewGuid();

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync((Journey)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _journeyServices.DeleteJourneyAsync(journeyId));
            Assert.Equal(StringResourceMessage.JourneyNotFound, ex.Message);
        }
    
    #endregion

        #region GeneratePublicLink
        [Fact]
        public async Task GeneratePublicLinkAsync_ShouldThrowNotFoundException_WhenJourneyDoesNotExist()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync((Journey)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _journeyServices.GeneratePublicLinkAsync(journeyId, userId));

            Assert.Equal(StringResourceMessage.JourneyNotFound, ex.Message);
        }

        [Fact]
        public async Task GeneratePublicLinkAsync_ShouldReturnExistingLink_WhenNonRevokedLinkExists()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingLink = new JourneyPublicLink
            {
                JourneyId = journeyId,
                Token = "existing-token",
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(new Journey { JourneyId = journeyId });

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.GetJourneyPublicLinkRevokedByJourneyId(journeyId))
                .ReturnsAsync(existingLink);

            // Act
            var result = await _journeyServices.GeneratePublicLinkAsync(journeyId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"/api/journeys/public/{existingLink.Token}", result.Url);
            Assert.Equal(existingLink.Token, result.Token);

            // Ensure AddAsync is never called because existing link is returned
            _journeyPublicLinkRepositoryMock.Verify(r => r.AddAsync(It.IsAny<JourneyPublicLink>()), Times.Never);
        }

        [Fact]
        public async Task GeneratePublicLinkAsync_ShouldCreateNewLink_WhenNoExistingLink()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(new Journey { JourneyId = journeyId });

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.GetJourneyPublicLinkRevokedByJourneyId(journeyId))
                .ReturnsAsync((JourneyPublicLink)null);

            JourneyPublicLink addedLink = null;
            _journeyPublicLinkRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<JourneyPublicLink>()))
                .Callback<JourneyPublicLink>(link => addedLink = link)
                .Returns(Task.FromResult(journeyId));

            // Act
            var result = await _journeyServices.GeneratePublicLinkAsync(journeyId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.StartsWith("/api/journeys/public/", result.Url);
            Assert.NotNull(addedLink);
            Assert.Equal(journeyId, addedLink.JourneyId);
            Assert.False(addedLink.IsRevoked);
            Assert.Equal(result.Token, addedLink.Token);
            Assert.InRange((DateTime.UtcNow - addedLink.CreatedAt).TotalSeconds, 0, 5);

            _journeyPublicLinkRepositoryMock.Verify(r => r.AddAsync(It.IsAny<JourneyPublicLink>()), Times.Once);
        }
        #endregion

        #region GetAllJourniesForUsers
        [Fact]
        public async Task GetAllJourneysForUserAsync_ShouldReturnMappedJourneys_WhenUserExistsAndHasJourneys()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeyList = new List<Journey>
    {
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId }
    };

            var journeyDtos = new List<JourneyDto>
    {
        new JourneyDto { JourneyId = journeyList[0].JourneyId },
        new JourneyDto { JourneyId = journeyList[1].JourneyId }
    };

            _userRepositoryMock.Setup(r => r.GetAsyncById(userId))
                .ReturnsAsync(new User { Id = userId });

            _journeyRepositoryMock.Setup(r => r.GetJourneysByUserId(userId))
                .Returns(journeyList.AsQueryable());

            _mapperMock.Setup(m => m.Map<List<JourneyDto>>(journeyList))
                .Returns(journeyDtos);

            // Act
            var result = await _journeyServices.GetAllJourneysForUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(journeyDtos[0].JourneyId, result[0].JourneyId);
        }

        [Fact]
        public async Task GetAllJourneysForUserAsync_ShouldReturnEmptyList_WhenUserExistsButNoJourneys()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.GetAsyncById(userId))
                .ReturnsAsync(new User { Id = userId });

            _journeyRepositoryMock.Setup(r => r.GetJourneysByUserId(userId))
                .Returns(new List<Journey>().AsQueryable());

            // Act
            var result = await _journeyServices.GetAllJourneysForUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllJourneysForUserAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.GetAsyncById(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _journeyServices.GetAllJourneysForUserAsync(userId));
        }
        #endregion

        #region GetJourneyByIdAsync
        [Fact]
        public async Task GetJourneyByIdAsync_ShouldReturnMappedJourney_WhenJourneyExists()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var journeyEntity = new Journey { JourneyId = journeyId };
            var expectedDto = new JourneyDto { JourneyId = journeyId };

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(journeyEntity);

            _mapperMock
                .Setup(m => m.Map<JourneyDto>(journeyEntity))
                .Returns(expectedDto);

            // Act
            var result = await _journeyServices.GetJourneyByIdAsync(journeyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.JourneyId, result.JourneyId);
        }

        [Fact]
        public async Task GetJourneyByIdAsync_ShouldThrowNotFoundException_WhenJourneyDoesNotExist()
        {
            // Arrange
            var journeyId = Guid.NewGuid();

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync((Journey)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _journeyServices.GetJourneyByIdAsync(journeyId));
        }
        #endregion

        #region GetJourniesByFilter

        [Fact]
        public async Task GetJourniesByFilter_ShouldReturnFilteredAndPagedResult()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeyList = new List<Journey>
    {
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-2), ArrivalTime = DateTime.UtcNow, TransportationType = "Car" },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-1), ArrivalTime = DateTime.UtcNow.AddHours(1), TransportationType = "Car" },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-3), ArrivalTime = DateTime.UtcNow.AddHours(2), TransportationType = "Bike" }
    };

            var filter = new JourneyFilterRequestDto
            {
                UserId = userId,
                TransportType = "Car",
                StartDateFrom = DateTime.UtcNow.AddHours(-4),
                ArrivalDateTo = DateTime.UtcNow.AddHours(2),
                Page = 1,
                PageSize = 2,
                OrderBy = "StartTime",
                Direction = "asc"
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeyList.AsQueryable());

            _mapperMock
                .Setup(m => m.Map<List<JourneyDto>>(It.IsAny<List<Journey>>()))
                .Returns((List<Journey> source) =>
                    source.Select(j => new JourneyDto { JourneyId = j.JourneyId }).ToList());

            // Act
            var result = await _journeyServices.GetJourniesByFilter(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);     // 2 Cars
            Assert.Equal(2, result.TotalCount);      // Total matching = 2
        }

        [Fact]
        public async Task GetJourniesByFilter_ShouldReturnAllPaged_WhenNoFiltersApplied()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeyList = new List<Journey>
    {
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-2), ArrivalTime = DateTime.UtcNow, TransportationType = "Car" },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-1), ArrivalTime = DateTime.UtcNow.AddHours(1), TransportationType = "Bike" },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-3), ArrivalTime = DateTime.UtcNow.AddHours(2), TransportationType = "Train" }
    };

            var filter = new JourneyFilterRequestDto
            {
                Page = 1,
                PageSize = 2
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeyList.AsQueryable());

            _mapperMock
                .Setup(m => m.Map<List<JourneyDto>>(It.IsAny<List<Journey>>()))
                .Returns((List<Journey> source) =>
                    source.Select(j => new JourneyDto { JourneyId = j.JourneyId }).ToList());

            // Act
            var result = await _journeyServices.GetJourniesByFilter(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);          // PageSize = 2
            Assert.Equal(3, result.TotalCount);           // Total count = 3 journeys
        }

        [Fact]
        public async Task GetJourniesByFilter_ShouldFallbackToStartTime_WhenOrderByInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeyList = new List<Journey>
    {
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-1), ArrivalTime = DateTime.UtcNow, TransportationType = "Car" },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-2), ArrivalTime = DateTime.UtcNow, TransportationType = "Car" }
    };

            var filter = new JourneyFilterRequestDto
            {
                UserId = userId,
                TransportType =  "Car" ,
                Page = 1,
                PageSize = 10,
                OrderBy = "InvalidPropertyName",
                Direction = "asc"
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeyList.AsQueryable());

            _mapperMock
                .Setup(m => m.Map<List<JourneyDto>>(It.IsAny<List<Journey>>()))
                .Returns((List<Journey> source) =>
                    source.Select(j => new JourneyDto { JourneyId = j.JourneyId }).ToList());

            // Act
            var result = await _journeyServices.GetJourniesByFilter(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);

            // Check the first item has the earlier StartTime (because ordering fallback)
            Assert.True(result.Items[0].JourneyId == journeyList[1].JourneyId);
        }

        [Fact]
        public async Task GetJourniesByFilter_ShouldOrderDescending_WhenDirectionIsDesc()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeyList = new List<Journey>
    {
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-3), ArrivalTime = DateTime.UtcNow, TransportationType = "Car" },
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-1), ArrivalTime = DateTime.UtcNow, TransportationType = "Car" }
    };

            var filter = new JourneyFilterRequestDto
            {
                UserId = userId,
                TransportType = "Car" ,
                Page = 1,
                PageSize = 10,
                OrderBy = "StartTime",
                Direction = "desc"
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeyList.AsQueryable());

            _mapperMock
                .Setup(m => m.Map<List<JourneyDto>>(It.IsAny<List<Journey>>()))
                .Returns((List<Journey> source) =>
                    source.Select(j => new JourneyDto { JourneyId = j.JourneyId }).ToList());

            // Act
            var result = await _journeyServices.GetJourniesByFilter(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);

            // First item should have later StartTime (descending)
            Assert.Equal(journeyList[1].JourneyId, result.Items[0].JourneyId);
        }
        [Fact]
        public async Task GetJourniesByFilter_ShouldReturnEmpty_WhenNoJourneysMatchFilter()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeyList = new List<Journey>
    {
        new Journey { JourneyId = Guid.NewGuid(), UserId = userId, StartTime = DateTime.UtcNow.AddHours(-3), ArrivalTime = DateTime.UtcNow, TransportationType = "Bike" },
    };

            var filter = new JourneyFilterRequestDto
            {
                UserId = userId,
                TransportType = "Car" , // no "Car" journeys in list
                Page = 1,
                PageSize = 10,
                OrderBy = "StartTime",
                Direction = "asc"
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeyList.AsQueryable());

            _mapperMock
                .Setup(m => m.Map<List<JourneyDto>>(It.IsAny<List<Journey>>()))
                .Returns(new List<JourneyDto>());

            // Act
            var result = await _journeyServices.GetJourniesByFilter(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        #endregion

        #region GetMonthlyDistances
        [Fact]
        public async Task GetMonthlyDistancesAsync_ShouldReturnPagedGroupedResults()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeys = new List<Journey>
    {
        new Journey { UserId = userId, StartTime = new DateTime(2025, 7, 10), RouteDistanceKm = 5 },
        new Journey { UserId = userId, StartTime = new DateTime(2025, 7, 15), RouteDistanceKm = 10 },
        new Journey { UserId = userId, StartTime = new DateTime(2025, 6, 1), RouteDistanceKm = 20 },
        new Journey { UserId = Guid.NewGuid(), StartTime = new DateTime(2025, 7, 5), RouteDistanceKm = 7 }
    };

            var request = new MonthlyRouteDistanceDto
            {
                UserId = userId,
                Year = 2025,
                Page = 1,
                PageSize = 10,
                OrderBy = "totaldistancekm" // order descending by default
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeys.AsQueryable());  // just IQueryable is fine now

            // Act
            var result = await _journeyServices.GetMonthlyDistancesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // July and June grouped for userId

            var julyGroup = result.FirstOrDefault(g => g.Month == 7 && g.Year == 2025);
            Assert.NotNull(julyGroup);
            Assert.Equal(userId, julyGroup.UserId);
            Assert.Equal(15, julyGroup.TotalDistanceKm);

            var juneGroup = result.FirstOrDefault(g => g.Month == 6 && g.Year == 2025);
            Assert.NotNull(juneGroup);
            Assert.Equal(userId, juneGroup.UserId);
            Assert.Equal(20, juneGroup.TotalDistanceKm);

            Assert.True(result[0].TotalDistanceKm >= result[1].TotalDistanceKm);
        }

        [Fact]
        public async Task GetMonthlyDistancesAsync_ShouldReturnEmptyList_WhenNoMatchingJourneys()
        {
            // Arrange
            var journeys = new List<Journey>
    {
        new Journey { UserId = Guid.NewGuid(), StartTime = new DateTime(2025, 5, 1), RouteDistanceKm = 10 }
    };

            var request = new MonthlyRouteDistanceDto
            {
                UserId = Guid.NewGuid(),  // Different userId, no match
                Year = 2025,
                Month = 7
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeys.AsQueryable());

            // Act
            var result = await _journeyServices.GetMonthlyDistancesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMonthlyDistancesAsync_ShouldOrderByTotalDistanceKmDescending_ByDefault()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeys = new List<Journey>
    {
        new Journey { UserId = userId, StartTime = new DateTime(2025, 7, 10), RouteDistanceKm = 5 },
        new Journey { UserId = userId, StartTime = new DateTime(2025, 7, 15), RouteDistanceKm = 10 },
        new Journey { UserId = userId, StartTime = new DateTime(2025, 7, 1), RouteDistanceKm = 20 }
    };

            var request = new MonthlyRouteDistanceDto
            {
                UserId = userId,
                Year = 2025,
                Month = 7,
                Page = 1,
                PageSize = 10,
                OrderBy = null // No order specified
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeys.AsQueryable());

            // Act
            var result = await _journeyServices.GetMonthlyDistancesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count); // All same month/year => one group

            // Should be ordered descending by TotalDistanceKm
            Assert.Equal(35, result[0].TotalDistanceKm); // 5 + 10 + 20
        }

        [Fact]
        public async Task GetMonthlyDistancesAsync_ShouldOrderByUserId_WhenOrderByUserId()
        {
            // Arrange
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            var journeys = new List<Journey>
    {
        new Journey { UserId = user2, StartTime = new DateTime(2025, 7, 10), RouteDistanceKm = 15 },
        new Journey { UserId = user1, StartTime = new DateTime(2025, 7, 10), RouteDistanceKm = 5 }
    };

            var request = new MonthlyRouteDistanceDto
            {
                Year = 2025,
                Month = 7,
                Page = 1,
                PageSize = 10,
                OrderBy = "UserId"
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeys.AsQueryable());

            // Act
            var result = await _journeyServices.GetMonthlyDistancesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // UserId ordering ascending => user1 first
            Assert.True(result[0].UserId.CompareTo(result[1].UserId) < 0);
        }

        [Fact]
        public async Task GetMonthlyDistancesAsync_ShouldReturnCorrectPage()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var journeys = new List<Journey>();

            // Create 5 groups with different months
            for (int month = 1; month <= 5; month++)
            {
                journeys.Add(new Journey
                {
                    UserId = userId,
                    StartTime = new DateTime(2025, month, 1),
                    RouteDistanceKm = month * 10
                });
            }

            var request = new MonthlyRouteDistanceDto
            {
                UserId = userId,
                Year = 2025,
                Page = 2,
                PageSize = 2,
                OrderBy = "UserId"
            };

            _journeyRepositoryMock
                .Setup(r => r.GetAllJournies())
                .Returns(journeys.AsQueryable());

            // Act
            var result = await _journeyServices.GetMonthlyDistancesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // page size = 2

            // The first page contains months 1,2; second page contains months 3,4
            Assert.Contains(result, r => r.Month == 3);
            Assert.Contains(result, r => r.Month == 4);
        }

        #endregion

        #region GetPublicJourneyByToken

        [Fact]
        public async Task GetPublicJourneyByTokenAsync_ShouldReturnMappedDto_WhenLinkIsValid()
        {
            // Arrange
            var token = "valid-token";
            var journeyId = Guid.NewGuid();

            var link = new JourneyPublicLink
            {
                JourneyId = journeyId,
                Token = token,
                IsRevoked = false
            };

            var journey = new Journey { JourneyId = journeyId };

            var mappedDto = new JourneyPublicLinkDto { Token = token };

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.GetPublicLinkByToken(token))
                .ReturnsAsync(link);

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(journey);

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<JourneyPublicLink>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<JourneyPublicLinkDto>(link))
                .Returns(mappedDto);

            // Act
            var result = await _journeyServices.GetPublicJourneyByTokenAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(token, result.Token);
            Assert.True(link.IsRevoked);
            _journeyPublicLinkRepositoryMock.Verify(r => r.UpdateAsync(link), Times.Once);
        }

        [Fact]
        public async Task GetPublicJourneyByTokenAsync_ShouldThrowNotFoundException_WhenLinkNotFound()
        {
            // Arrange
            var token = "invalid-token";

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.GetPublicLinkByToken(token))
                .ReturnsAsync((JourneyPublicLink)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _journeyServices.GetPublicJourneyByTokenAsync(token));
            Assert.Equal(StringResourceMessage.PublicLinkNotFound, ex.Message);
        }

        [Fact]
        public async Task GetPublicJourneyByTokenAsync_ShouldThrowNotFoundException_WhenJourneyNotFound()
        {
            // Arrange
            var token = "valid-token";
            var journeyId = Guid.NewGuid();

            var link = new JourneyPublicLink
            {
                JourneyId = journeyId,
                Token = token,
                IsRevoked = false
            };

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.GetPublicLinkByToken(token))
                .ReturnsAsync(link);

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync((Journey)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _journeyServices.GetPublicJourneyByTokenAsync(token));
            Assert.Equal(StringResourceMessage.JourneyNotFound, ex.Message);
        }

        [Fact]
        public async Task GetPublicJourneyByTokenAsync_ShouldThrowGoneException_WhenLinkIsRevoked()
        {
            // Arrange
            var token = "revoked-token";
            var journeyId = Guid.NewGuid();

            var link = new JourneyPublicLink
            {
                JourneyId = journeyId,
                Token = token,
                IsRevoked = true
            };

            _journeyPublicLinkRepositoryMock
                .Setup(r => r.GetPublicLinkByToken(token))
                .ReturnsAsync(link);

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(new Journey { JourneyId = journeyId });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GoneException>(() => _journeyServices.GetPublicJourneyByTokenAsync(token));
            Assert.Equal(StringResourceMessage.PublicLinkRevoked, ex.Message);
        }

        #endregion

        #region  RevokePublicLink
        [Fact]
        public async Task RevokePublicLinkAsync_ShouldRevokeLinkAndAddAuditLog_WhenValid()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var link = new JourneyPublicLink { Id = Guid.NewGuid(), JourneyId = journeyId, IsRevoked = false };

            _journeyRepositoryMock.Setup(r => r.GetAsyncById(journeyId)).ReturnsAsync(new Journey { JourneyId = journeyId });
            _journeyPublicLinkRepositoryMock.Setup(r => r.GetJourneyPublicLinkRevokedByJourneyId(journeyId)).ReturnsAsync(link);
            _journeyPublicLinkRepositoryMock.Setup(r => r.UpdateAsync(link)).Returns(Task.CompletedTask);
            _auditLogRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>())).Returns(Task.FromResult(journeyId));

            // Act
            await _journeyServices.RevokePublicLinkAsync(journeyId, userId);

            // Assert
            Assert.True(link.IsRevoked);
            Assert.NotNull(link.RevokedAt);
            _journeyPublicLinkRepositoryMock.Verify(r => r.UpdateAsync(link), Times.Once);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.Is<AuditLog>(a =>
                a.UserId == userId &&
                a.TargetId == link.Id &&
                a.ActionType == "RevokePublicLink" &&
                a.Description.Contains(journeyId.ToString())
            )), Times.Once);
        }

        [Fact]
        public async Task RevokePublicLinkAsync_ShouldThrowNotFoundException_WhenJourneyNotFound()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _journeyRepositoryMock.Setup(r => r.GetAsyncById(journeyId)).ReturnsAsync((Journey)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _journeyServices.RevokePublicLinkAsync(journeyId, userId));
            Assert.Equal(StringResourceMessage.JourneyNotFound, ex.Message);
        }

        [Fact]
        public async Task RevokePublicLinkAsync_ShouldThrowNotFoundException_WhenLinkNotFound()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _journeyRepositoryMock.Setup(r => r.GetAsyncById(journeyId)).ReturnsAsync(new Journey());
            _journeyPublicLinkRepositoryMock.Setup(r => r.GetJourneyPublicLinkRevokedByJourneyId(journeyId)).ReturnsAsync((JourneyPublicLink)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _journeyServices.RevokePublicLinkAsync(journeyId, userId));
            Assert.Equal(StringResourceMessage.PublicLinkNotFound, ex.Message);
        }

        [Fact]
        public async Task RevokePublicLinkAsync_ShouldThrowGoneException_WhenLinkAlreadyRevoked()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var link = new JourneyPublicLink { IsRevoked = true };

            _journeyRepositoryMock.Setup(r => r.GetAsyncById(journeyId)).ReturnsAsync(new Journey());
            _journeyPublicLinkRepositoryMock.Setup(r => r.GetJourneyPublicLinkRevokedByJourneyId(journeyId)).ReturnsAsync(link);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GoneException>(() => _journeyServices.RevokePublicLinkAsync(journeyId, userId));
            Assert.Equal(StringResourceMessage.PublicLinkRevoked, ex.Message);
        }
        #endregion

        #region ShareJourneyAsync

        [Fact]
        public async Task ShareJourneyAsync_ShouldShareWithUsers_WhenValidRequest()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new JourneyShareRequestDto
            {
                UserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var existingShares = new List<JourneyShare>(); // no existing shares to cause skips

            var addedShares = new List<JourneyShare>();
            var addedAuditLogs = new List<AuditLog>();

            // Journey exists
            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(new Journey { JourneyId = journeyId });

            // Existing shares
            _journeyShareRepositoryMock
                .Setup(r => r.GetAll())
                .ReturnsAsync(existingShares);

            // AddAsync for journey shares returns new Guid
            _journeyShareRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<JourneyShare>()))
                .Callback<JourneyShare>(share => addedShares.Add(share))
                .ReturnsAsync(() => Guid.NewGuid());

            // AddAsync for audit logs returns new Guid
            _auditLogRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(log => addedAuditLogs.Add(log))
                .ReturnsAsync(() => Guid.NewGuid());

            // Act
            var result = await _journeyServices.ShareJourneyAsync(journeyId, userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.UserIds.Count, result.CreatedShareIds.Count);
            Assert.Equal(request.UserIds.Count, result.SharedWithUserIds.Count);

            // Verify shares were added with correct JourneyId, SharedByUserId and RecievingUserId
            foreach (var share in addedShares)
            {
                Assert.Equal(journeyId, share.JourneyId);
                Assert.Equal(userId, share.SharedByUserId);
                Assert.Contains(share.RecievingUserId, request.UserIds);
                Assert.False(share.IsRevoked);
            }

            // Verify audit logs were added for each share
            Assert.Equal(request.UserIds.Count, addedAuditLogs.Count);
            foreach (var log in addedAuditLogs)
            {
                Assert.Equal(userId, log.UserId);
                Assert.Equal("ShareJourney", log.ActionType);
                Assert.NotNull(log.Description);
                Assert.True(log.Timestamp <= DateTime.UtcNow);
            }

            // Verify repository methods were called expected number of times
            _journeyRepositoryMock.Verify(r => r.GetAsyncById(journeyId), Times.Once);
            _journeyShareRepositoryMock.Verify(r => r.GetAll(), Times.Once);
            _journeyShareRepositoryMock.Verify(r => r.AddAsync(It.IsAny<JourneyShare>()), Times.Exactly(request.UserIds.Count));
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Exactly(request.UserIds.Count));
        }
        [Fact]
        public async Task ShareJourneyAsync_ShouldThrowNotFoundException_WhenJourneyDoesNotExist()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new JourneyShareRequestDto { UserIds = new List<Guid> { Guid.NewGuid() } };

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync((Journey)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _journeyServices.ShareJourneyAsync(journeyId, userId, request));
        }
        [Fact]
        public async Task ShareJourneyAsync_ShouldSkipAlreadySharedUsers()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var sharedUserId1 = Guid.NewGuid();
            var sharedUserId2 = Guid.NewGuid();

            var request = new JourneyShareRequestDto
            {
                UserIds = new List<Guid> { sharedUserId1, sharedUserId2 }
            };

            // Journey exists
            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(new Journey { JourneyId = journeyId });

            // Existing shares - user1 already shared, user2 not shared
            var existingShares = new List<JourneyShare>
    {
        new JourneyShare
        {
            JourneyId = journeyId,
            RecievingUserId = sharedUserId1,
            IsRevoked = false
        }
    };

            _journeyShareRepositoryMock
                .Setup(r => r.GetAll())
                .ReturnsAsync(existingShares);

            var addedShares = new List<JourneyShare>();
            var addedAuditLogs = new List<AuditLog>();

            _journeyShareRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<JourneyShare>()))
                .Callback<JourneyShare>(share => addedShares.Add(share))
                .ReturnsAsync(Guid.NewGuid());

            _auditLogRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(log => addedAuditLogs.Add(log))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _journeyServices.ShareJourneyAsync(journeyId, userId, request);

            // Assert
            Assert.Single(result.CreatedShareIds); // Only 1 new share created
            Assert.Single(result.SharedWithUserIds);

            Assert.Contains(sharedUserId2, result.SharedWithUserIds);
            Assert.DoesNotContain(sharedUserId1, result.SharedWithUserIds);

            _journeyShareRepositoryMock.Verify(r => r.AddAsync(It.IsAny<JourneyShare>()), Times.Once);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Once);
        }
        [Fact]
        public async Task ShareJourneyAsync_ShouldReturnEmptyResponse_WhenNoUserIds()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new JourneyShareRequestDto { UserIds = new List<Guid>() };

            _journeyRepositoryMock
                .Setup(r => r.GetAsyncById(journeyId))
                .ReturnsAsync(new Journey { JourneyId = journeyId });

            _journeyShareRepositoryMock
                .Setup(r => r.GetAll())
                .ReturnsAsync(new List<JourneyShare>());

            // Act
            var result = await _journeyServices.ShareJourneyAsync(journeyId, userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.CreatedShareIds);
            Assert.Empty(result.SharedWithUserIds);

            _journeyShareRepositoryMock.Verify(r => r.AddAsync(It.IsAny<JourneyShare>()), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        #endregion
    }
}
