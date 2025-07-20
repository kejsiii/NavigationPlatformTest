using Application.Services.Messaging;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DailyGoalWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyGoalWorker> _logger;
        private readonly TimeSpan _delay = TimeSpan.FromMinutes(30);

        public DailyGoalWorker(IServiceProvider serviceProvider, ILogger<DailyGoalWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyGoalWorker started at {Time}", DateTimeOffset.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a scope for scoped services (DbContext, Repositories)
                    using var scope = _serviceProvider.CreateScope();

                    var journeyRepo = scope.ServiceProvider.GetRequiredService<IJourneyRepository>();
                    var badgeRepo = scope.ServiceProvider.GetRequiredService<IDailyGoalBadgeRepository>();
                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                    
                    var journeysToday = await journeyRepo.GetJourneysForDateAsync(DateTime.UtcNow.Date);

                    // Group journeys by user to calculate total distance per user
                    var journeysByUser = journeysToday.GroupBy(j => j.UserId);

                    foreach (var userGroup in journeysByUser)
                    {
                        var userId = userGroup.Key;

                        // Check if badge already awarded for today
                        if (await badgeRepo.ExistsForUserOnDate(userId, DateTime.UtcNow.Date))
                        {
                            _logger.LogInformation("User {UserId} already has badge for today", userId);
                            continue;
                        }

                        double runningTotal = 0;
                        Journey triggeringJourney = null;

                        foreach (var journey in userGroup.OrderBy(j => j.ArrivalTime))
                        {
                            runningTotal += (double)journey.RouteDistanceKm;

                            if (runningTotal >= 20.0)
                            {
                                triggeringJourney = journey;
                                break; // Stop at the first journey that pushed us past 20 km
                            }
                        }

                        // If the threshold was hit, award the badge
                        if (triggeringJourney != null)
                        {
                            triggeringJourney.IsDailyGoalAchieved = true;
                            await journeyRepo.UpdateAsync(triggeringJourney);

                            var badge = new DailyGoalBadge
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                Date = DateTime.UtcNow.Date,
                                TotalDistanceKm = runningTotal
                            };
                            await badgeRepo.AddAsync(badge);

                            await eventPublisher.PublishAsync(
                                "DailyGoalAchieved",
                                new DailyGoalAchieved(userId, DateTime.UtcNow.Date),
                                stoppingToken
                            );

                            _logger.LogInformation("Badge awarded to user {UserId} for journey {JourneyId}", userId, triggeringJourney.JourneyId);
                        }
                        else
                        {
                            _logger.LogInformation("User {UserId} has total distance {TotalDistance}, did not meet goal", userId, runningTotal);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DailyGoalWorker");
                }

                await Task.Delay(_delay, stoppingToken);
            }

            _logger.LogInformation("DailyGoalWorker stopping at {Time}", DateTimeOffset.UtcNow);
        }
    }

    public class DailyGoalAchieved
    {
        public Guid UserId { get; }
        public DateTime Date { get; }

        public DailyGoalAchieved(Guid userId, DateTime date)
        {
            UserId = userId;
            Date = date;
        }
    }
}
