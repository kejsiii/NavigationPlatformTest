using Application.Interfaces;
using AutoMapper;
using Common.Exceptions;
using DTO.WebApiDTO.User;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extentions;
using System.IdentityModel.Tokens.Jwt;
using LoginRequestDto = DTO.DTO.User.LoginRequestDto;
using RegisterRequestDto = DTO.DTO.User.RegisterRequestDto;


namespace Presentation.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserServices _userServices;
        private IValidator<RegisterRequestDtoApi> _registerUserValidator;
        private IValidator<LoginRequestDtoApi> _loginUserValidator;
        public UserController(IMapper mapper, IUserServices userServices, 
            IValidator<LoginRequestDtoApi> loginUserValidator,IValidator<RegisterRequestDtoApi> registerUserValidator)
        {
            _mapper = mapper;
            _userServices = userServices;
            _loginUserValidator = loginUserValidator;
            _registerUserValidator = registerUserValidator;
        }

        
        [HttpPost("register")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDtoApi request)
        {
            var result = await _registerUserValidator.ValidateAsync(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                ValidationExtensions.CheckModelState(this.ModelState); result.AddToModelState(ModelState);
            }
            var response = await _userServices.RegisterUserAsync(_mapper.Map<RegisterRequestDto>(request));
            return Ok(_mapper.Map<RegisterResponseDtoApi>(response));
        }

        
        [HttpPost("login")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDtoApi request)
        {
            var result = await _loginUserValidator.ValidateAsync(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                ValidationExtensions.CheckModelState(this.ModelState);
            }
            var response = await _userServices.LoginAsync(_mapper.Map<LoginRequestDto>(request));
            return Ok(response);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                throw new BadRequestException("Authorization token is missing or invalid.");

            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            await _userServices.LogoutAsync(token);

            return NoContent();
        }
    }
}
