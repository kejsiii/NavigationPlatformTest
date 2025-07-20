using Application.Interfaces;
using Application.Resources;
using AutoMapper;
using Common.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using DTO.DTO.User;
using Presentation.Utilities;

namespace Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly IJWTUtilities _jwt;
        private readonly IJwtBlacklistServices _jwtBlacklistServices;
        public UserServices(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper, IJWTUtilities jwt, IJwtBlacklistServices jwtBlacklistServices)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _jwt = jwt;
            _jwtBlacklistServices = jwtBlacklistServices;
        }

        public async Task<string> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, request.Password);
            if (user == null || user.Password != request.Password)
                throw new UnauthorizedException(StringResourceMessage.InvalidCredentials);
            var role = await _roleRepository.GetAsyncById(user.RoleId);
            var token = _jwt.GenerateToken(user.Id, request.Email,role.Name);
            return token;
        }

        public async Task LogoutAsync(string token)
        {
           await _jwtBlacklistServices.AddToBlacklistAsync(token);
        }

        public async Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request)
        {
            if (await _userRepository.FindByEmailOrUsernameAsync(request.Email, request.Username) != null)
                throw new ConflictException(StringResourceMessage.UserAlreadyExists);
            var role = await _roleRepository.GetRoleByName(request.Role);
            if (role == null)
                throw new NotFoundException(StringResourceMessage.RoleNotFound);
            var oid = await _userRepository.AddAsync(_mapper.Map<User>(request, opt => opt.AfterMap((o, dest) =>
            {
                dest.RoleId = role.Id;
                dest.Status = Status.Active.ToString();
            })));
            return new RegisterResponseDto() { Id = oid };
        }
    }
}
