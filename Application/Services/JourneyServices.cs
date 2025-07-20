using Application.Interfaces;
using Application.Resources;
using AutoMapper;
using Common.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using DTO.DTO.Journey;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Services
{
    public class JourneyServices : IJourneyServices
    {
        public IJourneyRepository _journeyRepository;
        public IMapper _mapper;
        public IUserRepository _userRepository;
        public IJourneyPublicLinkRepository _journeyPublicLinkRepository;
        public IJourneyShareRepository _journeyShareRepository;
        public IAuditLogRepository _auditLogRepository;
        public JourneyServices(IJourneyRepository journeyRepository, IMapper mapper,IUserRepository userRepository, IJourneyPublicLinkRepository journeyPublicLinkRepository, IJourneyShareRepository journeyShareRepository,
            IAuditLogRepository auditLogRepository) 
        {
            _journeyRepository = journeyRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _journeyPublicLinkRepository = journeyPublicLinkRepository;
            _journeyShareRepository = journeyShareRepository;
            _auditLogRepository = auditLogRepository;
        }
        public async Task<Guid> AddJourneyAsync(AddJourneyRequestDto request)
        {
            if (await _userRepository.GetAsyncById(request.UserId) == null)
                throw new NotFoundException(StringResourceMessage.UserNotFound);
            if (await _journeyRepository.GetJourneyByUserIdAndStartTime(request.UserId, request.StartTime) != null)
                throw new ConflictException(StringResourceMessage.JourneyAlreadyExists);
            var oid = await _journeyRepository.AddAsync(_mapper.Map<Journey>(request, opt => opt.AfterMap((o,dest) => 
            {
                dest.IsDailyGoalAchieved = false;
            })));
            return oid;
        }

        public async Task DeleteJourneyAsync(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsyncById(journeyId);
            if (journey == null)
                throw new NotFoundException(StringResourceMessage.JourneyNotFound);
            var journeyPublicLinks = journey.JourneyPublicLinks;
            if (journeyPublicLinks != null)
               await _journeyPublicLinkRepository.DeleteAll(journeyPublicLinks.ToList());
            var journeyShares = journey.JourneyShares;
            if (journeyShares != null)
                await _journeyShareRepository.DeleteAll(journeyShares.ToList());
            await _journeyRepository.RemoveAsync(journey);
        }

        public async Task<PublicJourneyLinkResponseDto> GeneratePublicLinkAsync(Guid id, Guid userId)
        {
            var journey = await _journeyRepository.GetAsyncById(id);
            if (journey == null)
                throw new NotFoundException(StringResourceMessage.JourneyNotFound);

            // Check for existing non-revoked public link
            var existingLink = await _journeyPublicLinkRepository.GetJourneyPublicLinkRevokedByJourneyId(id);

            if (existingLink != null)
            {
                return new PublicJourneyLinkResponseDto
                {
                    Url = $"/api/journeys/public/{existingLink.Token}",
                    Token = existingLink.Token
                };
            }

            var token = Guid.NewGuid().ToString(); 

            var publicLink = new JourneyPublicLink
            {
                JourneyId = id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _journeyPublicLinkRepository.AddAsync(publicLink);

            return new PublicJourneyLinkResponseDto
            {
                Url = $"/api/journeys/public/{token}",
                Token = token
            };
        }

        public async Task<List<JourneyDto>> GetAllJourneysForUserAsync(Guid userId)
        {
            var response = new List<JourneyDto>();
            if (await _userRepository.GetAsyncById(userId) == null)
                throw new NotFoundException(StringResourceMessage.UserNotFound);
            var journies = _journeyRepository.GetJourneysByUserId(userId).ToList();
            if (journies.Count != 0)
                return _mapper.Map<List<JourneyDto>>(journies);
            else
                return response;
        }

        public async Task<JourneyDto> GetJourneyByIdAsync(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsyncById(journeyId);
            if (journey == null)
                throw new NotFoundException(StringResourceMessage.JourneyNotFound);
            return _mapper.Map<JourneyDto>(journey);
        }

        public async Task<JourneyFilterResponseDto> GetJourniesByFilter(JourneyFilterRequestDto filter)
        {
            var query = _journeyRepository.GetAllJournies().Where(j =>
                (!filter.UserId.HasValue || j.UserId == filter.UserId.Value) &&
                (filter.TransportType == null || !filter.TransportType.Any() || filter.TransportType.Contains(j.TransportationType)) &&
                (!filter.StartDateFrom.HasValue || j.StartTime >= filter.StartDateFrom.Value) &&
                (!filter.ArrivalDateTo.HasValue || j.ArrivalTime <= filter.ArrivalDateTo.Value));

            
            var totalCount = query.Count();

            if (!string.IsNullOrWhiteSpace(filter.OrderBy))
            {
                var property = typeof(Journey).GetProperty(filter.OrderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    bool descending = string.Equals(filter.Direction, "desc", StringComparison.OrdinalIgnoreCase);

                    query = descending
                        ? query.OrderByDescending(j => property.GetValue(j, null))
                        : query.OrderBy(j => property.GetValue(j, null));
                }
                else
                {
                    query = query.OrderBy(j => j.StartTime);
                }
            }
            else
            {
                query = query.OrderBy(j => j.StartTime);
            }

            int skip = (filter.Page - 1) * filter.PageSize;
            var items = query.Skip(skip).Take(filter.PageSize).ToList();

            return new JourneyFilterResponseDto
            {
                Items = _mapper.Map<List<JourneyDto>>(items),
                TotalCount = totalCount
            };
        }

        public async Task<List<MonthlyRouteDistanceResponseDto>> GetMonthlyDistancesAsync(MonthlyRouteDistanceDto request)
        {
            var query = _journeyRepository.GetAllJournies()
                .Where(j =>
                    (!request.UserId.HasValue || j.UserId == request.UserId.Value) &&
                    (!request.Year.HasValue || j.StartTime.Year == request.Year.Value) &&
                    (!request.Month.HasValue || j.StartTime.Month == request.Month.Value));

            var grouped = query
                .GroupBy(j => new { j.UserId, j.StartTime.Year, j.StartTime.Month })
                .Select(g => new MonthlyRouteDistanceResponseDto
                {
                    UserId = g.Key.UserId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalDistanceKm = (double)g.Sum(j => j.RouteDistanceKm)
                });

            grouped = (request.OrderBy?.Trim().ToLower() == "userid")
                ? grouped.OrderBy(g => g.UserId)
                : grouped.OrderByDescending(g => g.TotalDistanceKm);

            int page = request.Page ?? 1;
            int pageSize = request.PageSize ?? 10;

            // Materialize the query into memory here synchronously
            var groupedList = grouped.ToList();

            // Apply pagination in memory
            var pagedResult = groupedList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Return as Task to keep async signature
            return await Task.FromResult(pagedResult);
        }


        public async Task<JourneyPublicLinkDto> GetPublicJourneyByTokenAsync(string token)
        {
            var link = await _journeyPublicLinkRepository.GetPublicLinkByToken(token);
            if (link == null)
                throw new NotFoundException(StringResourceMessage.PublicLinkNotFound);

            var journey = await _journeyRepository.GetAsyncById(link.JourneyId);
            if (journey == null)
                throw new NotFoundException(StringResourceMessage.JourneyNotFound);

            if (link.IsRevoked)
                throw new GoneException(StringResourceMessage.PublicLinkRevoked);

            link.IsRevoked = true;
            await _journeyPublicLinkRepository.UpdateAsync(link);
            return _mapper.Map<JourneyPublicLinkDto>(link);
        }

        public async Task RevokePublicLinkAsync(Guid journeyId, Guid userId)
        {
            var journey = await _journeyRepository.GetAsyncById(journeyId);
            if (journey == null)
                throw new NotFoundException(StringResourceMessage.JourneyNotFound);

            var link = await _journeyPublicLinkRepository.GetJourneyPublicLinkRevokedByJourneyId(journeyId);
            if (link == null)
                throw new NotFoundException(StringResourceMessage.PublicLinkNotFound);

            if (link.IsRevoked)
                throw new GoneException(StringResourceMessage.PublicLinkRevoked);

            link.IsRevoked = true;
            link.RevokedAt = DateTime.UtcNow;
            await _journeyPublicLinkRepository.UpdateAsync(link);

            var audit = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TargetId = link.Id,
                ActionType = "RevokePublicLink",
                Timestamp = DateTime.UtcNow,
                Description = $"User {userId} revoked the public link for journey {journeyId}."
            };

            await _auditLogRepository.AddAsync(audit);
        }

        public async Task<JourneyShareResponseDto> ShareJourneyAsync(Guid journeyId,Guid userId,JourneyShareRequestDto request)
        {
            
            var journey = await _journeyRepository.GetAsyncById(journeyId);
            if (journey == null)
                throw new NotFoundException(StringResourceMessage.JourneyNotFound);

            var response = new JourneyShareResponseDto() { CreatedShareIds = new List<Guid>(), SharedWithUserIds = new List<Guid>()};
            var journeyShared = await _journeyShareRepository.GetAll();

            foreach (var sharedUserId in request.UserIds)
            {
                var alreadyShared = journeyShared.FirstOrDefault(js =>
                    js.JourneyId == journeyId &&
                    js.RecievingUserId == sharedUserId &&
                    !js.IsRevoked);
               if (alreadyShared != null)
                    continue;

                var share = new JourneyShare
                {
                    Id = Guid.NewGuid(),
                    JourneyId = journeyId,
                    SharedByUserId = userId,
                    RecievingUserId = sharedUserId,
                    SharedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                await _journeyShareRepository.AddAsync(share);

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,                
                    TargetId = share.Id, 
                    ActionType = "ShareJourney",
                    Timestamp = DateTime.UtcNow,
                    Description = $"User {userId} shared journey {journeyId} with user {sharedUserId}."
                };
                await _auditLogRepository.AddAsync(auditLog);

                response.CreatedShareIds.Add(share.Id);
                response.SharedWithUserIds.Add(sharedUserId);
            }
            return response;
        }


    }
}
